namespace Papel.Integration.Application.TodoItems.Queries.GetItem;

using Papel.Integration.Application.Models;

public sealed record GetTodoItemQuery(Guid Id) : IRequest<Result<ToDoItemDto>>;
