using System;
using System.ComponentModel.DataAnnotations;

namespace TravelTourManagement.DataAccess.DTOs.Bookings;

public class ProcessPaymentRequest
{
    [Required]
    [Range(0.01, (double)decimal.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    public string PaymentMethod { get; set; } = null!;

    [Required]
    public string TransactionId { get; set; } = null!;
}
