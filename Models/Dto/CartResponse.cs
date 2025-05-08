namespace WEbAPi.Models.Dto
{
    public class CartResponse
    {
        public List<PosOrder> Items { get; set; } = new();
        public decimal TotalSum { get; set; }
    }
}
