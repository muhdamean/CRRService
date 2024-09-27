using CRRService;
using CRRService.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "CRRService";
});
builder.Services.AddHostedService<CrrService>();
builder.Services.AddScoped<DbService>();
builder.Services.AddScoped<ApiService>();
builder.Services.AddHttpClient();

var host = builder.Build();
host.Run();
