namespace Papel.Integration.Application.TodoItems.Models;

using Common.Filters;

public sealed partial record ToDoItemFilter
{
    public FilterFieldDefinition<Guid>? Id { get; set; }

    public FilterFieldDefinition<string>? Title { get; set; }
}
