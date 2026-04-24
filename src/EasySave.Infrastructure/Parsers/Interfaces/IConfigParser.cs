using EasySave.Domain.Entities;

namespace EasySave.Infrastructure.Parsers.Interfaces;

public interface IConfigParser
{
    FileConfig Parse(string filePath);
}
