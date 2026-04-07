
using Bloom.Main.Modules;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddModules(builder.Configuration);

var app = await builder.Build().UseModules();

app.Run();