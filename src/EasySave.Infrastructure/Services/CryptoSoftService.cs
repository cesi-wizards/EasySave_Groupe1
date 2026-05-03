using System.Diagnostics;
using System.Runtime.InteropServices;
using EasySave.Domain.Interfaces;

namespace EasySave.Infrastructure.Services;

public class CryptoSoftService : IEncryptionService
{
    private static readonly string _cryptoSoftExecutableName =
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "CryptoSoft.exe" : "CryptoSoft";

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
                // Couldn't find the path to cryptosoft in environment variables
                Debug.WriteLine($"Error : {_cryptoSoftExecutableName} wasn't found in the PATH.");
                return TimeSpan.FromMilliseconds(-3);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error CryptoSoft : {ex.Message}");
                return TimeSpan.FromMilliseconds(-2);
            }
        }
    }
}
