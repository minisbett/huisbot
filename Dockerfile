# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Run
FROM mcr.microsoft.com/dotnet/runtime:8.0
RUN apt-get update && apt-get install -y libgdiplus
WORKDIR /app
COPY --from=build /app/out .
CMD dotnet huisbot.dll