using System;
using System.Collections.Generic;
using Microsoft.Data.SqlTypes;

namespace BookMart.Model.EntityModels;

public partial class Book
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Isbn { get; set; }

    public decimal? Price { get; set; }

    public string? Description { get; set; }

    public SqlVector<float>? SearchVector { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public int? CreatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedOn { get; set; }

    public Guid TenantId { get; set; }

    public int? PublisherId { get; set; }

    public string? CustomCoverColor { get; set; }

    public bool? IsLimitedEdition { get; set; }

    public int? PromoCodeId { get; set; }

    public virtual ICollection<BookAuthorMap> BookAuthorMaps { get; set; } = new List<BookAuthorMap>();

    public virtual BookDetail? BookDetail { get; set; }

    public virtual Publisher? Publisher { get; set; }
}
