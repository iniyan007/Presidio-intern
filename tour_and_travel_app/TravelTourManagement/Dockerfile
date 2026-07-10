# Use the official ASP.NET Core runtime as a base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy the solution and project files
COPY ["TravelTourManagement.sln", "."]
COPY ["TravelTourManagement.API/TravelTourManagement.API.csproj", "TravelTourManagement.API/"]
COPY ["TravelTourManagement.Business/TravelTourManagement.Business.csproj", "TravelTourManagement.Business/"]
COPY ["TravelTourManagement.DataAccess/TravelTourManagement.DataAccess.csproj", "TravelTourManagement.DataAccess/"]
COPY ["TravelTourManagement.Tests/TravelTourManagement.Tests.csproj", "TravelTourManagement.Tests/"]

# Restore dependencies
RUN dotnet restore "TravelTourManagement.sln"

# Copy the remaining source code
COPY . .

# Build the API project
WORKDIR "/src/TravelTourManagement.API"
RUN dotnet build "TravelTourManagement.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the app
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TravelTourManagement.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TravelTourManagement.API.dll"]
