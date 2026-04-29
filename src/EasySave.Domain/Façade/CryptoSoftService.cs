using System.Diagnostics;

namespace EasySave.Domain.Façade;

public class CryptoSoftService
{
    private static readonly string _cryptoSoftExcutableName = "cryptosoft.exe";

    public static int Encrypte(string filePath, string key)
    {
        Process process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _cryptoSoftExcutableName,
                Arguments = $"{filePath},{key}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            }
        };

        process.Start();
        process.WaitForExit();
        return process.ExitCode;
    }
}
