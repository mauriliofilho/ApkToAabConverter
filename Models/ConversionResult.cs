namespace ApkToAabConverter.Models;

/// <summary>
/// Resultado de uma operação de conversão ou assinatura
/// </summary>
public class ConversionResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? OutputPath { get; set; }
    public List<string> Logs { get; set; } = new();
    public Exception? Error { get; set; }

    public static ConversionResult CreateSuccess(string message, string outputPath)
    {
        return new ConversionResult
        {
            Success = true,
            Message = message,
            OutputPath = outputPath
        };
    }

    public static ConversionResult CreateError(string message, Exception? error = null)
    {
        return new ConversionResult
        {
            Success = false,
            Message = message,
            Error = error
        };
    }
}
