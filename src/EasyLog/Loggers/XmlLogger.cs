using System.Xml.Linq;

namespace EasyLog.Loggers;

public class XmlLogger(string filePath) : AbstractLogger(FilePathToXmlPath(filePath))
{
    // Locker for multithreading
    private static readonly object _lock = new();

    public override void Write(Dictionary<string, object> dictionaryContent)
    {
        lock (_lock)
        {
            try
            {
                string xmlContent = ContentToXml(dictionaryContent);
                WriteXml(xmlContent);
            }
            catch (Exception ex)
            {
                throw new IOException("Failed to write XML log", ex);
            }
        }
    }

    private static string FilePathToXmlPath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            filePath = "default_log.xml";
        }

        return Path.ChangeExtension(filePath, ".xml");
    }

    private string ContentToXml(Dictionary<string, object> dictionaryContent)
    {
        if (dictionaryContent.Count == 0)
        {
            return new XElement("LogEntry").ToString(SaveOptions.DisableFormatting);
        }

        try
        {
            var root = new XElement("LogEntry",
                dictionaryContent.Select(kv => new XElement(kv.Key, kv.Value))
            );
            return root.ToString(SaveOptions.DisableFormatting);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to serialize config file in XML", ex);
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
            throw new IOException($"Failed to write to the file: {FilePath}", ex);
        }
    }

    private void WriteXmlNewFile(string xmlContent)
    {
        try
        {
            // We put the content between banners for XML format validity
            string initialContent = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + Environment.NewLine +
                                    "<Logs>" + Environment.NewLine +
                                    "  " + xmlContent + Environment.NewLine +
                                    "</Logs>";
            File.WriteAllText(FilePath, initialContent);
        }
        catch (IOException ex)
        {
            throw new IOException($"Failed to create the XML file: {FilePath}", ex);
        }
    }

    private void WriteXmlExistingFile(string xmlContent)
    {
        try
        {
            // we load the existing file
            var doc = XDocument.Load(FilePath);

            var newEntry = XElement.Parse(xmlContent);

            doc.Root?.Add(newEntry);

            // Save the state of the log file
            doc.Save(FilePath);
        }
        catch (Exception ex)
        {
            throw new IOException($"Failed to save the XML file: {FilePath}", ex);
        }
    }
}
