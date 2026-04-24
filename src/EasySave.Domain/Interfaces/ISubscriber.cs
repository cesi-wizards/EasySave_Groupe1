using EasySave.Domain.Entities;

namespace EasySave.Domain.Interfaces;

public interface ISubscriber
{
    void Update(Context context);
}
