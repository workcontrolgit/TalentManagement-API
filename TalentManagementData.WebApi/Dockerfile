FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/TalentManagementData.WebApi/TalentManagementData.WebApi.csproj", "src/TalentManagementData.WebApi/"]
COPY ["src/TalentManagementData.Application/TalentManagementData.Application.csproj", "src/TalentManagementData.Application/"]
COPY ["src/TalentManagementData.Domain/TalentManagementData.Domain.csproj", "src/TalentManagementData.Domain/"]
COPY ["src/TalentManagementData.Infrastructure.Persistence/TalentManagementData.Infrastructure.Persistence.csproj", "src/TalentManagementData.Infrastructure.Persistence/"]
COPY ["src/TalentManagementData.Infrastructure.Shared/TalentManagementData.Infrastructure.Shared.csproj", "src/TalentManagementData.Infrastructure.Shared/"]

RUN dotnet restore "src/TalentManagementData.WebApi/TalentManagementData.WebApi.csproj"

COPY . .
RUN dotnet publish "src/TalentManagementData.WebApi/TalentManagementData.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

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
ENTRYPOINT ["dotnet", "TalentManagementData.WebApi.dll"]

