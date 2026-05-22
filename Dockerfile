# 1. Aşama: Derleme (Build)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Tüm klasör yapısını kopyala (chatApplication, Domain, Application, Infrastructure)
COPY . .

# Projeyi kendi alt klasöründen restore ve publish et
RUN dotnet restore "chatApplication/chatApplication.csproj"
RUN dotnet publish "chatApplication/chatApplication.csproj" -c Release -o /app/publish

# 2. Aşama: Çalıştırma (Run)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Swagger'ın canlı ortamda da açılması için Environment değişkenini ekliyoruz
ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "chatApplication.dll"]