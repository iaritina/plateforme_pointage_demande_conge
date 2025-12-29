using BackOffice.Context;
using BackOffice.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Ajouter DbContext avec SQL Server
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Ajouter les services
builder.Services.AddScoped<SoldeCongeService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<RegistrationService>();
builder.Services.AddScoped<WorkSummaryService>();
builder.Services.AddScoped<MonitoringService>();

// Configurer CORS pour le frontend Next.js
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000") // URL de ton frontend
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

var app = builder.Build();

// Appliquer automatiquement les migrations à la startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Utiliser CORS avant l'autorisation et les endpoints
app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();