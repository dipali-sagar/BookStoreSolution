using System;
using System.Collections.Generic;

namespace BookMart.Model.EntityModels;

public partial class Author
{
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Expertise { get; set; } = null!;

    public string Biography { get; set; } = null!;

    public string WebsiteUrl { get; set; } = null!;

    public virtual ICollection<BookAuthorMap> BookAuthorMaps { get; set; } = new List<BookAuthorMap>();
}
