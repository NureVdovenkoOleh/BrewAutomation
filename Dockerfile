FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY BrewAutomation.sln .
COPY BrewAutomation/BrewAutomation.csproj ./BrewAutomation/
RUN dotnet restore

COPY . .
WORKDIR /app/BrewAutomation
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/BrewAutomation/out .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "BrewAutomation.dll"]
