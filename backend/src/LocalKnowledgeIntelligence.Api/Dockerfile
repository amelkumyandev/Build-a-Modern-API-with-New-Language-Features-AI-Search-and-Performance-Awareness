FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY LocalKnowledgeIntelligence.sln ./
COPY src/LocalKnowledgeIntelligence.Api/LocalKnowledgeIntelligence.Api.csproj src/LocalKnowledgeIntelligence.Api/
COPY src/LocalKnowledgeIntelligence.Application/LocalKnowledgeIntelligence.Application.csproj src/LocalKnowledgeIntelligence.Application/
COPY src/LocalKnowledgeIntelligence.Contracts/LocalKnowledgeIntelligence.Contracts.csproj src/LocalKnowledgeIntelligence.Contracts/
COPY src/LocalKnowledgeIntelligence.Domain/LocalKnowledgeIntelligence.Domain.csproj src/LocalKnowledgeIntelligence.Domain/
COPY src/LocalKnowledgeIntelligence.Infrastructure/LocalKnowledgeIntelligence.Infrastructure.csproj src/LocalKnowledgeIntelligence.Infrastructure/
RUN dotnet restore src/LocalKnowledgeIntelligence.Api/LocalKnowledgeIntelligence.Api.csproj
COPY . .
RUN dotnet publish src/LocalKnowledgeIntelligence.Api/LocalKnowledgeIntelligence.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "LocalKnowledgeIntelligence.Api.dll"]
