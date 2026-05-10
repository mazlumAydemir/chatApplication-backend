# IEA (Online Image Encryption Application) - Backend & Core System



🔗Frontend Reposu:https://github.com/mazlumAydemir/chatApplication-frontend

## 🚀 Sistemi Tek Tuşla Çalıştırma (Önerilen)

Tüm sistem (SQL Server, Redis, Backend API ve React Frontend) Docker Compose ile birbirine bağlanmıştır. Projeyi test etmek için bilgisayarınızda **Docker Desktop** yüklü olması yeterlidir.

1. Bu repoyu bilgisayarınıza indirin (Clone).
2. Terminali açın ve projenin ana dizinine gidin.
3. Aşağıdaki komutu çalıştırın:
   ```bash
   docker-compose up --build

4. Terminalde tüm servislerin ayağa kalktığını gördükten sonra tarayıcınızdan http://localhost:5173 adresine giderek uygulamayı kullanmaya başlayabilirsiniz.

🛠 Kullanılan Teknolojiler
Backend: ASP.NET Core 8, SignalR (Gerçek zamanlı sohbet)

Veritabanı: MS SQL Server & Entity Framework Core

Önbellek (Cache): Redis

Kriptografi: DES (Resim şifreleme), RSA (Oturum anahtarı takası), SHA-256 (Dijital İmza)
