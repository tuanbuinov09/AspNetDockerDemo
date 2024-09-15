using System;
using System.Collections.Generic;

namespace DockerDemo.Api.Entities;

public partial class Product
{
    public Guid ID { get; set; }

    public string Name { get; set; } = null!;

    public decimal? Price { get; set; }

    public Guid CategoryID { get; set; }

    public virtual Category Category { get; set; } = null!;
}
