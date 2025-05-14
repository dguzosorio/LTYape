FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["AntiFraudService.API/AntiFraudService.API.csproj", "AntiFraudService.API/"]
COPY ["AntiFraudService.Application/AntiFraudService.Application.csproj", "AntiFraudService.Application/"]
COPY ["AntiFraudService.Domain/AntiFraudService.Domain.csproj", "AntiFraudService.Domain/"]
COPY ["AntiFraudService.Infrastructure/AntiFraudService.Infrastructure.csproj", "AntiFraudService.Infrastructure/"]
RUN dotnet restore "AntiFraudService.API/AntiFraudService.API.csproj"
COPY . .
WORKDIR "/src/AntiFraudService.API"
RUN dotnet build "AntiFraudService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AntiFraudService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AntiFraudService.API.dll"] 