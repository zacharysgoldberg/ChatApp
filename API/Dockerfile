# [Use the SDK image to build the application]
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /API
# EXPOSE 80

COPY *.sln ./
COPY *.csproj ./
RUN dotnet restore
# [Copy everything else]
COPY . ./
WORKDIR /API

# [Build app]
FROM build as publish
RUN dotnet publish -c Release -o /app/publish
# RUN dotnet tool install --global dotnet-ef --version 7.0.14
# ENV PATH="$PATH:/root/.dotnet/tools"
# RUN dotnet ef database update

# [Create the final runtime image]
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /API
COPY --from=publish /app/publish .

# Copy the .env file into the image
# COPY .env ./

ENTRYPOINT ["dotnet", "API.dll"]

