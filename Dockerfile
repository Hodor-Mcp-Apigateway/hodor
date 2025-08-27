FROM mcr.microsoft.com/dotnet/nightly/aspnet:9.0-noble-chiseled
EXPOSE 8080
EXPOSE 8081

USER app

WORKDIR /home/app

COPY ./src/Papel.Integration.Presentation.Starter/publish/app .

ENTRYPOINT ["dotnet", "Papel.Integration.Presentation.Starter.dll"]
