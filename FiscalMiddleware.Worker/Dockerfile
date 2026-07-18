FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["FiscalMiddleware.Worker/FiscalMiddleware.Worker.csproj", "FiscalMiddleware.Worker/"]
COPY ["FiscalMiddleware.Application/FiscalMiddleware.Application.csproj", "FiscalMiddleware.Application/"]
COPY ["FiscalMiddleware.Domain/FiscalMiddleware.Domain.csproj", "FiscalMiddleware.Domain/"]
COPY ["FiscalMiddleware.Infrastructure/FiscalMiddleware.Infrastructure.csproj", "FiscalMiddleware.Infrastructure/"]
RUN dotnet restore "FiscalMiddleware.Worker/FiscalMiddleware.Worker.csproj"

COPY . .
WORKDIR "/src/FiscalMiddleware.Worker"
RUN dotnet build "FiscalMiddleware.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FiscalMiddleware.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FiscalMiddleware.Worker.dll"]
