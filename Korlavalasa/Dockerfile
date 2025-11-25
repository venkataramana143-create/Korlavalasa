# ------------ STAGE 1: BUILD ------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["Korlavalasa/Korlavalasa.csproj", "Korlavalasa/"]
RUN dotnet restore "Korlavalasa/Korlavalasa.csproj"

# Copy the full source
COPY . .

# Build & publish
WORKDIR "/src/Korlavalasa"
RUN dotnet publish -c Release -o /app/publish

# ------------ STAGE 2: RUNTIME ------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Expose Render's required port
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

ENTRYPOINT ["dotnet", "Korlavalasa.dll"]
