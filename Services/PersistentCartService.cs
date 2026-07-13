using CoffeeShopOnline.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace CoffeeShopOnline.Services
{
    public sealed class CartReservation
    {
        public int? TableId { get; set; }
        public int? DinerCount { get; set; }
        public bool ClosedParty { get; set; }
    }

    public sealed class AddToCartResult
    {
        public bool Success { get; private set; }
        public int Count { get; private set; }
        public string Message { get; private set; }

        public static AddToCartResult Completed(int count)
        {
            return new AddToCartResult { Success = true, Count = count };
        }

        public static AddToCartResult Failed(string message, int count)
        {
            return new AddToCartResult { Message = message, Count = count };
        }
    }

    public sealed class PersistentCartService
    {
        private const string CookieName = "gtr_cart";
        private const string RequestItemKey = "CoffeeShopOnline.CartKey";
        private readonly ApplicationDbContext db;
        private readonly HttpContextBase http;
        private readonly string userId;

        public PersistentCartService(ApplicationDbContext dbContext, HttpContextBase httpContext, string currentUserId)
        {
            db = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            http = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
            userId = string.IsNullOrWhiteSpace(currentUserId) ? null : currentUserId;
        }

        public CartReservation GetReservation()
        {
            var draft = FindDraft();
            return draft == null
                ? new CartReservation()
                : new CartReservation
                {
                    TableId = draft.TableId,
                    DinerCount = draft.DinerCount,
                    ClosedParty = draft.ClosedParty
                };
        }

        public void SetReservation(int dinerCount, bool closedParty)
        {
            var draft = GetOrCreateDraft();
            draft.DinerCount = dinerCount;
            draft.ClosedParty = closedParty;
            draft.TableId = null;
            Touch(draft);
            db.SaveChanges();
        }

        public void SelectTable(int tableId)
        {
            var draft = GetOrCreateDraft();
            draft.TableId = tableId;
            Touch(draft);
            db.SaveChanges();
        }

        public List<ShoppingCartModel> GetCart()
        {
            var draft = FindDraft();
            if (draft == null)
            {
                return new List<ShoppingCartModel>();
            }

            var rows = (from line in db.CartDraftLines
                        join item in db.Items on line.ItemId equals item.ItemId
                        where line.CartDraftId == draft.CartDraftId && line.Quantity > 0
                        select new { line.Quantity, Item = item }).ToList();

            return rows.Select(row =>
            {
                var unitPrice = IsPromotionActive(row.Item) ? row.Item.PromoPrice : row.Item.ItemPrice;
                return new ShoppingCartModel
                {
                    ItemId = row.Item.ItemId.ToString(),
                    ImagePath = row.Item.ImagePath,
                    ItemName = row.Item.ItemName,
                    Quantity = row.Quantity,
                    UnitPrice = unitPrice,
                    Total = unitPrice * row.Quantity,
                    Category = row.Item.CatogoryId,
                    TotalQuantity = Math.Max(0, row.Item.Quantity - row.Quantity)
                };
            }).ToList();
        }

        public int GetCount()
        {
            var draft = FindDraft();
            return draft == null
                ? 0
                : db.CartDraftLines.Where(line => line.CartDraftId == draft.CartDraftId).Select(line => (int?)line.Quantity).Sum() ?? 0;
        }

        public AddToCartResult AddItem(Guid itemId)
        {
            using (var transaction = db.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                var product = db.Items.SingleOrDefault(item => item.ItemId == itemId);
                var draft = GetOrCreateDraft();
                var currentCount = db.CartDraftLines
                    .Where(line => line.CartDraftId == draft.CartDraftId)
                    .Select(line => (int?)line.Quantity)
                    .Sum() ?? 0;

                if (product == null || product.Quantity <= 0)
                {
                    transaction.Rollback();
                    return AddToCartResult.Failed("הפריט אינו זמין כרגע.", currentCount);
                }

                var cartLine = db.CartDraftLines.SingleOrDefault(candidate =>
                    candidate.CartDraftId == draft.CartDraftId && candidate.ItemId == itemId);
                var requestedQuantity = cartLine == null ? 1 : cartLine.Quantity + 1;
                if (requestedQuantity > product.Quantity)
                {
                    transaction.Rollback();
                    return AddToCartResult.Failed("אין כמות נוספת במלאי עבור " + product.ItemName + ".", currentCount);
                }

                if (cartLine == null)
                {
                    db.CartDraftLines.Add(new CartDraftLine
                    {
                        CartDraftId = draft.CartDraftId,
                        ItemId = itemId,
                        Quantity = 1
                    });
                }
                else
                {
                    cartLine.Quantity = requestedQuantity;
                }

                Touch(draft);
                db.SaveChanges();
                transaction.Commit();
                return AddToCartResult.Completed(currentCount + 1);
            }
        }

        public void RemoveItem(Guid itemId)
        {
            var draft = FindDraft();
            if (draft == null)
            {
                return;
            }

            var line = db.CartDraftLines.SingleOrDefault(candidate =>
                candidate.CartDraftId == draft.CartDraftId && candidate.ItemId == itemId);
            if (line != null)
            {
                db.CartDraftLines.Remove(line);
                Touch(draft);
                db.SaveChanges();
            }
        }

        public void Clear()
        {
            var draft = FindDraft();
            if (draft == null)
            {
                return;
            }

            db.CartDrafts.Remove(draft);
            db.SaveChanges();
        }

        private CartDraft FindDraft()
        {
            CartDraft draft = null;
            if (userId != null)
            {
                draft = db.CartDrafts.OrderByDescending(candidate => candidate.UpdatedUtc)
                    .FirstOrDefault(candidate => candidate.UserId == userId);
            }

            if (draft == null)
            {
                var cartKey = ReadCartKey();
                if (cartKey != null)
                {
                    draft = db.CartDrafts.SingleOrDefault(candidate => candidate.CartKey == cartKey);
                }
            }

            return draft;
        }

        private CartDraft GetOrCreateDraft()
        {
            var draft = FindDraft();
            if (draft != null)
            {
                if (draft.UserId == null && userId != null)
                {
                    draft.UserId = userId;
                }
                WriteCartCookie(draft.CartKey);
                return draft;
            }

            var cartKey = ReadCartKey() ?? Guid.NewGuid().ToString("N");
            draft = new CartDraft
            {
                CartDraftId = Guid.NewGuid(),
                CartKey = cartKey,
                UserId = userId,
                UpdatedUtc = DateTime.UtcNow
            };
            db.CartDrafts.Add(draft);
            WriteCartCookie(cartKey);
            return draft;
        }

        private string ReadCartKey()
        {
            var requestValue = http.Items[RequestItemKey] as string;
            if (IsValidCartKey(requestValue))
            {
                return requestValue;
            }

            var cookieValue = http.Request.Cookies[CookieName]?.Value;
            if (IsValidCartKey(cookieValue))
            {
                http.Items[RequestItemKey] = cookieValue;
                return cookieValue;
            }
            return null;
        }

        private void WriteCartCookie(string cartKey)
        {
            http.Items[RequestItemKey] = cartKey;
            var cookie = new HttpCookie(CookieName, cartKey)
            {
                HttpOnly = true,
                Secure = http.Request.IsSecureConnection,
                Expires = DateTime.UtcNow.AddDays(30),
                SameSite = SameSiteMode.Lax
            };
            http.Response.Cookies.Set(cookie);
        }

        private void Touch(CartDraft draft)
        {
            draft.UpdatedUtc = DateTime.UtcNow;
            if (userId != null)
            {
                draft.UserId = userId;
            }
        }

        private static bool IsValidCartKey(string value)
        {
            Guid parsed;
            return value != null && value.Length == 32 && Guid.TryParseExact(value, "N", out parsed);
        }

        private static bool IsPromotionActive(Item item)
        {
            return item.Promo && DateTime.Now.Hour > 12 && DateTime.Now.Hour < 17;
        }
    }
}
