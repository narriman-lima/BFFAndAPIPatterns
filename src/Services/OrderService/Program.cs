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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Service v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

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
        return Results.Ok(orders);

    var userOrders = orders.Where(o => o.UserId == userId).ToList();
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
