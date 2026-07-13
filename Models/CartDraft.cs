using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeeShopOnline.Models
{
    public class CartDraft
    {
        public CartDraft()
        {
            Lines = new HashSet<CartDraftLine>();
        }

        [Key]
        public Guid CartDraftId { get; set; }

        [Required, StringLength(32), Index("IX_CartDraft_CartKey", IsUnique = true)]
        public string CartKey { get; set; }

        [StringLength(128), Index("IX_CartDraft_UserId")]
        public string UserId { get; set; }

        public int? TableId { get; set; }

        public int? DinerCount { get; set; }

        public bool ClosedParty { get; set; }

        public DateTime UpdatedUtc { get; set; }

        public virtual ICollection<CartDraftLine> Lines { get; set; }
    }

    public class CartDraftLine
    {
        public int CartDraftLineId { get; set; }

        [Index("IX_CartDraftLine_Item", 1, IsUnique = true)]
        public Guid CartDraftId { get; set; }

        [Index("IX_CartDraftLine_Item", 2, IsUnique = true)]
        public Guid ItemId { get; set; }

        public int Quantity { get; set; }

        [ForeignKey("CartDraftId")]
        public virtual CartDraft CartDraft { get; set; }
    }
}
