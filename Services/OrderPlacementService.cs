using CoffeeShopOnline.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CoffeeShopOnline.Services
{
    public sealed class OrderPlacementRequest
    {
        public int TableId { get; set; }
        public int DinerCount { get; set; }
        public string UserName { get; set; }
        public bool IsApproved { get; set; }
        public IList<ShoppingCartModel> Cart { get; set; }
    }

    public sealed class OrderPlacementResult
    {
        public bool Success { get; private set; }
        public bool RequiresNewTable { get; private set; }
        public string Message { get; private set; }

        public static OrderPlacementResult Completed()
        {
            return new OrderPlacementResult { Success = true };
        }

        public static OrderPlacementResult Failed(string message, bool requiresNewTable = false)
        {
            return new OrderPlacementResult
            {
                Success = false,
                RequiresNewTable = requiresNewTable,
                Message = message
            };
        }
    }

    public sealed class OrderPlacementService
    {
        private readonly ApplicationDbContext db;

        public OrderPlacementService(ApplicationDbContext dbContext)
        {
            db = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public OrderPlacementResult PlaceOrder(OrderPlacementRequest request)
        {
            if (request == null || request.Cart == null || request.Cart.Count == 0)
            {
                return OrderPlacementResult.Failed("הסל ריק. הוסיפו פריטים לפני שליחת ההזמנה.");
            }
            if (request.DinerCount < 1 || request.DinerCount > 12)
            {
                return OrderPlacementResult.Failed("מספר האורחים אינו תקין.", true);
            }

            var requestedLines = new List<Tuple<ShoppingCartModel, Guid, int>>();
            foreach (var line in request.Cart)
            {
                Guid productId;
                if (line == null || !Guid.TryParse(line.ItemId, out productId) || line.Quantity <= 0 ||
                    decimal.Truncate(line.Quantity) != line.Quantity || line.Quantity > int.MaxValue)
                {
                    return OrderPlacementResult.Failed("אחד הפריטים בסל אינו תקין. רעננו את הסל ונסו שוב.");
                }
                requestedLines.Add(Tuple.Create(line, productId, (int)line.Quantity));
            }

            using (var transaction = db.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    var table = db.RoomTables.SingleOrDefault(candidate => candidate.Id == request.TableId);
                    if (table == null || table.Available || request.DinerCount > table.TableSits)
                    {
                        transaction.Rollback();
                        return OrderPlacementResult.Failed("השולחן כבר אינו זמין או אינו מתאים להרכב. בחרו שולחן אחר.", true);
                    }

                    var productIds = requestedLines.Select(line => line.Item2).Distinct().ToList();
                    var products = db.Items.Where(item => productIds.Contains(item.ItemId)).ToList();
                    if (products.Count != productIds.Count)
                    {
                        transaction.Rollback();
                        return OrderPlacementResult.Failed("אחד הפריטים כבר אינו זמין. רעננו את הסל ונסו שוב.");
                    }

                    foreach (var requestedLine in requestedLines)
                    {
                        var product = products.Single(item => item.ItemId == requestedLine.Item2);
                        if (product.Quantity < requestedLine.Item3)
                        {
                            transaction.Rollback();
                            return OrderPlacementResult.Failed("המלאי השתנה בזמן ההזמנה. עדכנו את הכמויות בסל ונסו שוב.");
                        }
                    }

                    var user = string.IsNullOrWhiteSpace(request.UserName)
                        ? null
                        : db.Users.SingleOrDefault(candidate => candidate.UserName == request.UserName);

                    table.Available = true;
                    table.NumberOfTaken++;
                    var order = new Order
                    {
                        NumberOfDiners = request.DinerCount,
                        TableNumber = table.TableNumber,
                        TableSitTimeEnd = DateTime.Now.AddHours(2),
                        OrderDate = DateTime.Now,
                        OrderNumber = DateTime.Now.ToString("yyyyMMddHHmmssfff") + "-" + Guid.NewGuid().ToString("N").Substring(0, 6),
                        IsAprroved = request.IsApproved
                    };
                    db.Orders.Add(order);

                    var rewardUsed = false;
                    foreach (var requestedLine in requestedLines)
                    {
                        var product = products.Single(item => item.ItemId == requestedLine.Item2);
                        var quantity = requestedLine.Item3;
                        var unitPrice = IsPromotionActive(product) ? product.PromoPrice : product.ItemPrice;
                        var lineTotal = quantity * unitPrice;

                        if (!rewardUsed && user != null && user.stars >= 10 && product.CatogoryId == 1)
                        {
                            lineTotal -= unitPrice;
                            user.stars -= 10;
                            rewardUsed = true;
                        }
                        if (user != null && product.CatogoryId == 1)
                        {
                            user.stars += quantity;
                        }

                        product.Quantity -= quantity;
                        product.popular += quantity;
                        db.OrderDetails.Add(new OrderDetail
                        {
                            Total = lineTotal,
                            ItemId = product.ItemId.ToString(),
                            Order = order,
                            Quantity = quantity,
                            UintPrice = unitPrice
                        });
                    }

                    db.SaveChanges();
                    transaction.Commit();
                    return OrderPlacementResult.Completed();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private static bool IsPromotionActive(Item item)
        {
            return item.Promo && DateTime.Now.Hour > 12 && DateTime.Now.Hour < 17;
        }
    }
}
