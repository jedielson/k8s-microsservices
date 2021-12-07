# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG APP_VERSION

WORKDIR /source

COPY . .
RUN dotnet restore
RUN dotnet build

# publish api
RUN dotnet publish -c release -o /app --no-restore
RUN sed -i "s/NoVersion/${APP_VERSION}/g" /app/appsettings.json

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "Assinatura.dll"]