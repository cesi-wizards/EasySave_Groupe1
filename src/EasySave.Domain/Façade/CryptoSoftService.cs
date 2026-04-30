using System.Diagnostics;

namespace EasySave.Domain.Façade;

public class CryptoSoftService
{
    private static readonly string _cryptoSoftExcutableName = "CryptoSoft.exe";
    public static int Encrypt(string filePath, string key)
    {
        using (Process process = new Process())
        {
            process.StartInfo = new ProcessStartInfo
            {
                FileName = _cryptoSoftExcutableName,
                Arguments = $"\"{filePath}\" \"{key}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            try
            {
                process.Start();
                process.WaitForExit();

                return process.ExitCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error CryptoSoft : {ex.Message}");
                return -1;
            }
        }
    }
}
