FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy all project files
COPY ["src/API/TCGOrderManagement.Api.csproj", "src/API/"]
COPY ["src/OrderService/OrderService.csproj", "src/OrderService/"]
COPY ["src/InventoryService/InventoryService.csproj", "src/InventoryService/"]
COPY ["src/Shared/Shared.csproj", "src/Shared/"]

# Restore dependencies
RUN dotnet restore "src/API/TCGOrderManagement.Api.csproj"

# Copy the entire source code
COPY . .

# Build the application
WORKDIR "/src/src/API"
RUN dotnet build "TCGOrderManagement.Api.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "TCGOrderManagement.Api.csproj" -c Release -o /app/publish

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Environment variables with placeholders that would be replaced in deployment
ENV ASPNETCORE_ENVIRONMENT=Production

# Database connection strings
ENV AUTH_DB_CONNECTION="Server=${DB_HOST};Database=TCGAuthDb;User Id=${DB_USER};Password=${DB_PASSWORD};"
ENV ORDERS_DB_CONNECTION="Server=${DB_HOST};Database=TCGOrdersDb;User Id=${DB_USER};Password=${DB_PASSWORD};"
ENV INVENTORY_DB_CONNECTION="Server=${DB_HOST};Database=TCGInventoryDb;User Id=${DB_USER};Password=${DB_PASSWORD};"
ENV MONGODB_CONNECTION="mongodb://${MONGODB_USER}:${MONGODB_PASSWORD}@${MONGODB_HOST}:27017"
ENV MONGODB_DATABASE="TCGInventoryDB"

# Authentication settings
ENV JWT_SECRET_KEY="${SECRET_KEY}"
ENV JWT_ISSUER="https://api.tcgordermanagement.com"
ENV JWT_AUDIENCE="https://tcgordermanagement.com"

# Messaging settings
ENV RABBITMQ_HOST="${RABBITMQ_HOST}"
ENV RABBITMQ_USERNAME="${RABBITMQ_USERNAME}"
ENV RABBITMQ_PASSWORD="${RABBITMQ_PASSWORD}"

# Run the API application (contains all functionality)
ENTRYPOINT ["dotnet", "TCGOrderManagement.Api.dll"] 