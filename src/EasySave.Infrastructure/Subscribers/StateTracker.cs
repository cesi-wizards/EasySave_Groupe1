using System.Runtime.Serialization.Json;

namespace EasySave.Infrastructure.Subscribers;

public class StateTracker
{
    public void ISubscriber.Update(Context context)
    {
        // récupère les données
        ISubscriber.Serialize(context);
    }
    private void ISubscriber.Serialize(Context context)
    {
        // traite les données

    }

    private void ISubscriber.WriteToFile()
    {

    }
}
