using System;
using System.Collections.Generic;

namespace BookMart.Model.EntityModels;

public partial class BookDetail
{
    public int BookId { get; set; }

    public int NumberOfPages { get; set; }

    public int NumberOfChapters { get; set; }

    public string? Format { get; set; }

    public string? Language { get; set; }

    public int? ItemWeightInGrams { get; set; }

    public virtual Book Book { get; set; } = null!;
}
