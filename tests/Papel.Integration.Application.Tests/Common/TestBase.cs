namespace Papel.Integration.Application.Tests.Common;
using Papel.Integration.Application.Common.Interfaces;

public class TestBase
{
    protected IApplicationDbContext Context { get; init; }

    protected ICurrentUserService CurrentUserService { get;init; }

    //protected SeedDataContext SeedDataContext { get; init; }

    protected IMediator Mediator { get; init; }

    protected TestBase(QueryTestFixture fixture)
    {
        Context = fixture.Context;
        Mediator = fixture.Mediator;
        CurrentUserService = fixture.CurrentUserService;
        //SeedDataContext = fixture.SeedDataContext;
    }
}
