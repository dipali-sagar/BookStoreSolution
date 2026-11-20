using System;
using System.Collections.Generic;

namespace BookMart.Model.EntityModels;

public partial class BookAuthorMap
{
    public int BookId { get; set; }

    public int AuthorId { get; set; }

    public int AuthorOrder { get; set; }

    public string? ContributionRole { get; set; }

    public string? Description { get; set; }

    public virtual Author Author { get; set; } = null!;

    public virtual Book Book { get; set; } = null!;
}
