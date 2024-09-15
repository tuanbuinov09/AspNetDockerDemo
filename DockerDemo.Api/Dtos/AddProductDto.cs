namespace DockerDemo.Api.Dtos;

public class AddProductDto
{
    public required string Name { get; set; }

    public decimal? Price { get; set; }

    public required Guid CategoryID { get; set; }
}
