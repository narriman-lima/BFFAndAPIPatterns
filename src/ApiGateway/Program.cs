var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gateway v1");
        c.RoutePrefix = string.Empty;
    });
}

app.Use(async (context, next) =>
{
    logger.LogInformation("[API GATEWAY] Recebendo requisição: {Method} {Path}", context.Request.Method, context.Request.Path);
    await next();
    logger.LogInformation("[API GATEWAY] Resposta enviada: {StatusCode}", context.Response.StatusCode);
});

app.UseRouting();

// API Gateway Endpoints - Calls BFF
app.MapGet("/dashboard/web", async (HttpClient httpClient, ILogger<Program> logger) =>
{
    logger.LogInformation("[API GATEWAY] Chamando BFF para dashboard web");
    try
    {
        var response = await httpClient.GetAsync("http://localhost:5001/dashboard/web");
        var content = await response.Content.ReadAsStringAsync();
        return Results.Content(content, "application/json");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "[API GATEWAY] Erro ao chamar BFF");
        return Results.Problem("Erro ao comunicar com BFF", statusCode: 502);
    }
})
.WithName("GetDashboardWeb");

app.MapGet("/dashboard/mobile", async (HttpClient httpClient, ILogger<Program> logger) =>
{
    logger.LogInformation("[API GATEWAY] Chamando BFF para dashboard mobile");
    try
    {
        var response = await httpClient.GetAsync("http://localhost:5001/dashboard/mobile");
        var content = await response.Content.ReadAsStringAsync();
        return Results.Content(content, "application/json");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "[API GATEWAY] Erro ao chamar BFF");
        return Results.Problem("Erro ao comunicar com BFF", statusCode: 502);
    }
})
.WithName("GetDashboardMobile");

// Gateway Management Endpoints
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    service = "API Gateway"
}))
.WithName("HealthCheck");

app.MapGet("/gateway/info", () => Results.Ok(new
{
    name = "API Gateway",
    version = "1.0.0",
    description = "API Gateway - Calls BFF explicitly",
    endpoints = new[]
    {
        new { path = "/dashboard/web", description = "Web dashboard (calls BFF)" },
        new { path = "/dashboard/mobile", description = "Mobile dashboard (calls BFF)" }
    }
}))
.WithName("GatewayInfo");

app.MapGet("/gateway/status", () => Results.Ok(new
{
    uptime = Environment.TickCount64,
    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
    machineName = Environment.MachineName
}))
.WithName("GatewayStatus");

app.Run();

