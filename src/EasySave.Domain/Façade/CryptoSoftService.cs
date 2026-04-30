using System.Diagnostics;

namespace EasySave.Domain.Façade;

public class CryptoSoftService
{
    private static readonly string _cryptoSoftExecutableName = "CryptoSoft.exe";
    public static int Encrypt(string filePath, string key)
    {
        using (Process process = new Process())
        {
            process.StartInfo = new ProcessStartInfo
            {
                FileName = _cryptoSoftExecutableName,
                Arguments = $"\"{filePath}\" \"{key}\"",
                UseShellExecute = false,
                RedirectStandardOutput = false,
                CreateNoWindow = true
            };
            try
            {
                process.Start();
                process.WaitForExit();

                return process.ExitCode;
            }
            catch (System.ComponentModel.Win32Exception)
            {
                // Couldn't find the path to cryptosoft in environment variables
                Debug.WriteLine("Error : CryptoSoft.exe isn't in!! the PATH of Windows.");
                return -3;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error CryptoSoft : {ex.Message}");
                return -2;
            }
        }
    }
}
