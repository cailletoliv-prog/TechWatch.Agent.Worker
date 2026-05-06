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

## Publish local Windows

Le projet fournit un profil de publish `win-x64` self-contained single-file.

```powershell
dotnet publish .\src\TechWatch.Agent.Worker\TechWatch.Agent.Worker.csproj -p:PublishProfile=win-x64
```

Sortie generee :

```text
artifacts/publish/win-x64/
```

Lancement sans `dotnet run` :

```powershell
.\artifacts\publish\win-x64\TechWatch.Agent.Worker.exe
```

Le publish embarque le runtime .NET et les dependances natives SQLite necessaires. Les chemins relatifs restent resolus depuis le dossier publie par defaut :

- `artifacts/publish/win-x64/data/techwatch.db`
- `artifacts/publish/win-x64/output/digests/yyyy-MM-dd.md`

## Automatisation quotidienne Windows

Le MVP ne contient pas de scheduler interne. Pour lancer le digest tous les jours, utilisez le Planificateur de taches Windows.

1. Ouvrir **Planificateur de taches**.
2. Cliquer sur **Creer une tache...**.
3. Onglet **General** :
   - Nom : `TechWatch Daily Digest`
   - Cocher **Executer meme si l'utilisateur n'est pas connecte** si vous voulez que la tache tourne sans session ouverte.
   - Cocher **Executer avec les autorisations maximales** si necessaire pour acceder au dossier de publish.
4. Onglet **Declencheurs** :
   - Nouveau...
   - Commencer la tache : **Selon un calendrier**
   - Parametres : **Chaque jour**
   - Heure conseillee : `07:00`
5. Onglet **Actions** :
   - Nouveau...
   - Action : **Demarrer un programme**
   - Programme/script :

```text
E:\Applications\TechWatch.Agent.Worker\artifacts\publish\win-x64\TechWatch.Agent.Worker.exe
```

   - Demarrer dans :

```text
E:\Applications\TechWatch.Agent.Worker\artifacts\publish\win-x64
```

6. Onglet **Conditions** :
   - Decochez **Ne demarrer la tache que si l'ordinateur est sur secteur** si vous utilisez un portable et que c'est acceptable.
7. Onglet **Parametres** :
   - Cocher **Executer la tache des que possible apres un demarrage planifie manque**.
   - Conserver **Ne pas demarrer une nouvelle instance** si la tache est deja en cours.

Pour tester, clic droit sur la tache puis **Executer**. Le digest doit etre genere dans le dossier configure et s'ouvrir automatiquement avec l'application Markdown par defaut.

### Ollama au demarrage

TechWatch appelle Ollama localement. Si Ollama n'est pas deja lance au moment de la tache, l'analyse LLM echouera pour les items en attente.

Options simples :

- Installer Ollama avec son demarrage automatique si disponible.
- Ajouter une tache planifiee Windows au demarrage qui lance :

```text
ollama serve
```

- Verifier que le modele configure est present :

```powershell
ollama pull llama3.1
ollama list
```

## Sorties locales

- Base SQLite : `data/techwatch.db`
- Digest Markdown : `output/digests/yyyy-MM-dd.md`

Le mode V1 est `RunOnce`: ingestion, filtrage, stockage, analyse Ollama des items en attente, generation du digest, puis arret de l'application.
