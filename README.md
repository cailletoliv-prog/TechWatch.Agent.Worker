# TechWatch Agent Worker

MVP local-first de veille technologique pour C#/.NET, ASP.NET Core, EF Core, Oracle, Angular et IA appliquee au developpement.

## Prerequis

- .NET 8 SDK ou plus recent.
- Ollama installe et lance localement.
- Modele conseille :

```powershell
ollama pull llama3.1
ollama serve
```

Par defaut, l'application utilise `http://localhost:11434` et le modele `llama3.1`.

## Configuration

La configuration principale est dans `src/TechWatch.Agent.Worker/appsettings.json`.

En developpement, `appsettings.Development.json` explicite :

- `TechWatch:RunOnce`
- `TechWatch:Ollama`
- `TechWatch:Storage`
- `TechWatch:Digest`

Les dossiers `data/` et `output/digests/` sont crees automatiquement au premier run.

## Commandes

```powershell
dotnet restore .\TechWatch.Agent.Worker.slnx
dotnet build .\TechWatch.Agent.Worker.slnx
dotnet test .\TechWatch.Agent.Worker.slnx
dotnet run --project .\src\TechWatch.Agent.Worker\TechWatch.Agent.Worker.csproj
```

## Sorties locales

- Base SQLite : `data/techwatch.db`
- Digest Markdown : `output/digests/yyyy-MM-dd.md`

Le mode V1 est `RunOnce`: ingestion, filtrage, stockage, analyse Ollama des items en attente, generation du digest, puis arret de l'application.
