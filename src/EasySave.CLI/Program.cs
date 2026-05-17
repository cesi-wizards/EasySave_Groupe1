using System.Text.Json;
using System.Text.Json.Serialization;
using EasySave.Application;
using EasySave.Domain.Entities;

namespace EasySave.CLI;

public static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0) return;

        string configFilePath = Path.Combine(Directory.GetCurrentDirectory(), "config.json");
        if (!File.Exists(configFilePath))
            throw new FileNotFoundException("No config file found at: " + configFilePath);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        FileConfig config = JsonSerializer.Deserialize<FileConfig>(File.ReadAllText(configFilePath), options)
            ?? throw new InvalidOperationException("Failed to deserialize config file");

        int[] jobIndices = ParseJobIndices(args[0]);

        var jobManager = new JobManager(config.BusinessSoftwares);
        jobManager.SetPriorityExtensions(config.PriorityExtensions);
        jobManager.SetLargeFileSizeThreshold(config.LargeFileSizeThresholdKb);

        foreach (int index in jobIndices)
        {
            BackupConfig job = config.Jobs[index - 1];
            job.LogFileType = config.LogFileType;
            jobManager.AddJob(job);
        }

        jobManager.ExecuteAllJobs().Wait();
    }

    private static int[] ParseJobIndices(string arg)
    {
        arg = arg.Trim();

        if (arg.Contains('-'))
        {
            int first = int.Parse(arg.Split('-')[0]);
            int last  = int.Parse(arg.Split('-')[1]);
            return Enumerable.Range(first, last - first + 1).ToArray();
        }

        if (arg.Contains(';'))
            return arg.Split(';').Select(int.Parse).ToArray();

        if (int.TryParse(arg, out int single))
            return [single];

        return [1];
    }
}
