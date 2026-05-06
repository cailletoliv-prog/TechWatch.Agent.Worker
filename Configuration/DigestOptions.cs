namespace TechWatch.Agent.Worker.Configuration;

public sealed class DigestOptions
{
    public string OutputDirectory { get; init; } = "output/digests";

    public string FileNameFormat { get; init; } = "yyyy-MM-dd";
}
