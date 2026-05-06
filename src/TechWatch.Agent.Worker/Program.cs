using TechWatch.Agent.Worker.Configuration;
using TechWatch.Agent.Worker.Filtering;
using TechWatch.Agent.Worker;
using TechWatch.Agent.Worker.Scheduling;
using TechWatch.Agent.Worker.Sources;

var builder = Host.CreateApplicationBuilder(args);
builder.Services
    .AddOptions<TechWatchOptions>()
    .BindConfiguration(TechWatchOptions.SectionName)
    .ValidateDataAnnotations();
builder.Services
    .AddOptions<FilterOptions>()
    .BindConfiguration($"{TechWatchOptions.SectionName}:Filtering")
    .ValidateDataAnnotations();

builder.Services.AddHttpClient();
builder.Services.AddSingleton<IContentFilter, KeywordContentFilter>();
builder.Services.AddSingleton<ISourceReader, RssSourceReader>();
builder.Services.AddSingleton<ISourceAggregator, SourceAggregator>();
builder.Services.AddSingleton<TechWatchPipeline>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
