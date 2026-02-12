# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ApiResources/TalentManagement-API/TalentManagementAPI.WebApi/TalentManagementAPI.WebApi.csproj", "ApiResources/TalentManagement-API/TalentManagementAPI.WebApi/"]
COPY ["ApiResources/TalentManagement-API/TalentManagementAPI.Application/TalentManagementAPI.Application.csproj", "ApiResources/TalentManagement-API/TalentManagementAPI.Application/"]
COPY ["ApiResources/TalentManagement-API/TalentManagementAPI.Domain/TalentManagementAPI.Domain.csproj", "ApiResources/TalentManagement-API/TalentManagementAPI.Domain/"]
COPY ["ApiResources/TalentManagement-API/TalentManagementAPI.Infrastructure.Persistence/TalentManagementAPI.Infrastructure.Persistence.csproj", "ApiResources/TalentManagement-API/TalentManagementAPI.Infrastructure.Persistence/"]
COPY ["ApiResources/TalentManagement-API/TalentManagementAPI.Infrastructure.Shared/TalentManagementAPI.Infrastructure.Shared.csproj", "ApiResources/TalentManagement-API/TalentManagementAPI.Infrastructure.Shared/"]
RUN dotnet restore "./ApiResources/TalentManagement-API/TalentManagementAPI.WebApi/TalentManagementAPI.WebApi.csproj"
COPY . .
WORKDIR "/src/ApiResources/TalentManagement-API/TalentManagementAPI.WebApi"
RUN dotnet build "./TalentManagementAPI.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TalentManagementAPI.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TalentManagementAPI.WebApi.dll"]