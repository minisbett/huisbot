# Build
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Run
FROM mcr.microsoft.com/dotnet/runtime:7.0
CMD sh -c 'export $(cat "$HUIS_BOT_ENV_FILE" | xargs)'
WORKDIR /app
COPY --from=build /app/out .
CMD dotnet huisbot.dll