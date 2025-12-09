using ApkToAabConverter.Models;
using System.Diagnostics;
using System.Text;
using System.IO.Compression;

namespace ApkToAabConverter.Services;

/// <summary>
/// Serviço para conversão de APK para AAB e assinatura
/// Converte manualmente criando a estrutura AAB correta (SEM bundletool - como a implementação Swift)
/// </summary>
public class ApkToAabService : IApkToAabService
{
    private readonly string _jarsignerPath;
    private readonly string _keytoolPath;
    private readonly string _debugKeystorePath;

    public ApkToAabService()
    {
        // Configurar caminhos das ferramentas
        _jarsignerPath = FindJarsigner();
        _keytoolPath = FindKeytool();
        
        // Caminho do debug keystore do Android
        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        _debugKeystorePath = Path.Combine(homeDir, ".android", "debug.keystore");
    }

    public async Task<ConversionResult> ConvertApkToAabAsync(string apkPath, string outputPath, IProgress<string>? progress = null)
    {
        var result = new ConversionResult();

        try
        {
            // Validar entrada
            if (!File.Exists(apkPath))
            {
                return ConversionResult.CreateError($"APK file not found: {apkPath}");
            }

            progress?.Report("Starting APK to AAB conversion...");
            progress?.Report($"APK: {Path.GetFileName(apkPath)}");

            // Criar diretório temporário
            var tempDir = Path.Combine(Path.GetTempPath(), $"apktoaab_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);

            try
            {
                // Passo 1: Extrair APK (30%)
                progress?.Report("Extracting APK contents...");
                var extractedDir = Path.Combine(tempDir, "extracted");
                Directory.CreateDirectory(extractedDir);
                await ExtractApkAsync(apkPath, extractedDir, progress);

                // Passo 2: Preparar estrutura AAB (50%)
                progress?.Report("Preparing AAB structure...");
                var aabDir = Path.Combine(tempDir, "aab_bundle");
                Directory.CreateDirectory(aabDir);

                // Passo 3: Criar módulo base (65%)
                progress?.Report("Creating base module...");
                var baseModuleDir = Path.Combine(aabDir, "base");
                CreateBaseModule(extractedDir, baseModuleDir, progress);

                // Passo 4: Criar BundleConfig (70%)
                progress?.Report("Generating bundle configuration...");
                CreateBundleConfig(aabDir);

                // Passo 5: Empacotar AAB (80%)
                progress?.Report("Packaging AAB file...");
                var unsignedAabPath = Path.Combine(tempDir, "unsigned.aab");
                PackageAab(aabDir, unsignedAabPath);

                // Passo 6: Copiar para destino final (não assinado ainda)
                progress?.Report("Finalizing...");
                File.Copy(unsignedAabPath, outputPath, true);

                progress?.Report("Conversion completed successfully!");
                result.Success = true;
                result.Message = "APK converted to AAB successfully (unsigned)";
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
                catch (Exception ex)
                {
                    progress?.Report($"Warning: Failed to clean temp directory: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            return ConversionResult.CreateError($"Error during conversion: {ex.Message}", ex);
        }

        return result;
    }

    private async Task ExtractApkAsync(string apkPath, string destination, IProgress<string>? progress)
    {
        progress?.Report("Unzipping APK file...");
        
        // APK é apenas um arquivo ZIP, extrair usando unzip
        var processInfo = new ProcessStartInfo
        {
            FileName = "/usr/bin/unzip",
            Arguments = $"-q \"{apkPath}\" -d \"{destination}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(processInfo);
        if (process == null)
        {
            throw new Exception("Failed to start unzip process");
        }

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync();
            throw new Exception($"Failed to extract APK: {error}");
        }

        progress?.Report($"Extracted {Directory.GetFiles(destination, "*", SearchOption.AllDirectories).Length} files");
    }

    private void CreateBaseModule(string extractedDir, string baseModuleDir, IProgress<string>? progress)
    {
        progress?.Report("Setting up module structure...");
        
        // Criar estrutura de diretórios do módulo base
        var manifestDir = Path.Combine(baseModuleDir, "manifest");
        var dexDir = Path.Combine(baseModuleDir, "dex");
        var resDir = Path.Combine(baseModuleDir, "res");
        var rootDir = Path.Combine(baseModuleDir, "root");

        Directory.CreateDirectory(manifestDir);
        Directory.CreateDirectory(dexDir);
        Directory.CreateDirectory(resDir);
        Directory.CreateDirectory(rootDir);

        // Copiar AndroidManifest.xml
        var manifestSource = Path.Combine(extractedDir, "AndroidManifest.xml");
        var manifestDest = Path.Combine(manifestDir, "AndroidManifest.xml");
        if (File.Exists(manifestSource))
        {
            File.Copy(manifestSource, manifestDest, true);
            progress?.Report("✓ Copied AndroidManifest.xml");
        }

        // Copiar arquivos DEX
        var dexFiles = Directory.GetFiles(extractedDir, "*.dex", SearchOption.TopDirectoryOnly);
        foreach (var dexFile in dexFiles)
        {
            var destFile = Path.Combine(dexDir, Path.GetFileName(dexFile));
            File.Copy(dexFile, destFile, true);
        }
        if (dexFiles.Length > 0)
        {
            progress?.Report($"✓ Copied {dexFiles.Length} DEX file(s)");
        }

        // Copiar diretório res
        var resSource = Path.Combine(extractedDir, "res");
        if (Directory.Exists(resSource))
        {
            CopyDirectory(resSource, resDir);
            progress?.Report("✓ Copied resources");
        }

        // Copiar lib para root
        var libSource = Path.Combine(extractedDir, "lib");
        if (Directory.Exists(libSource))
        {
            var libDest = Path.Combine(rootDir, "lib");
            CopyDirectory(libSource, libDest);
            progress?.Report("✓ Copied native libraries");
        }

        // Copiar assets para root
        var assetsSource = Path.Combine(extractedDir, "assets");
        if (Directory.Exists(assetsSource))
        {
            var assetsDest = Path.Combine(rootDir, "assets");
            CopyDirectory(assetsSource, assetsDest);
            progress?.Report("✓ Copied assets");
        }

        // Copiar resources.arsc para root
        var resourcesSource = Path.Combine(extractedDir, "resources.arsc");
        if (File.Exists(resourcesSource))
        {
            var resourcesDest = Path.Combine(rootDir, "resources.arsc");
            File.Copy(resourcesSource, resourcesDest, true);
            progress?.Report("✓ Copied resources.arsc");
        }

        // Copiar META-INF para root se existir
        var metaInfSource = Path.Combine(extractedDir, "META-INF");
        if (Directory.Exists(metaInfSource))
        {
            var metaInfDest = Path.Combine(rootDir, "META-INF");
            CopyDirectory(metaInfSource, metaInfDest);
            progress?.Report("✓ Copied META-INF");
        }
    }

    private void CreateBundleConfig(string aabDir)
    {
        var bundleConfig = @"{
  ""bundletool"": {
    ""version"": ""1.15.6""
  },
  ""optimizations"": {
    ""splitsConfig"": {
      ""splitDimension"": [
        {
          ""value"": ""ABI"",
          ""negate"": false
        },
        {
          ""value"": ""SCREEN_DENSITY"",
          ""negate"": false
        },
        {
          ""value"": ""LANGUAGE"",
          ""negate"": false
        }
      ]
    },
    ""uncompressNativeLibraries"": {
      ""enabled"": true
    }
  },
  ""compression"": {
    ""uncompressedGlob"": [""**.so"", ""**.dex""]
  }
}";

        var configPath = Path.Combine(aabDir, "BundleConfig.pb.json");
        File.WriteAllText(configPath, bundleConfig);
    }

    private void PackageAab(string aabDir, string outputPath)
    {
        // Criar um arquivo ZIP da estrutura do AAB
        // AAB é essencialmente um ZIP com estrutura específica
        if (File.Exists(outputPath))
        {
            File.Delete(outputPath);
        }

        ZipFile.CreateFromDirectory(aabDir, outputPath, CompressionLevel.Optimal, false);
    }

    private void CopyDirectory(string sourceDir, string destDir)
    {
        if (!Directory.Exists(destDir))
        {
            Directory.CreateDirectory(destDir);
        }

        // Copiar arquivos
        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var destFile = Path.Combine(destDir, Path.GetFileName(file));
            File.Copy(file, destFile, true);
        }

        // Copiar subdiretórios recursivamente
        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            var destSubDir = Path.Combine(destDir, Path.GetFileName(dir));
            CopyDirectory(dir, destSubDir);
        }
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

            string keystorePath;
            string keystorePassword;
            string keyAlias;
            string keyPassword;

            if (certificate.IsDefault)
            {
                // Usar certificado debug do Android
                progress?.Report("Using Android debug certificate...");
                
                if (!File.Exists(_debugKeystorePath))
                {
                    progress?.Report("Debug keystore not found, generating...");
                    await GenerateDebugKeystoreAsync(_debugKeystorePath, progress);
                }

                keystorePath = _debugKeystorePath;
                keystorePassword = "android";
                keyAlias = "androiddebugkey";
                keyPassword = "android";
            }
            else
            {
                // Usar certificado customizado
                keystorePath = certificate.KeystorePath;
                keystorePassword = certificate.KeystorePassword;
                keyAlias = certificate.KeyAlias;
                keyPassword = certificate.KeyPassword;

                if (!File.Exists(keystorePath))
                {
                    return ConversionResult.CreateError($"Keystore not found: {keystorePath}");
                }
            }

            // Assinar AAB usando jarsigner
            progress?.Report("Signing with jarsigner...");
            await SignWithJarsignerAsync(aabPath, keystorePath, keystorePassword, keyAlias, keyPassword, progress);

            // Verificar assinatura
            progress?.Report("Verifying signature...");
            await VerifySignatureAsync(aabPath, keystorePath, keystorePassword, progress);

            progress?.Report("AAB signed successfully!");
            return ConversionResult.CreateSuccess("AAB signed successfully", aabPath);
        }
        catch (Exception ex)
        {
            return ConversionResult.CreateError($"Error during signing: {ex.Message}", ex);
        }
    }

