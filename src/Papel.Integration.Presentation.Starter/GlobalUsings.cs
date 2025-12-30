global using System.Reflection;
<<<<<<< HEAD
=======
//#if (EnableRedisCache)
global using EFCoreSecondLevelCacheInterceptor;
global using Papel.Integration.EFCore.Caching.Redis.Extensions;
//#endif
>>>>>>> b321969 (change rabbitmq to kafka and masstransit to wolverinefx)
global using HealthChecks.UI.Client;
global using Microsoft.AspNetCore.Diagnostics.HealthChecks;
global using Papel.Integration.Application;
global using Papel.Integration.Infrastructure.Core;
global using Papel.Integration.Infrastructure.Core.Extensions;
//#if (EnableKafka)
global using Papel.Integration.MessageBrokers.Kafka.Extensions;
//#endif
global using Papel.Integration.Persistence.PostgreSQL.Extensions;
//#if (EnableRest)
global using Papel.Integration.Presentation.Rest.Extensions;
//#endif
global using OpenTelemetry.Resources;
global using OpenTelemetry.Trace;
//#if (EnableGraphQL)
global using Papel.Integration.Presentation.GraphQL.Extensions;
//#endif
//#if (EnableGrpc)
global using Papel.Integration.Presentation.Grpc.Extensions;
//#endif
//#if (EnableSignalR)
global using Papel.Integration.Presentation.SignalR.Extensions;
//#endif
