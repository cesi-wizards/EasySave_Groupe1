namespace EasySave.Infrastructure.Parsers.Interfaces;

public interface IConfigParser
{
    FileConfig Parse(string FilePath);
}
