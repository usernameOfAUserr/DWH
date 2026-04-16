using DWH.Application.Helpers;
using DWH.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServices();
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.Configure<DestatisOptions>(builder.Configuration.GetSection("Destatis"));

var app = builder.Build();

app.UseCors("AllowAll");

app.UseRouting();

app.MapControllers();

app.Run();