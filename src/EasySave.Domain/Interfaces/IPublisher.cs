namespace EasySave.Domain.Interfaces;

public interface IPublisher
{
    void Attach(ISubscriber subscriber);
    void Detach(ISubscriber subscriber);
}
