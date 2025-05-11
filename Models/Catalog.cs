using System.Text.Json.Serialization;

namespace WEbAPi.Models;

public class Catalog
{
    [JsonPropertyName("catalogsId")]
    public int CatalogsId { get; set; }

    public string Title { get; set; }
    public string Author { get; set; }
    public string Publisher { get; set; }

    [JsonPropertyName("yearPublic")]
    public int YearPublic { get; set; }

    public string Price { get; set; }

    [JsonIgnore]
    public List<Review>? Reviews { get; set; }

    [JsonIgnore]
    public ICollection<Order>? Orders { get; set; }

    public ICollection<Category>? Categories { get; set; }

    [JsonIgnore]
    public ICollection<PosOrder>? PosOrders { get; set; } // Добавьте это свойство
}