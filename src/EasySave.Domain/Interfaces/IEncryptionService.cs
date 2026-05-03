namespace EasySave.Domain.Interfaces;

public interface IEncryptionService
{
    TimeSpan Encrypt(string filePath, string key);
}
