using Grpc.Server.Context;
using Grpc.Server.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var config = builder.Configuration;

services.AddDbContext<AppDbContext>(
    options => options.UseSqlite(
        config.GetConnectionString("DB")
        )
    );

services
    .AddGrpc()
    .AddJsonTranscoding();

services.AddGrpcReflection();

var app = builder.Build();

app.MapGrpcService<ToDoService>();

IWebHostEnvironment env = app.Environment;

if (env.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

app.Run();