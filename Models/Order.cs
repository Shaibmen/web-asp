using System.Text.Json.Serialization;

namespace WEbAPi.Models;

public class Order
{
    [JsonPropertyName("ordersId")]
    public int OrdersId { get; set; }

    [JsonPropertyName("usersId")]
    public int? UsersId { get; set; }

    [JsonPropertyName("catalogsId")]
    public int? CatalogsId { get; set; }

    [JsonPropertyName("totalSum")]
    public decimal TotalSum { get; set; }

    public virtual Catalog? Catalogs { get; set; }

    [JsonIgnore]
    public virtual ICollection<PosOrder>? PosOrders { get; set; } = new List<PosOrder>();

    public virtual User? Users { get; set; }
}