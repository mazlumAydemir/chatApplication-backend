# 1. Aşama: Derleme (Build) için .NET SDK kullan
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Tüm projeyi kopyala
COPY . .

# Projeyi Restore et ve Yayınla (Publish)
RUN dotnet restore "chatApplication/chatApplication.csproj"
RUN dotnet publish "chatApplication/chatApplication.csproj" -c Release -o /app/publish

# 2. Aşama: Çalıştırma (Run) için daha hafif olan ASP.NET imajını kullan
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Backend'i başlat
ENTRYPOINT ["dotnet", "chatApplication.dll"]