using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CoffeeShopOnline.Models
{
    public sealed class RoomTableDashboardViewModel
    {
        public RoomTableDashboardViewModel()
        {
            Tables = new List<RoomTableDashboardItemViewModel>();
        }

        public IList<RoomTableDashboardItemViewModel> Tables { get; set; }
        public int FreeCount { get; set; }
        public int OccupiedCount { get; set; }
        public int RequiresReleaseCount { get; set; }
        public int InsideCount { get; set; }
        public int OutsideCount { get; set; }
        public int TotalSeats { get; set; }
    }

    public sealed class RoomTableDashboardItemViewModel
    {
        public int Id { get; set; }
        public int TableNumber { get; set; }
        public int TableSeats { get; set; }
        public bool IsInside { get; set; }
        public bool IsOccupied { get; set; }
        public bool RequiresRelease { get; set; }
        public int TimesUsed { get; set; }
        public int? ActiveDiners { get; set; }
        public DateTime? ActiveUntil { get; set; }
        public string ActiveOrderNumber { get; set; }
    }

    public sealed class RoomTableCreateViewModel
    {
        [Required]
        [Range(1, 12, ErrorMessage = "מספר המושבים חייב להיות בין 1 ל־12.")]
        [Display(Name = "מספר מושבים")]
        public int TableSeats { get; set; }

        [Display(Name = "אזור הישיבה")]
        public bool IsInside { get; set; }
    }

    public sealed class RoomTableEditViewModel
    {
        public int Id { get; set; }
        public int TableNumber { get; set; }
        public bool IsInside { get; set; }
        public bool IsOccupied { get; set; }
        public int? ActiveDiners { get; set; }

        [Required]
        [Range(1, 12, ErrorMessage = "מספר המושבים חייב להיות בין 1 ל־12.")]
        [Display(Name = "מספר מושבים")]
        public int TableSeats { get; set; }
    }
}
