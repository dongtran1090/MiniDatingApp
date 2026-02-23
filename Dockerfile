# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore "MiniDatingApp.csproj"
RUN dotnet publish "MiniDatingApp.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

# Render provides PORT env var
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

CMD ["dotnet", "MiniDatingApp.dll"]
