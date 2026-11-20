namespace BookMart.Model.Features
{
    public interface ISoftDeletable
    {
        bool IsDeleted { get; set; }
        DateTime? DeletedOn { get; set; }
        void Undo(); // Optional helper to restore
    }
}
