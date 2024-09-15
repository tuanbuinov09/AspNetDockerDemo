using System;
using System.Collections.Generic;

namespace DockerDemo.Api.Entities;

public partial class Category
{
    public Guid ID { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
