using EasySave.Application.ViewModels;

namespace EasySave.CLI;

public static class Program
{
    public static void Main(string[] args)
    {
        string configFilePath = Path.Combine(Directory.GetCurrentDirectory(), "config.json");
        if (args.Length != 0)
        {
            if (!File.Exists(configFilePath))
            {
                throw new Exception("Aucun fichier de configuration trouvé à l'emplacement : " + configFilePath);
            }
            int[] jobToExecute = GetJobsToExecute(args);

            var viewModel = new MainViewModel(configFilePath, jobToExecute);
            viewModel.CreateBackupConfig();
            viewModel.Execute();
        }
    }

    private static int[] GetJobsToExecute(string[] args)
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
        else
        {
            return [1]; // Si aucun argument n'est fourni, exécute le job 1 par défaut
        }
    }
}
