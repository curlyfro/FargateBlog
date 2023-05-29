#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["FargateBlog.Containers.Encoder/FargateBlog.Containers.Encoder.csproj", "FargateBlog.Containers.Encoder/"]
COPY ["FargateBlog.Core/FargateBlog.Core.csproj", "FargateBlog.Core/"]
RUN dotnet restore "FargateBlog.Containers.Encoder/FargateBlog.Containers.Encoder.csproj"
COPY . .
WORKDIR "/src/FargateBlog.Containers.Encoder"
RUN dotnet build "FargateBlog.Containers.Encoder.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FargateBlog.Containers.Encoder.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final

RUN apt update && apt upgrade
RUN apt install ffmpeg -y

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FargateBlog.Containers.Encoder.dll"]