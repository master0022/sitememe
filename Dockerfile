# =========================
#  Stage 1 — Build
# =========================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only csproj first (better build caching)
COPY BlazorApp1.csproj .

RUN dotnet restore

# Copy all project files
COPY . .

RUN dotnet publish -c Release -o /app/out

# =========================
#  Stage 2 — Runtime
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/out .

# Render assigns random PORT, we MUST listen on it
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

ENTRYPOINT ["dotnet", "BlazorApp1.dll"]
