# 🌫️ Türkiye Hava Kalitesi Takip Uygulaması

Bu proje, Türkiye’deki şehirlerin **hava kalitesi verilerini** kullanıcı dostu bir şekilde görselleştirmeyi amaçlayan bir web uygulamasıdır.  
Veriler **WAQI (World Air Quality Index) API** üzerinden çekilmekte, Türkiye haritası üzerinde **Leaflet kütüphanesi** kullanılarak dinamik bir şekilde sunulmaktadır.  

---

## 🎯 Genel Amaç
- Türkiye’deki şehirlerin **hava kalitesi verilerini** tek bir platformda göstermek.  
- Kullanıcılara **şehir bazlı hava kirliliği değerlerini, ana kirletici bilgilerini, sıcaklık ve ölçüm saatlerini** sunmak.  
- Şehirlerin detay sayfalarında **1 ve 2 haftalık hava kalitesi analizini grafiklerle** göstermek.  
- Kullanıcıların hava kalitesi verilerini kolayca takip etmesini sağlamak.  

---

## ⚙️ Özellikler
- 🗺️ **Türkiye Haritası (Leaflet.js)**  
  - Tüm şehirler harita üzerinde işaretlenir.  
  - Her şehre tıklandığında, **AQI değeri (hava kalite indeksi)**, **ana kirletici**, **ölçüm saati** ve **sıcaklık bilgisi** görüntülenir.  

- 📊 **Detaylı Şehir Analizi**  
  - Şehir adına tıklanınca detay sayfası açılır.  
  - Detay sayfasında **1 ve 2 haftalık analizler** sütun grafikleri (bar chart) ile gösterilir.  
  - Bu sayede şehirlerdeki hava kalitesi değişimi izlenebilir.  

- 🌡️ **Gösterilen Veriler**  
  - Hava Kalite İndeksi (AQI)  
  - Ana kirletici (PM2.5, CO vb.)  
  - Ölçüm tarihi ve saati  
  - Sıcaklık bilgisi  

- 🗄️ **Veritabanı Desteği**  
  - Tüm veriler **PostgreSQL** veritabanında saklanır.  
  - Şehir adı, koordinat, AQI değeri, ana kirletici, tarih-saat bilgisi ve sıcaklık kayıt altında tutulur.  

---

## 🛠️ Kullanılan Teknolojiler
- **Frontend:** HTML, CSS, Leaflet.js  
- **Backend:** C# (ASP.NET Core)  
- **Veritabanı:** PostgreSQL  
- **API:** WAQI (World Air Quality Index API)  
- **Grafikler:** Chart.js (sütun grafikler için)  

---

## 🚀 Kurulum ve Çalıştırma

### 1. Depoyu Klonla
```bash
git clone <repo-link>
cd hava-kalitesi-projesi
