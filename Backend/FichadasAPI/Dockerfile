# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar csproj y restaurar dependencias
COPY FichadasAPI.csproj .
RUN dotnet restore

# Copiar el resto del codigo y compilar
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Etapa de runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Instalar dependencias para SQL Server
RUN apt-get update && apt-get install -y --no-install-recommends \
    libicu-dev \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Exponer puerto 5210
EXPOSE 5210

# Variables de entorno para ASP.NET
ENV ASPNETCORE_URLS=http://+:5210
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "FichadasAPI.dll"]
