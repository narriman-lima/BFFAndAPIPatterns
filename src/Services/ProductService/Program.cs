var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

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

app.Use(async (context, next) =>
{
    logger.LogInformation("[PRODUCT SERVICE] Recebendo requisição: {Method} {Path}", context.Request.Method, context.Request.Path);
    await next();
    logger.LogInformation("[PRODUCT SERVICE] Resposta enviada: {StatusCode}", context.Response.StatusCode);
});

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
    {
        logger.LogInformation("[PRODUCT SERVICE] Buscando todos os produtos");
        return Results.Ok(products);
    }

    logger.LogInformation("[PRODUCT SERVICE] Buscando produtos com IDs: {Ids}", ids);
    var productIds = ids.Split(',')
        .Select(id => int.TryParse(id.Trim(), out var result) ? result : 0)
        .Where(id => id > 0)
        .ToList();

    var filteredProducts = products.Where(p => productIds.Contains(p.Id)).ToList();
    logger.LogInformation("[PRODUCT SERVICE] Encontrados {Count} produtos", filteredProducts.Count);
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
