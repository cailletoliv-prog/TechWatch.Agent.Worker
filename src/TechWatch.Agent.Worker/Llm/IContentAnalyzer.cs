using TechWatch.Agent.Worker.Models;

namespace TechWatch.Agent.Worker.Llm;

public interface IContentAnalyzer
{
    Task<AnalysisResult> AnalyzeAsync(
        TechItem item,
        CancellationToken cancellationToken);
}
