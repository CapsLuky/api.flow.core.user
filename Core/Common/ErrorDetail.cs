namespace Core.Common;

public sealed class ErrorDetail(string code, string details, string property)
{
    public string Code { get; } = code;
    public string Details { get; } = details;
    public string Property { get; } = property;
}
