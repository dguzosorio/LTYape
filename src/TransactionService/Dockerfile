FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/TransactionService/TransactionService.API/TransactionService.API.csproj", "src/TransactionService/TransactionService.API/"]
COPY ["src/TransactionService/TransactionService.Application/TransactionService.Application.csproj", "src/TransactionService/TransactionService.Application/"]
COPY ["src/TransactionService/TransactionService.Domain/TransactionService.Domain.csproj", "src/TransactionService/TransactionService.Domain/"]
COPY ["src/TransactionService/TransactionService.Infrastructure/TransactionService.Infrastructure.csproj", "src/TransactionService/TransactionService.Infrastructure/"]

RUN dotnet restore "src/TransactionService/TransactionService.API/TransactionService.API.csproj"

# Copy everything else and build
COPY ["src/TransactionService/", "src/TransactionService/"]
WORKDIR "/src/src/TransactionService/TransactionService.API"
RUN dotnet build "TransactionService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TransactionService.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TransactionService.API.dll"] 