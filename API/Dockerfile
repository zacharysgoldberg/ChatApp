FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /API
EXPOSE 80
EXPOSE 81

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /API
COPY *.csproj ./
RUN dotnet restore
COPY . ./
WORKDIR /API
RUN dotnet build -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Create the final runtime image
FROM base AS final
WORKDIR /API
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "API.dll"]