using Volo.Abp.Domain.Entities;

namespace TelegramServer.Entities.Es;

public abstract class TelegramEsEntity<TKey> : Entity, IEntity<TKey>
{
    public virtual TKey Id { get; set; }

    public override object[] GetKeys()
    {
        return new object[] { Id };
    }
}