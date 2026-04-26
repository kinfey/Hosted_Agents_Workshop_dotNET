FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# Copy both projects so the ProjectReference from AgentHost -> Core resolves
COPY WorkshopLab.Core/ ./WorkshopLab.Core/
COPY WorkshopLab.AgentHost/ ./WorkshopLab.AgentHost/

WORKDIR /src/WorkshopLab.AgentHost

RUN dotnet restore
RUN dotnet build -c Release --no-restore
RUN dotnet publish -c Release --no-build -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app

RUN addgroup -S appgroup && adduser -S appuser -G appgroup

COPY --from=build /app .

RUN chown -R appuser:appgroup /app
USER appuser

EXPOSE 8088
ENTRYPOINT ["dotnet", "WorkshopLab.AgentHost.dll"]