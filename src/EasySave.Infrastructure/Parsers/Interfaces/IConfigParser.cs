namespace EasySave.Infrastructure.Parsers.Interfaces;

public interface IConfigParser
{
    Config Parse(string FilePath);
}
