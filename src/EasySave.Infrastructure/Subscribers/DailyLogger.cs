namespace EasySave.Infrastructure.Subscribers;

public class DailyLogger
{
    public void ISubscriber.Update(Context context)
    {
        // récupère les données
        ISubscriber.Serialize(context);
    }
    private void ISubscriber.Serialize(Context context)
    {
        // traite les données

        /*
        ecrire quand il reçoit un chemin qui fini par "/" 
            -> différent si c'est 
                -> dosssier (avec "/") 
                -> fichier (sans "/")
         */

        if(context.SourcePath.end)
    }

    private void ISubscriber.WriteToFile()
    {
        
    }
}
