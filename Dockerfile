# Multi-stage Dockerfile for BaknusITCare (.NET 8 Blazor Web App & Microsoft SQL Server Environment)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["BaknusITCare.csproj", "./"]
RUN dotnet restore "BaknusITCare.csproj"

# Copy source code and build
COPY . .
RUN dotnet build "BaknusITCare.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BaknusITCare.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Copy published binaries
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BaknusITCare.dll"]
