#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["/net6-api.csproj", "/Net6-Api/"]
RUN dotnet restore "/Net6-Api/net6-api.csproj"
COPY . .
WORKDIR "/Net6-Api"
RUN dotnet build "net6-api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "net6-api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "net6-api.dll"]