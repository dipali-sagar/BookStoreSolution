namespace BookMart.Model.RequestModels
{
    public class UpdateBookRequest
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ISBN { get; set; }
        public decimal Price { get; set; }
    }
}
