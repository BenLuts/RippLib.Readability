using System;

namespace RippLib.Readability.EFExtensions.Tests.DB.Entities;
public class Product
{
    public Product()
    {
        Id = Guid.NewGuid();
    }
    public Product(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; set; }
    public string Name { get; set; } = "Dummy Name";
    public string Description { get; set; } = "Dummy Description";

}
