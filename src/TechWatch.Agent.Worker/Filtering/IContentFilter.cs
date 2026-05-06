using TechWatch.Agent.Worker.Models;

namespace TechWatch.Agent.Worker.Filtering;

public interface IContentFilter
{
    ContentFilterResult Evaluate(TechItem item);
}
