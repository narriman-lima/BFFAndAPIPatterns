using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

var app = builder.Build();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    logger.LogInformation("[AGGREGATOR] Recebendo requisição: {Method} {Path}", context.Request.Method, context.Request.Path);
    await next();
    logger.LogInformation("[AGGREGATOR] Resposta enviada: {StatusCode}", context.Response.StatusCode);
});

// Endpoints
app.MapGet("/dashboard-data", async (HttpClient httpClient) =>
    {
        logger.LogInformation("[AGGREGATOR] Iniciando agregação de dados dos microserviços");
        try
        {
            // Parallel calls to all services
            logger.LogInformation("[AGGREGATOR] Iniciando chamadas paralelas aos microserviços");
            var userTask = httpClient.GetAsync("http://localhost:5003/users/1");
            var ordersTask = httpClient.GetAsync("http://localhost:5004/orders?userId=1");
            var productsTask = httpClient.GetAsync("http://localhost:5005/products?ids=10,11,12");

            await Task.WhenAll(userTask, ordersTask, productsTask);

            logger.LogInformation("[AGGREGATOR] Chamadas paralelas concluídas");
            var userResponse = await userTask.Result.Content.ReadAsStringAsync();
            var ordersResponse = await ordersTask.Result.Content.ReadAsStringAsync();
            var productsResponse = await productsTask.Result.Content.ReadAsStringAsync();

            logger.LogInformation("[AGGREGATOR] Dados agregados com sucesso");
            return Results.Ok(new
            {
                user = userResponse,
                orders = ordersResponse,
                products = productsResponse
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[AGGREGATOR] Erro ao agregar dados dos microserviços");
            return Results.BadRequest(new { error = ex.Message });
        }
    })
    .WithName("GetDashboardData");

app.Run();

