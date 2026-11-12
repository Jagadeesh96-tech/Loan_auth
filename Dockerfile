
# ==========================================
# Build Stage
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy project file
COPY ["LoanManagement.IdentityService/LoanManagement.IdentityService.csproj", "LoanManagement.IdentityService/"]

# Restore NuGet packages
RUN dotnet restore "LoanManagement.IdentityService/LoanManagement.IdentityService.csproj"

# Copy source code
COPY . .

# Publish application
WORKDIR /src/LoanManagement.IdentityService

RUN dotnet publish "LoanManagement.IdentityService.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false


# ==========================================
# Runtime Stage
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

# Install Kerberos native library
RUN apt-get update && \
    apt-get install -y --no-install-recommends libgssapi-krb5-2 && \
    rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=build /app/publish .

# Render application port
EXPOSE 8080

# Start application
ENTRYPOINT ["sh", "-c", "dotnet LoanManagement.IdentityService.dll --urls http://0.0.0.0:${PORT:-8080}"]
