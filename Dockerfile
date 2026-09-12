FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY VehicleParkingManagementSystem/VehicleParkingManagementSystem.csproj VehicleParkingManagementSystem/
RUN dotnet restore VehicleParkingManagementSystem/VehicleParkingManagementSystem.csproj
COPY VehicleParkingManagementSystem/ VehicleParkingManagementSystem/
RUN dotnet publish VehicleParkingManagementSystem/VehicleParkingManagementSystem.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet VehicleParkingManagementSystem.dll"]
