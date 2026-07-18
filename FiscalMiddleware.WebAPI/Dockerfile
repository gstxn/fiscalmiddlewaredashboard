FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["FiscalMiddleware.WebAPI/FiscalMiddleware.WebAPI.csproj", "FiscalMiddleware.WebAPI/"]
COPY ["FiscalMiddleware.Application/FiscalMiddleware.Application.csproj", "FiscalMiddleware.Application/"]
COPY ["FiscalMiddleware.Domain/FiscalMiddleware.Domain.csproj", "FiscalMiddleware.Domain/"]
COPY ["FiscalMiddleware.Infrastructure/FiscalMiddleware.Infrastructure.csproj", "FiscalMiddleware.Infrastructure/"]
RUN dotnet restore "FiscalMiddleware.WebAPI/FiscalMiddleware.WebAPI.csproj"

COPY . .
WORKDIR "/src/FiscalMiddleware.WebAPI"
RUN dotnet build "FiscalMiddleware.WebAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FiscalMiddleware.WebAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FiscalMiddleware.WebAPI.dll"]
