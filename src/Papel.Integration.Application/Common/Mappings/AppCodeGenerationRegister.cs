namespace Papel.Integration.Application.Common.Mappings;
using System.ComponentModel.DataAnnotations.Schema;

public class AppCodeGenerationRegister : ICodeGenerationRegister
{
    public void Register(CodeGenerationConfig config)
    {
        config.AdaptTo("[name]Dto").ForType<Txn>()
            .IgnoreAttributes(typeof(NotMappedAttribute));
        config.AdaptTo("[name]Dto").ForType<Account>()
            .IgnoreAttributes(typeof(NotMappedAttribute));
        config.AdaptTo("[name]Dto").ForType<LoadMoneyRequest>()
            .IgnoreAttributes(typeof(NotMappedAttribute));
        config.AdaptTo("[name]Dto").ForType<Customer>()
            .IgnoreAttributes(typeof(NotMappedAttribute));

        config.GenerateMapper("[name]Mapper")
            .ForType<Txn>()
            .ForType<LoadMoneyRequest>()
            .ForType<Customer>()
            .ForType<Account>();
    }
}
