using TechWatch.Agent.Worker.Configuration;
using TechWatch.Agent.Worker.Filtering;
using TechWatch.Agent.Worker.Llm;
using TechWatch.Agent.Worker;
using TechWatch.Agent.Worker.Scheduling;
using TechWatch.Agent.Worker.Sources;
using TechWatch.Agent.Worker.Storage;

var builder = Host.CreateApplicationBuilder(args);
builder.Services
    .AddOptions<TechWatchOptions>()
    .BindConfiguration(TechWatchOptions.SectionName)
    .ValidateDataAnnotations();
builder.Services
    .AddOptions<FilterOptions>()
    .BindConfiguration($"{TechWatchOptions.SectionName}:Filtering")
    .ValidateDataAnnotations();
builder.Services
    .AddOptions<StorageOptions>()
    .BindConfiguration($"{TechWatchOptions.SectionName}:Storage")
    .ValidateDataAnnotations();
builder.Services
    .AddOptions<OllamaOptions>()
    .BindConfiguration($"{TechWatchOptions.SectionName}:Ollama")
    .ValidateDataAnnotations();

builder.Services.AddHttpClient();
builder.Services.AddSingleton<IContentFilter, KeywordContentFilter>();
builder.Services.AddSingleton<ISourceReader, RssSourceReader>();
builder.Services.AddSingleton<ISourceAggregator, SourceAggregator>();
builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.AddSingleton<ITechItemRepository, SqliteTechItemRepository>();
builder.Services.AddSingleton<IOllamaClient, OllamaClient>();
builder.Services.AddSingleton<IContentAnalyzer, OllamaContentAnalyzer>();
builder.Services.AddSingleton<TechWatchPipeline>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
