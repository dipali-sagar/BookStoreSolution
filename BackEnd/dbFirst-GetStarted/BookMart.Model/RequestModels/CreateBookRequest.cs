namespace BookMart.Model.RequestModels
{
    public class CreateBookRequest
    {
        public string Title { get; set; }
        public string ISBN { get; set; }
        public decimal Price { get; set; }
    }
}