    private async Task GenerateDebugKeystoreAsync(string keystorePath, IProgress<string>? progress)
    {
        // Criar diretório .android se não existir
        var androidDir = Path.GetDirectoryName(keystorePath);
        if (androidDir != null && !Directory.Exists(androidDir))
        {
            Directory.CreateDirectory(androidDir);
        }

        if (string.IsNullOrEmpty(_keytoolPath))
        {
            throw new Exception("keytool not found. Please install Java JDK.\n\nYou can install it via Homebrew:\nbrew install openjdk");
        }

        var processInfo = new ProcessStartInfo
        {
            FileName = _keytoolPath,
            Arguments = $"-genkeypair -v -keystore \"{keystorePath}\" " +
                       "-storepass android -alias androiddebugkey -keypass android " +
                       "-keyalg RSA -keysize 2048 -validity 10000 " +
                       "-dname \"CN=Android Debug,O=Android,C=US\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(processInfo);
        if (process == null)
        {
            throw new Exception("Failed to start keytool process");
        }

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync();
            throw new Exception($"Failed to generate debug keystore: {error}");
        }

        progress?.Report("✓ Debug keystore generated");
    }

    private async Task SignWithJarsignerAsync(string aabPath, string keystorePath, string keystorePassword, 
                                             string keyAlias, string keyPassword, IProgress<string>? progress)
    {
        if (string.IsNullOrEmpty(_jarsignerPath))
        {
            throw new Exception("jarsigner not found. Please install Java JDK.\n\nYou can install it via Homebrew:\nbrew install openjdk");
        }

        var processInfo = new ProcessStartInfo
        {
            FileName = _jarsignerPath,
            Arguments = $"-verbose -sigalg SHA256withRSA -digestalg SHA-256 " +
                       $"-keystore \"{keystorePath}\" " +
                       $"-storepass {keystorePassword} " +
                       $"-keypass {keyPassword} " +
                       $"\"{aabPath}\" {keyAlias}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(processInfo);
        if (process == null)
        {
            throw new Exception("Failed to start jarsigner process");
        }

        var output = new StringBuilder();
        var error = new StringBuilder();

        process.OutputDataReceived += (s, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                output.AppendLine(e.Data);
                progress?.Report(e.Data);
            }
        };

        process.ErrorDataReceived += (s, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                error.AppendLine(e.Data);
            }
        };

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new Exception($"Failed to sign AAB: {error}");
        }
    }

    private async Task VerifySignatureAsync(string aabPath, string keystorePath, string keystorePassword, IProgress<string>? progress)
    {
        if (string.IsNullOrEmpty(_jarsignerPath))
        {
            throw new Exception("jarsigner not found");
        }

        var processInfo = new ProcessStartInfo
        {
            FileName = _jarsignerPath,
            Arguments = $"-verify -verbose -keystore \"{keystorePath}\" -storepass {keystorePassword} \"{aabPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(processInfo);
        if (process == null)
        {
            throw new Exception("Failed to start verification process");
        }

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync();
            throw new Exception($"AAB signature verification failed: {error}");
        }

        progress?.Report("✓ Signature verified");
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
            // Verificar jarsigner
            if (string.IsNullOrEmpty(_jarsignerPath))
            {
                return false;
            }

            var jarsignerTest = await RunCommandAsync(_jarsignerPath, "-help");
            if (!jarsignerTest.Success)
            {
                return false;
            }

            // Verificar keytool
            if (string.IsNullOrEmpty(_keytoolPath))
            {
                return false;
            }

            var keytoolTest = await RunCommandAsync(_keytoolPath, "-help");
            return keytoolTest.Success;
        }
        catch
        {
            return false;
        }
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

    private string FindJarsigner()
    {
        // Locais possíveis para jarsigner
        var possiblePaths = new[]
        {
            "/usr/bin/jarsigner",
            "/opt/homebrew/bin/jarsigner",
            "/usr/local/bin/jarsigner",
        };

        foreach (var path in possiblePaths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        // Tentar usar which
        var whichResult = RunWhichCommand("jarsigner");
        if (!string.IsNullOrEmpty(whichResult))
        {
            return whichResult;
        }

        // Tentar JAVA_HOME
        var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
        if (!string.IsNullOrEmpty(javaHome))
        {
            var path = Path.Combine(javaHome, "bin", "jarsigner");
            if (File.Exists(path))
            {
                return path;
            }
        }

        return string.Empty;
    }

    private string FindKeytool()
    {
        var possiblePaths = new[]
        {
            "/usr/bin/keytool",
            "/opt/homebrew/bin/keytool",
            "/usr/local/bin/keytool",
        };

        foreach (var path in possiblePaths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        // Tentar which
        var whichResult = RunWhichCommand("keytool");
        if (!string.IsNullOrEmpty(whichResult))
        {
            return whichResult;
        }

        // Tentar JAVA_HOME
        var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
        if (!string.IsNullOrEmpty(javaHome))
        {
            var path = Path.Combine(javaHome, "bin", "keytool");
            if (File.Exists(path))
            {
                return path;
            }
        }

        return string.Empty;
    }

    private string RunWhichCommand(string tool)
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "/usr/bin/which",
                Arguments = tool,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null) return string.Empty;

            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            return process.ExitCode == 0 && !string.IsNullOrEmpty(output) ? output : string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
}

