namespace TechWatch.Agent.Worker.Models;

public enum TechItemStatus
{
    New = 1,
    FilteredOut = 2,
    PendingAnalysis = 3,
    Analyzed = 4,
    IncludedInDigest = 5,
    AnalysisFailed = 6
}
