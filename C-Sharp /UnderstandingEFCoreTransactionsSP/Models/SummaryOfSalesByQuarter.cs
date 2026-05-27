using System;
using System.Collections.Generic;

namespace UnderstandingEFCoreTransactionsSP.Models;

public partial class SummaryOfSalesByQuarter
{
    public DateTime? Shippeddate { get; set; }

    public int? Orderid { get; set; }

    public decimal? Subtotal { get; set; }
}
