# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files first to leverage Docker cache
COPY ["Selfix.EntryPoint/Selfix.EntryPoint.csproj", "Selfix.EntryPoint/"]
COPY ["Selfix.Application/Selfix.Application.csproj", "Selfix.Application/"]
COPY ["Selfix.Infrastructure/Selfix.Infrastructure.csproj", "Selfix.Infrastructure/"]
COPY ["Selfix.Domain/Selfix.Domain.csproj", "Selfix.Domain/"]
COPY ["Selfix.Shared/Selfix.Shared.csproj", "Selfix.Shared/"]
COPY ["Submodules/Selfix.Schema/Selfix.Schema/Selfix.Schema.csproj", "Submodules/Selfix.Schema/Selfix.Schema/"]

# Restore dependencies
RUN dotnet restore "Selfix.EntryPoint/Selfix.EntryPoint.csproj"

# Copy the rest of the code
COPY . .

# Build and publish
WORKDIR "/src/Selfix.EntryPoint"
RUN dotnet publish "Selfix.EntryPoint.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Selfix.EntryPoint.dll"]