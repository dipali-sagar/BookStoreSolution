using System;
using System.Collections.Generic;

namespace BookMart.Model.EntityModels;

public partial class Product
{
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string ProductName { get; set; } = null!;

    public string? City { get; set; }

    public string Region { get; set; } = null!;

    public string PostalCode { get; set; } = null!;

    public decimal Price { get; set; }

    public string Isbn { get; set; } = null!;

    public DateTime CreatedOn { get; set; }
}
