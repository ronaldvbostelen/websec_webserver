FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["ReversiServer/ReversiServer.csproj", "ReversiServer/"]
RUN dotnet restore "ReversiServer/ReversiServer.csproj"
COPY . .
WORKDIR "/src/ReversiServer"
RUN dotnet build "ReversiServer.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "ReversiServer.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "ReversiServer.dll"]