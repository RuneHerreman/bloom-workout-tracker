FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
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
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

# Npgsql may attempt to load GSSAPI/Kerberos native libs (even when you don't explicitly use Kerberos).
# The base ASP.NET image doesn't include them.
RUN apt-get update \
	&& apt-get install -y --no-install-recommends curl libgssapi-krb5-2 \
	&& rm -rf /var/lib/apt/lists/*

WORKDIR /app
EXPOSE 5000
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Bloom.Main.dll"]
