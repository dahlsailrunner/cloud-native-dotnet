#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CarvedRock.Api/CarvedRock.Api.csproj", "CarvedRock.Api/"]
COPY ["CarvedRock.Domain/CarvedRock.Domain.csproj", "CarvedRock.Domain/"]
COPY ["CarvedRock.Core/CarvedRock.Core.csproj", "CarvedRock.Core/"]
COPY ["CarvedRock.Data/CarvedRock.Data.csproj", "CarvedRock.Data/"]
# RUN dotnet restore "./CarvedRock.Api/CarvedRock.Api.csproj" // causes a failure on package restores (maybe due to CPM?)
COPY . .
WORKDIR "/src/CarvedRock.Api"
RUN dotnet build "./CarvedRock.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CarvedRock.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CarvedRock.Api.dll"]