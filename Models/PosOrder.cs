using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace WEbAPi.Models;

public class PosOrder
{
    public int PosOrderId { get; set; }
    public int? OrderId { get; set; }
    public int? ProductId { get; set; }
    public int Count { get; set; }

    [JsonIgnore] // Чтобы избежать циклических ссылок при сериализации
    public virtual Order? Order { get; set; }

    public virtual Catalog? Product { get; set; } // Теперь Catalog виден
}