FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app
# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
ARG YAML_CONFIG_PATH
ARG METADATA_PATH
WORKDIR /app
COPY --from=build-env /app/out .
COPY $YAML_CONFIG_PATH .
COPY $METADATA_PATH .
ENTRYPOINT ["dotnet", "Lingk_SAML_Example.dll"]
