using ApkToAabConverter.Models;

namespace ApkToAabConverter.Services;

/// <summary>
/// Interface para serviço de conversão de APK para AAB
/// </summary>
public interface IApkToAabService
{
    /// <summary>
    /// Converte um arquivo APK para AAB
    /// </summary>
    Task<ConversionResult> ConvertApkToAabAsync(string apkPath, string outputPath, IProgress<string>? progress = null);

    /// <summary>
    /// Assina um arquivo AAB com certificado
    /// </summary>
    Task<ConversionResult> SignAabAsync(string aabPath, CertificateInfo certificate, IProgress<string>? progress = null);

    /// <summary>
    /// Converte e assina um APK em uma única operação
    /// </summary>
    Task<ConversionResult> ConvertAndSignAsync(string apkPath, string outputPath, CertificateInfo certificate, IProgress<string>? progress = null);

    /// <summary>
    /// Verifica se as ferramentas necessárias estão instaladas
    /// </summary>
    Task<bool> VerifyToolsAsync();
}
