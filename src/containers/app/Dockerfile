# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build
WORKDIR /source

# copy everything else and build app
COPY client-webapi/. ./client-webapi/
WORKDIR /source/client-webapi
RUN dotnet publish -c release -o /app

# runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine as runtime
WORKDIR /app
COPY --from=build /app ./
CMD ["dotnet" "client-webapi.dll"]