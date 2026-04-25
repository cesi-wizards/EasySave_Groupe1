using System.Xml.Linq;
using EasyLog.Interfaces;

namespace EasyLog.Loggers;

public class XmlLogger : ILogger
{
    public string FilePath { get; }

    // Locker for multithreading
    private static readonly object _lock = new();

    public XmlLogger(string filePath)
    {
        FilePath = FilePathToXmlPath(filePath);
    }

    public void Write(Dictionary<string, object> dictionaryContent)
    {
        lock (_lock)
        {
            try
            {
                // On s'assure d'utiliser le bon paramètre
                string xmlContent = ContentToXml(dictionaryContent);
                WriteXml(xmlContent);
            }
            catch (Exception ex)
            {
                // On propage l'erreur avec un message explicite
                throw new IOException("Échec de l'écriture du log XML.", ex);
            }
        }
    }

    private string FilePathToXmlPath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            filePath = "default_log.xml";
        }

        return Path.ChangeExtension(filePath, ".xml");
    }

    private string ContentToXml(Dictionary<string, object> dictionaryContent)
    {
        if (dictionaryContent == null || dictionaryContent.Count == 0)
        {
            return new XElement("LogEntry").ToString(SaveOptions.DisableFormatting);
        }

        try
        {
            // On crée un élément racine <LogEntry>
            var root = new XElement("LogEntry",
                dictionaryContent.Select(kv => new XElement(kv.Key, kv.Value))
            );

            // On exporte en une seule ligne pour garder la logique de "une ligne = un log"
            return root.ToString(SaveOptions.DisableFormatting);
        }
        catch (Exception ex)
        {
            // Ici on lance une exception liée au XML, pas au JSON !
            throw new InvalidOperationException("Impossible de sérialiser le contenu en XML.", ex);
        }
    }

    private void EnsureDirectoryExists()
    {
        try
        {
            string directory = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new IOException($"Permission denied to create the folder : {FilePath}", ex);
        }
        catch (PathTooLongException ex)
        {
            throw new IOException($"Filepath is too long for the file tree : {FilePath}", ex);
        }
    }

    private void WriteXml(string xmlContent)
    {
        // ensures the directory exists
        EnsureDirectoryExists();

        try
        {
            if (!File.Exists(FilePath))
            {
                WriteXmlNewFile(xmlContent);
            }
            else
            {
                WriteXmlExistingFile(xmlContent);
            }
        }
        catch (IOException ex)
        {
            throw new IOException($"Error, couldn't write within the file : {FilePath}", ex);
        }
    }

    private void WriteXmlNewFile(string xmlContent)
    {
        try
        {
            // We put the content between banners for xml format validity
            string initialContent = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + Environment.NewLine +
                                    "<Logs>" + Environment.NewLine +
                                    "  " + xmlContent + Environment.NewLine +
                                    "</Logs>";
            File.WriteAllText(FilePath, initialContent);
        }
        catch (IOException ex)
        {
            throw new IOException($"Impossible to create the XML file : {FilePath}", ex);
        }
    }

    private void WriteXmlExistingFile(string xmlContent)
    {
        try
        {
            // we load the existing file
            XDocument doc = XDocument.Load(FilePath);

            XElement newEntry = XElement.Parse(xmlContent);

            doc.Root?.Add(newEntry);

            // Save the state of the log file
            doc.Save(FilePath);
        }
        catch (Exception ex)
        {
            throw new IOException($"Error, couldn't save the state of the XML file : {FilePath}", ex);
        }
    }
}
