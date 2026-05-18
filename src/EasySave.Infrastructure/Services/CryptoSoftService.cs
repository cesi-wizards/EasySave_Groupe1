using System.Diagnostics;
using System.Runtime.InteropServices;
using EasySave.Domain.Interfaces;

namespace EasySave.Infrastructure.Services;

public class CryptoSoftService : IEncryptionService
{
    private static readonly string _cryptoSoftExecutableName =
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "CryptoSoft.exe" : "/usr/local/bin/CryptoSoft";

    public TimeSpan Encrypt(string filePath, string key)
    {
        using (Process process = new Process())
        {
            process.StartInfo = new ProcessStartInfo
            {
                FileName = _cryptoSoftExecutableName,
                Arguments = $"\"{filePath.Trim()}\" \"{key.Trim()}\"",
                UseShellExecute = false,
                RedirectStandardOutput = false,
                CreateNoWindow = true
            };
            try
            {
                process.Start();
                process.WaitForExit();

                return TimeSpan.FromMilliseconds(process.ExitCode);
            }
            catch (System.ComponentModel.Win32Exception)
            {
                // CryptoSoft not found in PATH — surface as a real error so the caller can report it
                throw new FileNotFoundException($"'{_cryptoSoftExecutableName}' was not found in PATH. Please install CryptoSoft.\n " +
                                                $"https://github.com/cesi-wizards/CryptoSoft/releases/tag/v1.0.0");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error CryptoSoft : {ex.Message}");
                return TimeSpan.FromMilliseconds(-2);
            }
        }
    }
}
