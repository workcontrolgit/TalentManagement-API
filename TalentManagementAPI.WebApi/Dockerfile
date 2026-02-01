FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/TalentManagementAPI.WebApi/TalentManagementAPI.WebApi.csproj", "src/TalentManagementAPI.WebApi/"]
COPY ["src/TalentManagementAPI.Application/TalentManagementAPI.Application.csproj", "src/TalentManagementAPI.Application/"]
COPY ["src/TalentManagementAPI.Domain/TalentManagementAPI.Domain.csproj", "src/TalentManagementAPI.Domain/"]
COPY ["src/TalentManagementAPI.Infrastructure.Persistence/TalentManagementAPI.Infrastructure.Persistence.csproj", "src/TalentManagementAPI.Infrastructure.Persistence/"]
COPY ["src/TalentManagementAPI.Infrastructure.Shared/TalentManagementAPI.Infrastructure.Shared.csproj", "src/TalentManagementAPI.Infrastructure.Shared/"]

RUN dotnet restore "src/TalentManagementAPI.WebApi/TalentManagementAPI.WebApi.csproj"

COPY . .
RUN dotnet publish "src/TalentManagementAPI.WebApi/TalentManagementAPI.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

RUN useradd --create-home --uid 10001 appuser
USER appuser

EXPOSE 8080
EXPOSE 8443

HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 CMD curl -f http://localhost:8080/health || exit 1

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TalentManagementAPI.WebApi.dll"]

