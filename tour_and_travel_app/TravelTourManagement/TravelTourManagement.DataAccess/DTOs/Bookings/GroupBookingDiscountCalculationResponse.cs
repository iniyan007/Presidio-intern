using System;

namespace TravelTourManagement.DataAccess.DTOs.Bookings;

public class GroupBookingDiscountCalculationResponse
{
    public decimal BaseAmountBeforeGroupDiscount { get; set; }
    public decimal GroupDiscountPercent { get; set; }
    public decimal GroupDiscountAmount { get; set; }
    public decimal BaseAmountAfterGroupDiscount { get; set; }
    public decimal PlatformFeePercent { get; set; }
    public decimal PlatformFeeAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
}
