var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product Service v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

// Mock data
var products = new List<Product>
{
    new Product { Id = 1, Name = "Laptop", Price = 1500.00m },
    new Product { Id = 2, Name = "Mouse", Price = 50.00m },
    new Product { Id = 3, Name = "Teclado", Price = 150.00m },
    new Product { Id = 4, Name = "Monitor", Price = 400.00m },
    new Product { Id = 5, Name = "Webcam", Price = 200.00m }
};

// Endpoints
app.MapGet("/products", (string? ids) =>
{
    if (string.IsNullOrEmpty(ids))
        return Results.Ok(products);

    var productIds = ids.Split(',')
        .Select(id => int.TryParse(id.Trim(), out var result) ? result : 0)
        .Where(id => id > 0)
        .ToList();

    var filteredProducts = products.Where(p => productIds.Contains(p.Id)).ToList();
    return Results.Ok(filteredProducts);
})
.WithName("GetProducts");

app.Run();

record Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}
