# AutoMarket.Web

Фронтенд на Angular 22 (standalone-компоненти, signals, lazy-маршрути). Працює з API `http://localhost:5299`.

```bash
npm install   # один раз
npm start     # http://localhost:4200
```

Структура `src/app`:
- `core/` — ApiService (усі запити), AuthService (JWT у localStorage), interceptor, guards, моделі
- `shared/` — картка оголошення
- `pages/` — головна, оголошення, вхід, реєстрація, мої оголошення, форма, обране
- `pages/admin/` — модерація, користувачі, статистика
