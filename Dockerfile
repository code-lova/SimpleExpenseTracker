# Use the official ASP.NET core image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Use SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["SimpleExpenseTracker/SimpleExpenseTracker.csproj", "SimpleExpenseTracker/"]
RUN dotnet restore "SimpleExpenseTracker/SimpleExpenseTracker.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/SimpleExpenseTracker"
RUN dotnet publish -c Release -o /app/publish

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SimpleExpenseTracker.dll"]