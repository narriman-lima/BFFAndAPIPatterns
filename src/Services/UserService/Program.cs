var builder = WebApplication.CreateBuilder(args);

// Add OpenAPI services
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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "User Service v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    logger.LogInformation("[USER SERVICE] Recebendo requisição: {Method} {Path}", context.Request.Method, context.Request.Path);
    await next();
    logger.LogInformation("[USER SERVICE] Resposta enviada: {StatusCode}", context.Response.StatusCode);
});

// Mock data
var users = new List<User>
{
    new User { Id = 1, Name = "João Silva", Email = "joao@example.com" },
    new User { Id = 2, Name = "Maria Santos", Email = "maria@example.com" },
    new User { Id = 3, Name = "Pedro Oliveira", Email = "pedro@example.com" }
};

// Endpoints
app.MapGet("/users/{id}", (int id) =>
{
    logger.LogInformation("[USER SERVICE] Buscando usuário com ID: {UserId}", id);
    var user = users.FirstOrDefault(u => u.Id == id);
    if (user is not null)
    {
        logger.LogInformation("[USER SERVICE] Usuário encontrado: {UserName}", user.Name);
        return Results.Ok(user);
    }
    logger.LogWarning("[USER SERVICE] Usuário não encontrado com ID: {UserId}", id);
    return Results.NotFound(new { message = "User not found" });
})
.WithName("GetUser")
.WithDescription("Get a user by ID");

app.Run();

record User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}
