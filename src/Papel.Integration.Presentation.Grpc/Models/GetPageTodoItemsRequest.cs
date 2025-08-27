namespace Papel.Integration.Presentation.Grpc.Models;

[ProtoContract]
public sealed record GetPageTodoItemsRequest : PageContext
{
    [ProtoMember(1)]
    public Guid[] Ids { get; init; } = [];
}
