using Microsoft.AspNetCore.Mvc;
using Microsoft.AnalysisServices.AdomdClient;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// Load .env file
DotNetEnv.Env.Load();

// Đọc cấu hình từ .env
var ssasDataSource = Environment.GetEnvironmentVariable("SSAS_DATASOURCE");
var ssasCatalog = Environment.GetEnvironmentVariable("SSAS_CATALOG");
var ssasCubeName = Environment.GetEnvironmentVariable("SSAS_CUBE_NAME");
var sqlConnStr = Environment.GetEnvironmentVariable("SQLSERVER_CONNECTION_STRING");

// Cube Connection
builder.Services.AddSingleton(new CubeConfig
{
    DataSource = ssasDataSource!,
    Catalog = ssasCatalog!,
    CubeName = ssasCubeName!
});

// SQL Server Connection
builder.Services.AddSingleton(new SqlDbConfig
{
    ConnectionString = sqlConnStr!
});
builder.Services.AddSingleton<SqlQueryService>();

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
// app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();