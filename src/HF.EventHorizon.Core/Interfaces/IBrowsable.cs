namespace HF.EventHorizon.Core.Interfaces;

public interface IBrowsable
{
    List<string> Browse();
    Task<List<string>> BrowseAsync();
}