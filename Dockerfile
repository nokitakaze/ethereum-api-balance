# https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/building-net-docker-images?view=aspnetcore-6.0
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

LABEL maintainer="Nokita Kaze <admin@kanaria.ru>"

WORKDIR /source

# copy csproj and restore as distinct layers
COPY *.sln .
COPY global.json .
COPY WebApi/*.csproj ./WebApi/
COPY Common/*.csproj ./Common/
RUN dotnet restore

# copy everything else and build app
COPY . .
WORKDIR /source/WebApi
RUN dotnet publish -c Release -o /app

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app ./
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "EthereumAPIBalance.WebApi.dll", "--urls", "http://0.0.0.0:5267"]
