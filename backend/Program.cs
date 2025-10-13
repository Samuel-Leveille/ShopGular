var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresDb")));

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/", () => "wassup");

app.Run();