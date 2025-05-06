using System;
using System.Collections.Generic;

namespace WEbAPi.Models;

public class Catalog
{
    public int CatalogsId { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public string Publisher { get; set; }
    public int YearPublic { get; set; }
    public string Price { get; set; }
    public ICollection<Order> Orders { get; set; }
    public ICollection<Category> Categories { get; set; }
}