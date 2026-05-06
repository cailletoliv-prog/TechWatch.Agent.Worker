using TechWatch.Agent.Worker.Models;

namespace TechWatch.Agent.Worker.Filtering;

public interface IContentFilter
{
    bool IsRelevant(TechItem item);
}
