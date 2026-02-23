# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# copy all source
COPY . .

# restore & publish the web project
RUN dotnet restore "MiniDatingApp/MiniDatingApp.csproj"
RUN dotnet publish "MiniDatingApp/MiniDatingApp.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

# Render provides PORT env var
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

# start app
CMD ["dotnet", "MiniDatingApp.dll"]