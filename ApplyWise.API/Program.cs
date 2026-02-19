using ApplyWise.Infrastructure;
using ApplyWise.Application;
using ApplyWise.Infrastructure.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2. Registrar nossos contratos (DI) "Quando pedirem X, entregue Y"
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()   // Permite qualquer site
              .AllowAnyMethod()   // Permite GET, POST, PUT, DELETE, etc.
              .AllowAnyHeader();  // Permite qualquer cabeçalho (JSON, Auth, etc.)
    });
});

var app = builder.Build();

// --- SEÇÃO DE MIDDLEWARES (Pipeline de Execução) ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseGlobalExceptionHandler();

app.UseCors("AllowAll"); 

app.UseAuthorization();

app.MapControllers();

app.Run();