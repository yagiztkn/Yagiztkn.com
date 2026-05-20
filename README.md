# MyPortfolio - İçerik Yönetim Sistemi

Bu proje, ASP.NET Core MVC ve Entity Framework Core kullanılarak N-Tier (Çok Katmanlı) Mimari prensiplerine uygun olarak geliştirilmiş kişisel bir portfolyo ve içerik yönetim platformudur.

## 🚀 Teknolojiler ve Mimari
* **Backend:** C# .NET 8.0
* **Mimari:** N-Tier Architecture (Core, DataAccess, Business, WebUI)
* **Veritabanı:** MS SQL Server
* **ORM:** Entity Framework Core (Code-First Approach)
* **Tasarım Deseni:** Repository Pattern (Generic Repository)

## 📂 Proje Yapısı
* **`MyPortfolio.Core`**: Veritabanı tablolarının (Entity) tutulduğu çekirdek katman.
* **`MyPortfolio.DataAccess`**: Veritabanı bağlantıları (DbContext) ve CRUD işlemlerinin yapıldığı katman.
* **`MyPortfolio.Business`**: İş kurallarının yönetildiği katman.
* **`MyPortfolio.WebUI`**: Ziyaretçi arayüzü ve Admin panelini barındıran MVC katmanı.