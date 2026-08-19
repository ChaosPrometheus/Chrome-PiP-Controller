# Chrome PiP Controller

Лёгкая portable-утилита для Windows, которая автоматически управляет окнами **Picture-in-Picture** в Chrome и Chromium-браузерах.

## ✨ Возможности

* 🔍 Автоматическое обнаружение PiP
* 📐 Настройка размера и положения
* 📌 Всегда поверх окон
* 🖱️ Клики насквозь
* 👻 Прозрачность 20–100%
* 💾 Сохранение настроек
* 🚀 Автозапуск Windows
* 🔽 Работа в системном трее
* 🪟 Ручной выбор окна Chrome
* 🌐 Русский / английский интерфейс

## 🚀 Использование

1. Запустите `ChromePiPController.exe`.
2. Откройте видео в Chrome.
3. Включите **Picture-in-Picture**.
4. Программа автоматически найдёт PiP и применит настройки.

Установка не требуется.

## 🔨 Сборка

**Русская версия:**

```bash
csc /out:ChromePiPController.exe ChromePiPController.cs
```

**Английская версия:**

```bash
csc /define:ENGLISH /out:ChromePiPController.exe ChromePiPController.cs
```

### Требования

* Windows 10 / 11
* .NET Framework 4.7.2+
* Chrome или Chromium-браузер
