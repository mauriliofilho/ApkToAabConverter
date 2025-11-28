using ApkToAabConverter.Services;
using ApkToAabConverter.Models;

namespace ApkToAabConverter;

public partial class MainPage : ContentPage
{
    private readonly IApkToAabService _apkToAabService;
    private string _selectedApkPath = string.Empty;
    private string _outputPath = string.Empty;
    private CertificateInfo _selectedCertificate;
    private bool _isProcessing;

    public MainPage()
    {
        InitializeComponent();
        _apkToAabService = new ApkToAabService();
        _selectedCertificate = CertificateInfo.DefaultAndroidCertificate();
    }

    private async void OnSelectApkClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Select APK file",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.macOS, new[] { "apk" } },
                    { DevicePlatform.MacCatalyst, new[] { "apk" } }
                })
            });

            if (result != null)
            {
                _selectedApkPath = result.FullPath;
                SelectedApkLabel.Text = result.FullPath;
                SelectedApkLabel.IsVisible = true;

                // Gerar caminho de saída
                var directory = Path.GetDirectoryName(result.FullPath) ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(result.FullPath);
                _outputPath = Path.Combine(directory, $"{fileNameWithoutExtension}.aab");
                OutputPathEntry.Text = _outputPath;

                // Habilitar botões
                ConvertOnlyButton.IsEnabled = true;
                ConvertAndSignButton.IsEnabled = true;

                AddLog($"Selected APK: {result.FullPath}");
                StatusLabel.Text = "APK selected. Ready to convert.";
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnSelectCertificateClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Select Keystore file",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.macOS, new[] { "jks", "keystore", "p12" } },
                    { DevicePlatform.MacCatalyst, new[] { "jks", "keystore", "p12" } }
                })
            });

            if (result != null)
            {
                // Solicitar informações do keystore
                var keystorePassword = await DisplayPromptAsync("Keystore Password", "Enter keystore password:", "OK", "Cancel", keyboard: Keyboard.Default);
                if (string.IsNullOrEmpty(keystorePassword)) return;

                var keyAlias = await DisplayPromptAsync("Key Alias", "Enter key alias:", "OK", "Cancel");
                if (string.IsNullOrEmpty(keyAlias)) return;

                var keyPassword = await DisplayPromptAsync("Key Password", "Enter key password:", "OK", "Cancel", keyboard: Keyboard.Default);
                if (string.IsNullOrEmpty(keyPassword)) return;

                _selectedCertificate = new CertificateInfo
                {
                    Name = "Custom Certificate",
                    KeystorePath = result.FullPath,
                    KeystorePassword = keystorePassword,
                    KeyAlias = keyAlias,
                    KeyPassword = keyPassword,
                    IsDefault = false
                };

                UseDefaultCertCheckbox.IsChecked = false;
                CertificateInfoLabel.Text = $"Custom Certificate: {Path.GetFileName(result.FullPath)}";
                AddLog($"Selected keystore: {result.FullPath}");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnConvertOnlyClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_selectedApkPath))
        {
            await DisplayAlert("No APK selected", "Please select an APK file first.", "OK");
            return;
        }

        await ConvertAsync(false);
    }

    private async void OnConvertAndSignClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_selectedApkPath))
        {
            await DisplayAlert("No APK selected", "Please select an APK file first.", "OK");
            return;
        }

        await ConvertAsync(true);
    }

    private async Task ConvertAsync(bool signAfterConversion)
    {
        if (_isProcessing) return;

        _isProcessing = true;
        ProgressFrame.IsVisible = true;
        ActivityIndicator.IsRunning = true;
        ConvertOnlyButton.IsEnabled = false;
        ConvertAndSignButton.IsEnabled = false;

        try
        {
            var progress = new Progress<string>(message =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    AddLog(message);
                    ProgressBar.Progress = Math.Min(1.0, ProgressBar.Progress + 0.1);
                });
            });

            StatusLabel.Text = signAfterConversion ? "Converting and signing..." : "Converting...";

            ConversionResult result;
            if (signAfterConversion)
            {
                // Aplicar certificado padrão se necessário
                if (UseDefaultCertCheckbox.IsChecked)
                {
                    _selectedCertificate = CertificateInfo.DefaultAndroidCertificate();
                }

                result = await _apkToAabService.ConvertAndSignAsync(_selectedApkPath, _outputPath, _selectedCertificate, progress);
            }
            else
            {
                result = await _apkToAabService.ConvertApkToAabAsync(_selectedApkPath, _outputPath, progress);
            }

            if (result.Success)
            {
                StatusLabel.Text = "Operation completed successfully!";
                StatusLabel.TextColor = Colors.Green;
                AddLog($"✓ Success: {result.OutputPath}");
                await DisplayAlert("Success", $"Operation completed successfully!\n\nOutput: {result.OutputPath}", "OK");
            }
            else
            {
                StatusLabel.Text = "Operation failed.";
                StatusLabel.TextColor = Colors.Red;
                AddLog($"✗ Error: {result.Message}");
                await DisplayAlert("Error", result.Message, "OK");
            }
        }
        catch (Exception ex)
        {
            StatusLabel.Text = "Error during operation.";
            StatusLabel.TextColor = Colors.Red;
            AddLog($"✗ Exception: {ex.Message}");
            await DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            _isProcessing = false;
            ProgressFrame.IsVisible = false;
            ActivityIndicator.IsRunning = false;
            ProgressBar.Progress = 0;
            ConvertOnlyButton.IsEnabled = !string.IsNullOrEmpty(_selectedApkPath);
            ConvertAndSignButton.IsEnabled = !string.IsNullOrEmpty(_selectedApkPath);
        }
    }

    private void OnClearLogsClicked(object sender, EventArgs e)
    {
        LogOutputLabel.Text = string.Empty;
    }

    private void AddLog(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        var currentLog = LogOutputLabel.Text ?? string.Empty;
        LogOutputLabel.Text = currentLog + $"[{timestamp}] {message}\n";
    }
}
