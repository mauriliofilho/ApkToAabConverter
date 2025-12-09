﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApkToAabConverter.Services;
using ApkToAabConverter.Models;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;

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
            MainThread.BeginInvokeOnMainThread(() =>
            {
                AddLog("Opening file picker...");
            });
            
            var customFileType = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.macOS, new[] { "apk" } },
                    { DevicePlatform.MacCatalyst, new[] { "apk" } }
                });

            var options = new PickOptions
            {
                PickerTitle = "Select APK file",
                FileTypes = customFileType
            };

            FileResult? result = null;
            
            // Garantir que o FilePicker seja chamado na main thread
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    result = await FilePicker.Default.PickAsync(options);
                }
                catch (Exception ex)
                {
                    AddLog($"✗ FilePicker error: {ex.Message}");
                    await DisplayAlert("Error", $"Failed to open file picker: {ex.Message}", "OK");
                }
            });

            if (result != null)
            {
                _selectedApkPath = result.FullPath;
                SelectedApkLabel.Text = $"Selected: {Path.GetFileName(result.FullPath)}";
                SelectedApkLabel.IsVisible = true;

                // Gerar caminho de saída
                var directory = Path.GetDirectoryName(result.FullPath) ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(result.FullPath);
                _outputPath = Path.Combine(directory, $"{fileNameWithoutExtension}.aab");
                OutputPathEntry.Text = _outputPath;

                // Habilitar botões
                ConvertOnlyButton.IsEnabled = true;
                ConvertAndSignButton.IsEnabled = true;

                AddLog($"✓ Selected APK: {result.FullPath}");
                StatusLabel.Text = "APK selected. Ready to convert.";
                StatusLabel.TextColor = Colors.Blue;
            }
            else
            {
                AddLog("File selection cancelled.");
            }
        }
        catch (Exception ex)
        {
            AddLog($"✗ Error selecting file: {ex.Message}");
            await DisplayAlert("Error", $"Failed to select file: {ex.Message}", "OK");
        }
    }

    private async void OnSelectCertificateClicked(object sender, EventArgs e)
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                AddLog("Opening keystore file picker...");
            });
            
            var customFileType = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.macOS, new[] { "jks", "keystore", "p12" } },
                    { DevicePlatform.MacCatalyst, new[] { "jks", "keystore", "p12" } }
                });

            var options = new PickOptions
            {
                PickerTitle = "Select Keystore file",
                FileTypes = customFileType
            };

            FileResult? result = null;
            
            // Garantir que o FilePicker seja chamado na main thread
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    result = await FilePicker.Default.PickAsync(options);
                }
                catch (Exception ex)
                {
                    AddLog($"✗ FilePicker error: {ex.Message}");
                    await DisplayAlert("Error", $"Failed to open file picker: {ex.Message}", "OK");
                }
            });

            if (result != null)
            {
                AddLog($"Selected keystore: {Path.GetFileName(result.FullPath)}");
                
                // Solicitar informações do keystore
                var keystorePassword = await DisplayPromptAsync("Keystore Password", "Enter keystore password:", "OK", "Cancel", keyboard: Keyboard.Default);
                if (string.IsNullOrEmpty(keystorePassword))
                {
                    AddLog("Keystore password not provided.");
                    return;
                }

                var keyAlias = await DisplayPromptAsync("Key Alias", "Enter key alias:", "OK", "Cancel");
                if (string.IsNullOrEmpty(keyAlias))
                {
                    AddLog("Key alias not provided.");
                    return;
                }

                var keyPassword = await DisplayPromptAsync("Key Password", "Enter key password:", "OK", "Cancel", keyboard: Keyboard.Default);
                if (string.IsNullOrEmpty(keyPassword))
                {
                    AddLog("Key password not provided.");
                    return;
                }

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
                CertificateInfoLabel.TextColor = Colors.Green;
                AddLog($"✓ Custom certificate configured: {Path.GetFileName(result.FullPath)}");
            }
            else
            {
                AddLog("Certificate selection cancelled.");
            }
        }
        catch (Exception ex)
        {
            AddLog($"✗ Error selecting certificate: {ex.Message}");
            await DisplayAlert("Error", $"Failed to select certificate: {ex.Message}", "OK");
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
