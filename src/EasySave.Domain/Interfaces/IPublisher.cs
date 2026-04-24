namespace EasySave.Domain.Interfaces;

public interface IPublisher
{
    // #Notify() not specified: interfaces can't have protected members
    void Attach(ISubscriber subscriber);
    void Detach(ISubscriber subscriber);
}
