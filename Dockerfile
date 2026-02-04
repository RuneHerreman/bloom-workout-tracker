FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files first
COPY ["src/Bloom.Main/Bloom.Main.csproj", "src/Bloom.Main/"]
COPY ["src/Bloom.Application/Bloom.Application.csproj", "src/Bloom.Application/"]
COPY ["src/Bloom.Domain/Bloom.Domain.csproj", "src/Bloom.Domain/"]
COPY ["src/Bloom.Infrastructure/Bloom.Infrastructure.csproj", "src/Bloom.Infrastructure/"]

# Restore
RUN dotnet restore "src/Bloom.Main/Bloom.Main.csproj"

# Copy source
COPY . .

# Build & Publish - STAY IN /src and use FULL PATH
RUN dotnet publish "src/Bloom.Main/Bloom.Main.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 5000
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Bloom.Main.dll"]
