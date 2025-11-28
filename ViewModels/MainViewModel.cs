using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ApkToAabConverter.Models;
using ApkToAabConverter.Services;

namespace ApkToAabConverter.ViewModels;

/// <summary>
/// ViewModel principal do aplicativo
/// </summary>
public class MainViewModel : INotifyPropertyChanged
{
    private readonly IApkToAabService _apkToAabService;
    private string _selectedApkPath = string.Empty;
    private string _outputPath = string.Empty;
    private string _statusMessage = "Ready";
    private bool _isProcessing;
    private double _progress;
    private string _logOutput = string.Empty;
    private CertificateInfo _selectedCertificate;
    private bool _useDefaultCertificate = true;

    public event PropertyChangedEventHandler? PropertyChanged;

    public MainViewModel(IApkToAabService apkToAabService)
    {
        _apkToAabService = apkToAabService;
        _selectedCertificate = CertificateInfo.DefaultAndroidCertificate();

        // Comandos
        SelectApkCommand = new Command(async () => await SelectApkAsync());
        ConvertOnlyCommand = new Command(async () => await ConvertOnlyAsync(), () => CanExecuteConversion);
        ConvertAndSignCommand = new Command(async () => await ConvertAndSignAsync(), () => CanExecuteConversion);
        SelectCertificateCommand = new Command(async () => await SelectCertificateAsync());
        ClearLogsCommand = new Command(() => LogOutput = string.Empty);
    }

    #region Properties

    public string SelectedApkPath
    {
        get => _selectedApkPath;
        set
        {
            _selectedApkPath = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasApkSelected));
            UpdateCommandStates();
        }
    }

    public string OutputPath
    {
        get => _outputPath;
        set
        {
            _outputPath = value;
            OnPropertyChanged();
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public bool IsProcessing
    {
        get => _isProcessing;
        set
        {
            _isProcessing = value;
            OnPropertyChanged();
            UpdateCommandStates();
        }
    }

    public double Progress
    {
        get => _progress;
        set
        {
            _progress = value;
            OnPropertyChanged();
        }
    }

    public string LogOutput
    {
        get => _logOutput;
        set
        {
            _logOutput = value;
            OnPropertyChanged();
        }
    }

    public bool UseDefaultCertificate
    {
        get => _useDefaultCertificate;
        set
        {
            _useDefaultCertificate = value;
            if (value)
            {
                _selectedCertificate = CertificateInfo.DefaultAndroidCertificate();
            }
            OnPropertyChanged();
        }
    }

    public CertificateInfo SelectedCertificate
    {
        get => _selectedCertificate;
        set
        {
            _selectedCertificate = value;
            OnPropertyChanged();
        }
    }

    public bool HasApkSelected => !string.IsNullOrEmpty(SelectedApkPath);

    public bool CanExecuteConversion => HasApkSelected && !IsProcessing;

    #endregion

    #region Commands

    public ICommand SelectApkCommand { get; }
    public ICommand ConvertOnlyCommand { get; }
    public ICommand ConvertAndSignCommand { get; }
    public ICommand SelectCertificateCommand { get; }
    public ICommand ClearLogsCommand { get; }

    #endregion

    #region Methods

    private async Task SelectApkAsync()
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
                SelectedApkPath = result.FullPath;
                
                // Gerar caminho de saída padrão
                var directory = Path.GetDirectoryName(result.FullPath) ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(result.FullPath);
                OutputPath = Path.Combine(directory, $"{fileNameWithoutExtension}.aab");

                AddLog($"Selected APK: {result.FullPath}");
                StatusMessage = "APK selected. Ready to convert.";
            }
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Error selecting file", ex.Message);
        }
    }

    private async Task ConvertOnlyAsync()
    {
        if (string.IsNullOrEmpty(SelectedApkPath))
        {
            await ShowErrorAsync("No APK selected", "Please select an APK file first.");
            return;
        }

        IsProcessing = true;
        Progress = 0;
        StatusMessage = "Converting APK to AAB...";

        try
        {
            var progress = new Progress<string>(message =>
            {
                AddLog(message);
                Progress += 10;
            });

            var result = await _apkToAabService.ConvertApkToAabAsync(SelectedApkPath, OutputPath, progress);

            if (result.Success)
            {
                StatusMessage = "Conversion completed successfully!";
                AddLog($"✓ AAB created: {result.OutputPath}");
                await ShowSuccessAsync("Success", $"AAB file created successfully:\n{result.OutputPath}");
            }
            else
            {
                StatusMessage = "Conversion failed.";
                AddLog($"✗ Error: {result.Message}");
                await ShowErrorAsync("Conversion failed", result.Message);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = "Error during conversion.";
            AddLog($"✗ Exception: {ex.Message}");
            await ShowErrorAsync("Error", ex.Message);
        }
        finally
        {
            IsProcessing = false;
            Progress = 0;
        }
    }

    private async Task ConvertAndSignAsync()
    {
        if (string.IsNullOrEmpty(SelectedApkPath))
        {
            await ShowErrorAsync("No APK selected", "Please select an APK file first.");
            return;
        }

        IsProcessing = true;
        Progress = 0;
        StatusMessage = "Converting and signing...";

        try
        {
            var progress = new Progress<string>(message =>
            {
                AddLog(message);
                Progress += 5;
            });

            var result = await _apkToAabService.ConvertAndSignAsync(
                SelectedApkPath,
                OutputPath,
                SelectedCertificate,
                progress
            );

            if (result.Success)
            {
                StatusMessage = "Conversion and signing completed!";
                AddLog($"✓ Signed AAB created: {result.OutputPath}");
                await ShowSuccessAsync("Success", $"Signed AAB file created successfully:\n{result.OutputPath}");
            }
            else
            {
                StatusMessage = "Operation failed.";
                AddLog($"✗ Error: {result.Message}");
                await ShowErrorAsync("Operation failed", result.Message);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = "Error during operation.";
            AddLog($"✗ Exception: {ex.Message}");
            await ShowErrorAsync("Error", ex.Message);
        }
        finally
        {
            IsProcessing = false;
            Progress = 0;
        }
    }

    private async Task SelectCertificateAsync()
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Select Keystore file",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.macOS, new[] { "jks", "keystore" } },
                    { DevicePlatform.MacCatalyst, new[] { "jks", "keystore" } }
                })
            });

            if (result != null)
            {
                // Aqui seria necessário um diálogo para pedir informações do keystore
                // Por enquanto, apenas definir o caminho
                SelectedCertificate = new CertificateInfo
                {
                    Name = "Custom Certificate",
                    KeystorePath = result.FullPath,
                    IsDefault = false
                };

                UseDefaultCertificate = false;
                AddLog($"Selected keystore: {result.FullPath}");
            }
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Error selecting certificate", ex.Message);
        }
    }

    private void AddLog(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        LogOutput += $"[{timestamp}] {message}\n";
    }

    private void UpdateCommandStates()
    {
        (ConvertOnlyCommand as Command)?.ChangeCanExecute();
        (ConvertAndSignCommand as Command)?.ChangeCanExecute();
    }

    private async Task ShowErrorAsync(string title, string message)
    {
        if (Application.Current?.MainPage != null)
        {
            await Application.Current.MainPage.DisplayAlert(title, message, "OK");
        }
    }

    private async Task ShowSuccessAsync(string title, string message)
    {
        if (Application.Current?.MainPage != null)
        {
            await Application.Current.MainPage.DisplayAlert(title, message, "OK");
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
}
