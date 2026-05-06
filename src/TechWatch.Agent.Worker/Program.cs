using TechWatch.Agent.Worker.Configuration;
using TechWatch.Agent.Worker.Filtering;
using TechWatch.Agent.Worker;
using TechWatch.Agent.Worker.Scheduling;

var builder = Host.CreateApplicationBuilder(args);
builder.Services
    .AddOptions<TechWatchOptions>()
    .BindConfiguration(TechWatchOptions.SectionName)
    .ValidateDataAnnotations();
builder.Services
    .AddOptions<FilterOptions>()
    .BindConfiguration($"{TechWatchOptions.SectionName}:Filtering")
    .ValidateDataAnnotations();

builder.Services.AddSingleton<IContentFilter, KeywordContentFilter>();
builder.Services.AddSingleton<TechWatchPipeline>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
