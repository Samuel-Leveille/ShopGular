using Microsoft.EntityFrameworkCore;
using ShopGular.Backend;
using ShopGular.Backend.Services;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresDb")));

builder.Services.AddControllers();

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<SellerService>();
builder.Services.AddScoped<ClientService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost",
        policy => policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

app.UseHttpsRedirection();

app.UseCors("AllowLocalhost");

app.MapControllers();

app.Run();