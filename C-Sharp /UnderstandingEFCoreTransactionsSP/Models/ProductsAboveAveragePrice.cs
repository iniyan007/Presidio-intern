using System;
using System.Collections.Generic;

namespace UnderstandingEFCoreTransactionsSP.Models;

public partial class ProductsAboveAveragePrice
{
    public string? Productname { get; set; }

    public decimal? Unitprice { get; set; }
}
