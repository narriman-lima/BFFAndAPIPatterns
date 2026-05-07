var builder = WebApplication.CreateBuilder(args);

// Add OpenAPI services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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
    var user = users.FirstOrDefault(u => u.Id == id);
    return user is not null ? Results.Ok(user) : Results.NotFound(new { message = "User not found" });
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
