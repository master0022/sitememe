##########################################
# Stage 1 — Build
##########################################
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy all source code
COPY . .

# DEBUG: Show files received inside container
RUN echo "=== BUILD CONTEXT CONTENTS ===" && ls -R .

# Restore
RUN echo "=== DOTNET RESTORE ===" && dotnet restore

# Publish
RUN echo "=== DOTNET PUBLISH ===" && dotnet publish -c Release -o /app/out

# DEBUG: Show published result
RUN echo "=== PUBLISHED FILES ===" && ls -R /app/out
##########################################



##########################################
# Stage 2 — Runtime
##########################################
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app/out .

# DEBUG: Show final runtime files
RUN echo "=== FINAL IMAGE FILES ===" && ls -R .

# Render sets $PORT automatically
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

# Log PORT before launching
CMD echo "=== STARTING APP ON PORT: ${PORT} ===" && \
    echo "=== ENTRYPOINT: dotnet BlazorApp1.dll ===" && \
    dotnet BlazorApp1.dll