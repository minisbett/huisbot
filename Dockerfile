# Build
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Run
FROM mcr.microsoft.com/dotnet/runtime:7.0
WORKDIR /app
COPY --from=build /app/out .
CMD [ -f "$HUIS_BOT_ENV_FILE" ] && export $(grep -v '^#' "$HUIS_BOT_ENV_FILE" | xargs) && dotnet huisbot.dll || echo "Error: HUIS_BOT_ENV_FILE ('$HUIS_BOT_ENV_FILE') not found."