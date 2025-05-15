# Use the full SDK since we're running dotnet run (not prebuilding)
FROM mcr.microsoft.com/dotnet/sdk:8.0

# Set working directory
WORKDIR /app

# Copy everything into the container
COPY . .

# Default command: run the app
ENTRYPOINT ["dotnet", "run", "--"]
