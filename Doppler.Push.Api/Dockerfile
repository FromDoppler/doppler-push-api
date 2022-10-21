#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Doppler.Push.Api/Doppler.Push.Api.csproj", "Doppler.Push.Api/"]
RUN dotnet restore "Doppler.Push.Api/Doppler.Push.Api.csproj"
COPY. .
WORKDIR "/src/Doppler.Push.Api"
RUN dotnet build "Doppler.Push.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Doppler.Push.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from= publish /app/publish.
ENTRYPOINT["dotnet", "Doppler.Push.Api.dll"]
