using System.ComponentModel.DataAnnotations;

namespace BookStore.Model.RequestModels
{
    /// <summary>
    /// This DTO strictly defines what properties a user is allowed to modify.
    /// It prevents malicious users from updating restricted fields like 'TenantId', 'CreatedOn', 
    /// or 'IsDeleted' by injecting them into the JSON payload.
    /// </summary>
    public class UpdateBookRequest
    {
        #region Request Properties & Validation

        // The ID is mandatory for an update operation. We use the Range attribute 
        // to ensure they don't pass an invalid default value like 0.
        [Required(ErrorMessage = "The book ID is required for updating.")]
        [Range(1, int.MaxValue, ErrorMessage = "A valid Book ID must be provided.")]
        public int Id { get; set; }

        [Required(ErrorMessage = "The book title is required.")]
        [MaxLength(100, ErrorMessage = "The title cannot exceed 100 characters.")]
        public string Title { get; set; }

        [MaxLength(32, ErrorMessage = "The ISBN cannot exceed 32 characters.")]
        public string? ISBN { get; set; }

        [Range(0.01, 10000.00, ErrorMessage = "The price must be greater than zero.")]
        public decimal Price { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        #endregion
    }
}