namespace ApkToAabConverter.Models;

/// <summary>
/// Informações sobre o certificado de assinatura
/// </summary>
public class CertificateInfo
{
    public string Name { get; set; } = string.Empty;
    public string KeystorePath { get; set; } = string.Empty;
    public string KeystorePassword { get; set; } = string.Empty;
    public string KeyAlias { get; set; } = string.Empty;
    public string KeyPassword { get; set; } = string.Empty;
    public bool IsDefault { get; set; }

    /// <summary>
    /// Certificado padrão do Android (testkey)
    /// </summary>
    public static CertificateInfo DefaultAndroidCertificate()
    {
        return new CertificateInfo
        {
            Name = "Default Android Certificate (testkey)",
            IsDefault = true,
            KeystorePath = "testkey", // Será tratado especialmente
            KeystorePassword = "android",
            KeyAlias = "androiddebugkey",
            KeyPassword = "android"
        };
    }
}
