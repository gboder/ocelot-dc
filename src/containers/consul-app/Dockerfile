# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /source

# copy everything else and build app
COPY client-webapi/. ./client-webapi/
WORKDIR /source/client-webapi
RUN dotnet publish -c release -o /app --self-contained -r linux-x64

# runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0 as runtime
WORKDIR /app
COPY --from=build /app ./
CMD ["./client-webapi.dll"]

# decorate with consul