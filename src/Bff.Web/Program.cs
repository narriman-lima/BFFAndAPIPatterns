using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BFF Web v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

// Endpoints
app.MapGet("/dashboard", async (HttpClient httpClient) =>
    {
        try
        {
            var response = await httpClient.GetAsync("http://localhost:5002/dashboard-data");
            var content = await response.Content.ReadAsStringAsync();

            using JsonDocument doc = JsonDocument.Parse(content);
            var root = doc.RootElement;
            
            var userRaw = root.GetProperty("user").GetString();
            using JsonDocument userDoc = JsonDocument.Parse(userRaw!);
            var userElement = userDoc.RootElement;

            var userName = userElement.GetProperty("name").GetString();
            var userEmail = userElement.GetProperty("email").GetString();
            
            var ordersRaw = root.GetProperty("orders").GetString();
            var orders = JsonSerializer.Deserialize<List<Order>>(ordersRaw!) ?? new();

            var totalOrders = orders.Count;
            var lastOrders = orders
                .OrderByDescending(o => o.Id)
                .Take(2)
                .ToList();
            
            var productsRaw = root.GetProperty("products").GetString();
            var products = JsonSerializer.Deserialize<List<Product>>(productsRaw!) ?? new();
            
            var dashboard = new
            {
                name = userName,
                email = userEmail,
                totalOrders,
                lastOrders = lastOrders.Select(o => new
                {
                    o.Id,
                    o.UserId,
                    o.ProductId,
                    o.Total
                }),
                highlights = products.Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Price
                })
            };

            return Results.Ok(dashboard);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    })
    .WithName("GetDashboard");

app.Run();

record Order
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("userId")]
    public int UserId { get; set; }

    [JsonPropertyName("productId")]
    public int ProductId { get; set; }

    [JsonPropertyName("total")]
    public decimal Total { get; set; }
}

record Product
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }
}
