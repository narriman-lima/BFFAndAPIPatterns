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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Service v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    logger.LogInformation("[ORDER SERVICE] Recebendo requisição: {Method} {Path}", context.Request.Method, context.Request.Path);
    await next();
    logger.LogInformation("[ORDER SERVICE] Resposta enviada: {StatusCode}", context.Response.StatusCode);
});

// Mock data
var orders = new List<Order>
{
    new Order { Id = 1, UserId = 1, ProductId = 10, Total = 150.50m },
    new Order { Id = 2, UserId = 1, ProductId = 11, Total = 299.99m },
    new Order { Id = 3, UserId = 2, ProductId = 10, Total = 150.50m },
    new Order { Id = 4, UserId = 3, ProductId = 12, Total = 89.99m },
    new Order { Id = 5, UserId = 2, ProductId = 12, Total = 89.99m }
};

// Endpoints
app.MapGet("/orders", (int? userId) =>
{
    if (userId is null)
    {
        logger.LogInformation("[ORDER SERVICE] Buscando todos os pedidos");
        return Results.Ok(orders);
    }

    logger.LogInformation("[ORDER SERVICE] Buscando pedidos do usuário: {UserId}", userId);
    var userOrders = orders.Where(o => o.UserId == userId).ToList();
    logger.LogInformation("[ORDER SERVICE] Encontrados {Count} pedidos para o usuário {UserId}", userOrders.Count, userId);
    return Results.Ok(userOrders);
})
.WithName("GetOrders");

app.Run();

record Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public decimal Total { get; set; }
}
