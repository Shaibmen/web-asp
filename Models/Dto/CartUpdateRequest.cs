namespace API_ASP.Models.Requests
{
    public class CartUpdateRequest
    {
        public int PosOrderId { get; set; }
        public int NewCount { get; set; }
    }
}
