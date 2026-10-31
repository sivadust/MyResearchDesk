FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY src/MyResearchDesk.Web/MyResearchDesk.Web.csproj src/MyResearchDesk.Web/
RUN dotnet restore src/MyResearchDesk.Web/MyResearchDesk.Web.csproj
COPY src/ src/
RUN dotnet publish src/MyResearchDesk.Web/MyResearchDesk.Web.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "MyResearchDesk.Web.dll"]
