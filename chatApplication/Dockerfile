# 1. Aþama: Build (SDK kullanarak projeyi derliyoruz)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Proje dosyasýný kopyala ve baðýmlýlýklarý yükle
COPY *.csproj ./
RUN dotnet restore

# Tüm kodlarý kopyala ve yayýnla (Publish)
COPY . ./
RUN dotnet publish -c Release -o out

# 2. Aþama: Çalýþtýrma (Sadece Runtime kullanarak boyutu küçültüyoruz)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Uygulama hangi porttan çalýþacak? (Örn: 80 veya 8080)
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "chatApplication.dll"] 
# NOT: "chatApplication.dll" yazan yeri kendi projenin asýl adýna göre deðiþtir.