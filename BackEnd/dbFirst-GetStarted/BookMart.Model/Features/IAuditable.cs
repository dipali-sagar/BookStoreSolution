namespace BookMart.Model.Features
{
    public interface IAuditable
    {
        DateTime CreatedOn { get; set; }
        DateTime UpdatedOn { get; set; }
        public int? CreatedBy { get; set; }
    }
}
