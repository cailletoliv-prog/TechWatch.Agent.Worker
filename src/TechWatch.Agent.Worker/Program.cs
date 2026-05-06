using TechWatch.Agent.Worker.Configuration;
using TechWatch.Agent.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services
    .AddOptions<TechWatchOptions>()
    .BindConfiguration(TechWatchOptions.SectionName)
    .ValidateDataAnnotations();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
