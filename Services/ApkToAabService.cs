using ApkToAabConverter.Models;
using System.Diagnostics;
using System.Text;

namespace ApkToAabConverter.Services;

/// <summary>
/// Serviço para conversão de APK para AAB e assinatura
/// Utiliza bundletool e jarsigner do Android SDK
/// </summary>
public class ApkToAabService : IApkToAabService
{
    private readonly string _bundletoolPath;
    private readonly string _javaPath;
    private readonly string _keytoolPath;
    private readonly string _certificatesPath;

    public ApkToAabService()
    {
        // Configurar caminhos das ferramentas
        _javaPath = GetJavaPath();
        _keytoolPath = GetKeytoolPath();
        _bundletoolPath = Path.Combine(AppContext.BaseDirectory, "Resources", "Tools", "bundletool.jar");
        _certificatesPath = Path.Combine(AppContext.BaseDirectory, "Resources", "Certificates");
    }

    public async Task<ConversionResult> ConvertApkToAabAsync(string apkPath, string outputPath, IProgress<string>? progress = null)
    {
        var result = new ConversionResult();

        try
        {
            if (!File.Exists(apkPath))
            {
                return ConversionResult.CreateError($"APK file not found: {apkPath}");
            }

            progress?.Report("Starting APK to AAB conversion...");

            // Criar diretório de trabalho temporário
            var tempDir = Path.Combine(Path.GetTempPath(), $"apk_to_aab_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);

            try
            {
                // Passo 1: Extrair APK usando bundletool
                progress?.Report("Extracting APK contents...");
                var extractResult = await RunBundletoolCommand(
                    $"build-apks --bundle={outputPath} --mode=universal --apks={apkPath}",
                    progress
                );

                if (!extractResult.Success)
                {
                    return extractResult;
                }

                progress?.Report("Conversion completed successfully!");
                result.Success = true;
                result.Message = "APK converted to AAB successfully";
                result.OutputPath = outputPath;
            }
            finally
            {
                // Limpar diretório temporário
                try
                {
                    if (Directory.Exists(tempDir))
                    {
                        Directory.Delete(tempDir, true);
                    }
                }
                catch { }
            }
        }
        catch (Exception ex)
        {
            return ConversionResult.CreateError($"Error during conversion: {ex.Message}", ex);
        }

        return result;
    }

    public async Task<ConversionResult> SignAabAsync(string aabPath, CertificateInfo certificate, IProgress<string>? progress = null)
    {
        try
        {
            if (!File.Exists(aabPath))
            {
                return ConversionResult.CreateError($"AAB file not found: {aabPath}");
            }

            progress?.Report("Signing AAB file...");

            // Preparar keystore
            string keystorePath;
            if (certificate.IsDefault)
            {
                // Usar certificado padrão do Android
                keystorePath = await PrepareDefaultKeystoreAsync(progress);
            }
            else
            {
                keystorePath = certificate.KeystorePath;
            }

            if (!File.Exists(keystorePath))
            {
                return ConversionResult.CreateError($"Keystore not found: {keystorePath}");
            }

            // Assinar usando jarsigner
            var signArgs = $"-verbose -sigalg SHA256withRSA -digestalg SHA-256 " +
                          $"-keystore \"{keystorePath}\" " +
                          $"-storepass {certificate.KeystorePassword} " +
                          $"-keypass {certificate.KeyPassword} " +
                          $"\"{aabPath}\" {certificate.KeyAlias}";

            var signResult = await RunJarsignerCommand(signArgs, progress);

            if (!signResult.Success)
            {
                return signResult;
            }

            // Verificar assinatura
            progress?.Report("Verifying signature...");
            var verifyArgs = $"-verify -verbose \"{aabPath}\"";
            var verifyResult = await RunJarsignerCommand(verifyArgs, progress);

            if (!verifyResult.Success)
            {
                return ConversionResult.CreateError("Signature verification failed");
            }

            progress?.Report("AAB signed successfully!");
            return ConversionResult.CreateSuccess("AAB signed successfully", aabPath);
        }
        catch (Exception ex)
        {
            return ConversionResult.CreateError($"Error during signing: {ex.Message}", ex);
        }
    }

    public async Task<ConversionResult> ConvertAndSignAsync(string apkPath, string outputPath, CertificateInfo certificate, IProgress<string>? progress = null)
    {
        progress?.Report("Starting APK to AAB conversion and signing...");

        // Converter APK para AAB
        var convertResult = await ConvertApkToAabAsync(apkPath, outputPath, progress);
        if (!convertResult.Success)
        {
            return convertResult;
        }

        // Assinar AAB
        var signResult = await SignAabAsync(outputPath, certificate, progress);
        return signResult;
    }

    public async Task<bool> VerifyToolsAsync()
    {
        try
        {
            // Verificar Java
            var javaCheck = await RunCommandAsync(_javaPath, "-version");
            if (!javaCheck.Success)
            {
                return false;
            }

            // Verificar bundletool
            if (!File.Exists(_bundletoolPath))
            {
                return false;
            }

            var bundletoolCheck = await RunCommandAsync(_javaPath, $"-jar \"{_bundletoolPath}\" version");
            return bundletoolCheck.Success;
        }
        catch
        {
            return false;
        }
    }

    private async Task<ConversionResult> PrepareDefaultKeystoreAsync(IProgress<string>? progress)
    {
        var keystorePath = Path.Combine(_certificatesPath, "debug.keystore");

        if (File.Exists(keystorePath))
        {
            return ConversionResult.CreateSuccess("Using existing keystore", keystorePath);
        }

        progress?.Report("Creating default Android keystore...");

        // Criar keystore usando keytool
        var keytoolArgs = $"-genkey -v -keystore \"{keystorePath}\" " +
                         "-storepass android -alias androiddebugkey -keypass android " +
                         "-keyalg RSA -keysize 2048 -validity 10000 " +
                         "-dname \"CN=Android Debug,O=Android,C=US\"";

        var result = await RunCommandAsync(_keytoolPath, keytoolArgs);
        
        if (!result.Success)
        {
            return ConversionResult.CreateError("Failed to create default keystore");
        }

        return ConversionResult.CreateSuccess("Default keystore created", keystorePath);
    }

    private async Task<ConversionResult> RunBundletoolCommand(string arguments, IProgress<string>? progress)
    {
        return await RunCommandAsync(_javaPath, $"-jar \"{_bundletoolPath}\" {arguments}", progress);
    }

    private async Task<ConversionResult> RunJarsignerCommand(string arguments, IProgress<string>? progress)
    {
        var jarsignerPath = Path.Combine(Path.GetDirectoryName(_javaPath) ?? "", "jarsigner");
        return await RunCommandAsync(jarsignerPath, arguments, progress);
    }

    private async Task<ConversionResult> RunCommandAsync(string command, string arguments, IProgress<string>? progress = null)
    {
        var result = new ConversionResult();

        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processInfo };
            var output = new StringBuilder();
            var error = new StringBuilder();

            process.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    output.AppendLine(e.Data);
                    result.Logs.Add(e.Data);
                    progress?.Report(e.Data);
                }
            };

            process.ErrorDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    error.AppendLine(e.Data);
                    result.Logs.Add($"ERROR: {e.Data}");
                    progress?.Report($"ERROR: {e.Data}");
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            if (process.ExitCode == 0)
            {
                result.Success = true;
                result.Message = "Command executed successfully";
            }
            else
            {
                result.Success = false;
                result.Message = $"Command failed with exit code {process.ExitCode}: {error}";
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = $"Error executing command: {ex.Message}";
            result.Error = ex;
        }

        return result;
    }

    private string GetJavaPath()
    {
        // Tentar encontrar Java no sistema
        var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
        if (!string.IsNullOrEmpty(javaHome))
        {
            var javaPath = Path.Combine(javaHome, "bin", "java");
            if (File.Exists(javaPath))
            {
                return javaPath;
            }
        }

        // Tentar caminho padrão no macOS
        return "/usr/bin/java";
    }

    private string GetKeytoolPath()
    {
        var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
        if (!string.IsNullOrEmpty(javaHome))
        {
            var keytoolPath = Path.Combine(javaHome, "bin", "keytool");
            if (File.Exists(keytoolPath))
            {
                return keytoolPath;
            }
        }

        return "/usr/bin/keytool";
    }
}
