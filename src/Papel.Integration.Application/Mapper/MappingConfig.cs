namespace Papel.Integration.Application.Mapper;

using Events.Transaction;

public class MappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<TransactionCompltedDomainEvent, TransactionCompletedIntegrationEvent>();
    }
}
