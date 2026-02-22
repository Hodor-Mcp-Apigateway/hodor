FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props ./
COPY src/Hodor.Core/Hodor.Core.csproj src/Hodor.Core/
COPY src/Hodor.Application.Mcp/Hodor.Application.Mcp.csproj src/Hodor.Application.Mcp/
COPY src/Hodor.Infrastructure.Core/Hodor.Infrastructure.Core.csproj src/Hodor.Infrastructure.Core/
COPY src/Hodor.Infrastructure.ProcessManager/Hodor.Infrastructure.ProcessManager.csproj src/Hodor.Infrastructure.ProcessManager/
COPY src/Hodor.Persistence/Hodor.Persistence.csproj src/Hodor.Persistence/
COPY src/Hodor.Host/Hodor.Host.csproj src/Hodor.Host/

RUN dotnet restore src/Hodor.Host/Hodor.Host.csproj

COPY src/ src/
COPY mcp-config.json ./
RUN dotnet publish src/Hodor.Host/Hodor.Host.csproj -c Release -o /app/publish
COPY mcp-config.json /app/publish/

FROM mcr.microsoft.com/dotnet/aspnet:10.0
EXPOSE 8080

WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Hodor.Host.dll"]
