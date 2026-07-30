# ---------- Build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copie des .csproj d'abord (cache des couches de restore)
COPY src/Trackwise.API/Trackwise.API.csproj                       src/Trackwise.API/
COPY src/Trackwise.Application/Trackwise.Application.csproj        src/Trackwise.Application/
COPY src/Trackwise.Domain/Trackwise.Domain.csproj                 src/Trackwise.Domain/
COPY src/Trackwise.Infrastructure/Trackwise.Infrastructure.csproj src/Trackwise.Infrastructure/
RUN dotnet restore src/Trackwise.API/Trackwise.API.csproj

# Copie du reste + publish
COPY . .
RUN dotnet publish src/Trackwise.API/Trackwise.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---------- Runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
# Dossier de logs inscriptible par l'utilisateur non-root 'app' (sinon Serilog ne peut pas écrire)
RUN mkdir -p /app/logs && chown -R app:app /app/logs
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER app
ENTRYPOINT ["dotnet", "Trackwise.API.dll"]
