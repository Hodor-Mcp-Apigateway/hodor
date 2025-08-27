namespace Papel.Integration.Application.TodoItems.Commands.Update;

public sealed record UpdateToDoItemCommand(Guid Id, string Title, string Description) : IRequest;
