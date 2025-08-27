namespace Papel.Integration.Application.TodoItems.Queries.GetQueryable;

public sealed record GetAllToDoItemsQuery : IRequest<IQueryable<ToDoItem>>
{
}
