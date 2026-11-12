
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["LoanManagement.IdentityService/LoanManagement.IdentityService.csproj", "LoanManagement.IdentityService/"]

RUN dotnet restore "LoanManagement.IdentityService/LoanManagement.IdentityService.csproj"

COPY . .

WORKDIR /src/LoanManagement.IdentityService

RUN dotnet publish "LoanManagement.IdentityService.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["sh", "-c", "dotnet LoanManagement.IdentityService.dll --urls http://0.0.0.0:${PORT:-8080}"]
