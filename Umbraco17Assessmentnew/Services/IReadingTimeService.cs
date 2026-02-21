namespace Umbraco17Assessmentnew.Services;


public interface IReadingTimeService
{
  
    int Calculate(string? text);
    string GetLabel(string? text);
}
