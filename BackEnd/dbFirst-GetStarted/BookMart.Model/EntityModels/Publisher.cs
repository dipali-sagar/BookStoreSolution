using System;
using System.Collections.Generic;

namespace BookMart.Model.EntityModels;

public partial class Publisher
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int EstablishedYear { get; set; }

    public string Headquarters { get; set; } = null!;

    public string WebsiteUrl { get; set; } = null!;

    public string Description { get; set; } = null!;

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
