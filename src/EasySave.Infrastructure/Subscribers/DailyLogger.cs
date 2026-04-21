using System.Reflection.Metadata.Ecma335;

namespace EasySave.Infrastructure.Subscribers;

public class DailyLogger
{
    private string GetLogFilePath()
    {
        private string folderPath = "logs";
        private string fileName = $"{DateTime.Now:yyyy-MM-dd}.json";

        return Path.Combine(folderPath, fileName);
    }

    public void Update(Context context)
    {
        // transfère les données au Serialize
        Serialize(context);
    }
    private void Serialize(Context context)
    {
        // traite les données -> créé un fichier json dans lequel les données sont récupérées

        /*
        ecrire quand il reçoit un chemin qui fini par "/" 
            -> différent si c'est 
                -> dosssier (avec "/") 
                -> fichier (sans "/")
         
        if (context.SourcePath.EndsWith("/"))
        {
            // Dossier
        }

        // Fichier

        */

        string sourcePath = context.SourcePath;


    }

    private void WriteToFile()
    {
        // 

        /*
         regarde si il peut écrire le fichier -> chemin valide ?
        écrit dans le fichier
         */
    }
}
