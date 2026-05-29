using backend.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddControllers();
// builder.Services.AddOpenApi();

var connectionstring = builder.Configuration.GetConnectionString("AppDBConnectionString");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionstring));

var app = builder.Build();
app.MapControllers();
app.Run();

// // Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     // app.MapOpenApi();
// }
