using System.ComponentModel.DataAnnotations;

namespace BookStore.Model.RequestModels
{
    public class CreateBookRequest
    {
        #region Base Book Properties
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = null!;
        public string? ISBN { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        #endregion

        #region Publisher Relation
        public int? ExistingPublisherId { get; set; }
        public CreatePublisherRequest? NewPublisher { get; set; }
        #endregion

        #region Book Details Relation
        public CreateBookDetailsRequest? BookDetails { get; set; }
        #endregion

        #region Author Relations with Explicit Mapping Payload

        // Links existing authors while supplying the join-table payload
        public List<ExistingAuthorMappingRequest>? ExistingAuthorMappings { get; set; }

        // Creates new authors while supplying the join-table payload
        public List<NewAuthorMappingRequest>? NewAuthorMappings { get; set; }

        #endregion
    }

    #region Author Mapping DTOs

    public class ExistingAuthorMappingRequest
    {
        [Required] public int AuthorId { get; set; }
        public int AuthorOrder { get; set; }
        public string? ContributionRole { get; set; }
        public string? Description { get; set; }
    }

    public class NewAuthorMappingRequest
    {
        [Required] public CreateAuthorRequest Author { get; set; } = null!;
        public int AuthorOrder { get; set; }
        public string? ContributionRole { get; set; }
        public string? Description { get; set; }
    }

    #endregion

    #region Standard DTOs

    public class CreatePublisherRequest
    {
        [Required] public string Name { get; set; } = null!;
        public int EstablishedYear { get; set; }
        public string? Headquarters { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? Description { get; set; }
    }

    public class CreateAuthorRequest
    {
        [Required] public string FirstName { get; set; } = null!;
        [Required] public string LastName { get; set; } = null!;
        public string? Email { get; set; }
        public string? Expertise { get; set; }
        public string? Biography { get; set; }
        public string? WebsiteUrl { get; set; }
    }

    public class CreateBookDetailsRequest
    {
        [Required] public int NumberOfPages { get; set; }
        [Required] public int NumberOfChapters { get; set; }
        public string? Format { get; set; }
        public string? Language { get; set; }
        public int? ItemWeightInGrams { get; set; }
    }

    #endregion
}