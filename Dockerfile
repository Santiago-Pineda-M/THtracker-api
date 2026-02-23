FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar csproj y restaurar dependencias
COPY ["THtracker.API/THtracker.API.csproj", "THtracker.API/"]
COPY ["THtracker.Application/THtracker.Application.csproj", "THtracker.Application/"]
COPY ["THtracker.Infrastructure/THtracker.Infrastructure.csproj", "THtracker.Infrastructure/"]
RUN dotnet restore "THtracker.API/THtracker.API.csproj"

# Copiar todo y publicar
COPY . .
WORKDIR /src/THtracker.API
RUN dotnet publish "THtracker.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:80
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "THtracker.API.dll"]
