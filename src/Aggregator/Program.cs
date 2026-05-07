using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Aggregator Service v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

// Endpoints
app.MapGet("/dashboard-data", async (HttpClient httpClient) =>
    {
        try
        {
            // Parallel calls to all services
            var userTask = httpClient.GetAsync("http://localhost:5003/users/1");
            var ordersTask = httpClient.GetAsync("http://localhost:5004/orders?userId=1");
            var productsTask = httpClient.GetAsync("http://localhost:5005/products?ids=10,11,12");

            await Task.WhenAll(userTask, ordersTask, productsTask);

            var userResponse = await userTask.Result.Content.ReadAsStringAsync();
            var ordersResponse = await ordersTask.Result.Content.ReadAsStringAsync();
            var productsResponse = await productsTask.Result.Content.ReadAsStringAsync();

            return Results.Ok(new
            {
                user = userResponse,
                orders = ordersResponse,
                products = productsResponse
            });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    })
    .WithName("GetDashboardData");

app.Run();

