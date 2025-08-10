using BruSoftware.ListMmf;

namespace BruSoftware.SharedServices;

public interface IProgressReporterFactory
{
    IProgressReport Create(string title);
} 