# Parola Oluşturucu

C# ve WPF ile geliştirilmiş bir Windows masaüstü uygulaması. Parolaları cihazda üretir ve parola geçmişi tutmaz.

## Özellikler

- 4–64 karakter arasında ayarlanabilir parola uzunluğu
- Büyük harf, küçük harf, rakam ve sembol seçenekleri
- Seçili her karakter türünden en az bir karakter içeren parolalar
- Parola gücü göstergesi, yeniden üretme ve panoya kopyalama

## Proje yapısı

- `Parola.xaml` — Pencere tasarımı ve arayüz bileşenleri
- `Parola.cs` — Parola üretimi, güç tahmini ve düğme işlemleri

Uygulama, karakterleri seçmek için kriptografik rastgele sayı üreteci kullanır. Uzunluk veya karakter seçimi değiştiğinde yeni bir parola oluşturur.
