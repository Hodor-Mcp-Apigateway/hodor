global using System.Diagnostics;
global using System.Diagnostics.CodeAnalysis;
global using System.Reflection;

global using Microsoft.Extensions.Logging;
global using Microsoft.EntityFrameworkCore;
global using FluentValidation;
global using FluentResults;
global using MediatR;
global using Mapster;
global using MapsterMapper;
global using Ardalis.Specification;
global using FluentValidation.Results;
//#if (EnableKafka)
global using Wolverine;
//#endif
global using MediatR.Pipeline;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;
global using Papel.Integration.Common.Extensions;

global using Papel.Integration.Domain.AggregatesModel.ToDoAggregates.Entities;
global using Papel.Integration.Domain.Events;
