using EasySave.Application.ViewModels;

namespace EasySave.CLI;

public static class Program
{
    public static void Main(string[] args)
    {
        const string configFilePath = "config.json";
        if (args.Length != 0)
        {
            int[]? jobToExecute = GetJobToExecute(args);

            if (jobToExecute == null) return;

            var viewModel = new MainViewModel(configFilePath, jobToExecute);
            viewModel.CreateBackupConfig();
            viewModel.Execute();
        }
    }

    private static int[]? GetJobToExecute(string[] args)
    {
        string arg = args[0];

        if (arg.Contains('-', StringComparison.CurrentCultureIgnoreCase))
        {
            int firstJob = Int32.Parse(arg.Split("-")[0]);
            int secondJob = Int32.Parse(arg.Split("-")[1]);
            return Enumerable.Range(firstJob, secondJob - firstJob + 1).ToArray();
        }
        else if (arg.Contains(';', StringComparison.CurrentCultureIgnoreCase))
        {
            return arg.Split(';').Select(int.Parse).ToArray();
        }
        return null;
    }
}
