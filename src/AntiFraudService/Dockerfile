FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/AntiFraudService/AntiFraudService.API/AntiFraudService.API.csproj", "src/AntiFraudService/AntiFraudService.API/"]
COPY ["src/AntiFraudService/AntiFraudService.Application/AntiFraudService.Application.csproj", "src/AntiFraudService/AntiFraudService.Application/"]
COPY ["src/AntiFraudService/AntiFraudService.Domain/AntiFraudService.Domain.csproj", "src/AntiFraudService/AntiFraudService.Domain/"]
COPY ["src/AntiFraudService/AntiFraudService.Infrastructure/AntiFraudService.Infrastructure.csproj", "src/AntiFraudService/AntiFraudService.Infrastructure/"]

RUN dotnet restore "src/AntiFraudService/AntiFraudService.API/AntiFraudService.API.csproj"

# Copy everything else and build
COPY ["src/AntiFraudService/", "src/AntiFraudService/"]
WORKDIR "/src/src/AntiFraudService/AntiFraudService.API"
RUN dotnet build "AntiFraudService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AntiFraudService.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AntiFraudService.API.dll"] 