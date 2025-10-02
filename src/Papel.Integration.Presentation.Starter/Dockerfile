# Use the official ASP.NET Core runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 81

# Use the official .NET SDK for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project build configuration
COPY ["Directory.Build.props", "./"]

# Copy csproj files and restore dependencies
COPY ["src/Papel.Integration.Presentation.Starter/Papel.Integration.Presentation.Starter.csproj", "src/Papel.Integration.Presentation.Starter/"]
COPY ["src/Papel.Integration.Presentation.Rest/Papel.Integration.Presentation.Rest.csproj", "src/Papel.Integration.Presentation.Rest/"]
COPY ["src/Papel.Integration.Presentation.GraphQL/Papel.Integration.Presentation.GraphQL.csproj", "src/Papel.Integration.Presentation.GraphQL/"]
COPY ["src/Papel.Integration.Presentation.Grpc/Papel.Integration.Presentation.Grpc.csproj", "src/Papel.Integration.Presentation.Grpc/"]
COPY ["src/Papel.Integration.Presentation.SignalR/Papel.Integration.Presentation.SignalR.csproj", "src/Papel.Integration.Presentation.SignalR/"]
COPY ["src/Papel.Integration.Application/Papel.Integration.Application.csproj", "src/Papel.Integration.Application/"]
COPY ["src/Papel.Integration.Domain/Papel.Integration.Domain.csproj", "src/Papel.Integration.Domain/"]
COPY ["src/Papel.Integration.Infrastructure.Core/Papel.Integration.Infrastructure.Core.csproj", "src/Papel.Integration.Infrastructure.Core/"]
COPY ["src/Papel.Integration.Persistence.PostgreSQL/Papel.Integration.Persistence.PostgreSQL.csproj", "src/Papel.Integration.Persistence.PostgreSQL/"]
COPY ["src/Papel.Integration.Persistence.Redis/Papel.Integration.Persistence.Redis.csproj", "src/Papel.Integration.Persistence.Redis/"]
COPY ["src/Papel.Integration.MessageBrokers.RabbitMq/Papel.Integration.MessageBrokers.RabbitMq.csproj", "src/Papel.Integration.MessageBrokers.RabbitMq/"]
COPY ["src/Papel.Integration.Events/Papel.Integration.Events.csproj", "src/Papel.Integration.Events/"]
COPY ["src/Papel.Integration.Common/Papel.Integration.Common.csproj", "src/Papel.Integration.Common/"]
COPY ["src/Papel.Integration.EFCore.Caching.Redis/Papel.Integration.EFCore.Caching.Redis.csproj", "src/Papel.Integration.EFCore.Caching.Redis/"]

RUN dotnet restore "src/Papel.Integration.Presentation.Starter/Papel.Integration.Presentation.Starter.csproj" \
    --locked-mode false \
    --verbosity minimal \
    --disable-parallel \
    /p:TreatWarningsAsErrors=false \
    /p:WarningsAsErrors="" \
    /p:NoWarn="NU1604;NU1701;NU1603;NU1605" || true

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/src/Papel.Integration.Presentation.Starter"
RUN dotnet build "Papel.Integration.Presentation.Starter.csproj" -c $BUILD_CONFIGURATION -o /app/build \
    -p:TreatWarningsAsErrors=false \
    -p:RunAnalyzersDuringBuild=false \
    -p:EnableNETAnalyzers=false

# Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Papel.Integration.Presentation.Starter.csproj" -c $BUILD_CONFIGURATION -o /app/publish \
    /p:UseAppHost=false \
    -p:TreatWarningsAsErrors=false \
    -p:RunAnalyzersDuringBuild=false \
    -p:EnableNETAnalyzers=false

# Final stage - create runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create non-root user for security
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "Papel.Integration.Presentation.Starter.dll"]
