using BibliotecaDigital.Data.Context;
using BibliotecaDigital.Data.Repositories;
using BibliotecaDigital.Domain.Interfaces;
using BibliotecaDigital.API.Services;
using BibliotecaDigital.API.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddControllers();

builder.Services.AddDbContext<BibliotecaDigitalContext>(options =>
{
    var dbProvider = builder.Configuration.GetValue<string>("DbProvider");
    
    if (dbProvider == "Oracle")
    {
        var connectionString = builder.Configuration.GetConnectionString("OracleConnection");
        options.UseOracle(connectionString);
        
        if (builder.Environment.IsDevelopment())
        {
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        }
    }
    else
    {
        options.UseInMemoryDatabase("AcervoDigitalDB");
        
        if (builder.Environment.IsDevelopment())
        {
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        }
    }
});

builder.Services.AddScoped<IAutorRepository, AutorRepository>();
builder.Services.AddScoped<ILivroRepository, LivroRepository>();
builder.Services.AddScoped<IEmprestimoRepository, EmprestimoRepository>();

builder.Services.AddScoped<IAzureBlobStorageService, AzureBlobStorageService>();
builder.Services.AddScoped<IOpenLibraryService, OpenLibraryService>();
builder.Services.AddScoped<ExternalApiService>();
builder.Services.AddScoped<LivroEnriquecimentoService>();

builder.Services.AddHttpClient();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Acervo Digital API", 
        Version = "v1",
        Description = "API REST para gestão de acervos digitais - Checkpoint FIAP"
    });
});

var app = builder.Build();

app.UseGlobalExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Acervo Digital API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("🚀 Acervo Digital API iniciada com sucesso!");
logger.LogInformation("📚 Swagger disponível em: http://localhost:5000/swagger");
logger.LogInformation("🗄️  Banco de dados: {DbProvider}", app.Configuration.GetValue<string>("DbProvider"));

app.Run();
