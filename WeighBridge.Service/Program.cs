using WeighBridge.D365.Extensions;
using WeighBridge.Service;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "WeighBridge Sync Service";
});

builder.Services.AddD365Integration(builder.Configuration);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
