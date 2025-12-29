# Did I Do That? 🏠

A simple, offline-first mobile app for tracking recurring maintenance tasks. Built with .NET MAUI for iOS, Android, and Windows.

[![Develop Build](https://github.com/YOUR_USERNAME/DidIDoThatApp/actions/workflows/develop-build.yml/badge.svg)](https://github.com/YOUR_USERNAME/DidIDoThatApp/actions/workflows/develop-build.yml)
[![Release Build](https://github.com/YOUR_USERNAME/DidIDoThatApp/actions/workflows/release-build.yml/badge.svg)](https://github.com/YOUR_USERNAME/DidIDoThatApp/actions/workflows/release-build.yml)

## 📱 About

**Did I Do That?** answers one simple question:

> *"When was the last time I did this?"*

Whether it's changing your HVAC filter, rotating your tires, or replacing smoke detector batteries, this app helps you track recurring maintenance tasks without the complexity of full-featured task managers.

### Key Principles

- **Local-first**: Works fully offline with no account required
- **Simple**: No gamification, streaks, or social features
- **Low friction**: Minimal setup, fast interactions
- **Trustworthy**: Dates and status are always accurate

## ✨ Features

- **Task Management**: Create, edit, and delete recurring maintenance tasks
- **Categories**: Organize tasks into customizable categories (Home, Car, Health, etc.)
- **Smart Status Tracking**: 
  - 🟢 **Up to Date**: Task completed within the expected timeframe
  - 🟠 **Due Soon**: Task approaching its due date
  - 🔴 **Overdue**: Task past its due date
- **Completion History**: Full log of when each task was completed
- **Dashboard**: At-a-glance view of overdue and due-soon tasks
- **Local Notifications**: Get reminded when tasks become due
- **Data Export**: Export all data as JSON for backup

## 🛠️ Technology Stack

- **.NET 10** / **C# 13**
- **.NET MAUI** - Cross-platform UI framework
- **SQLite** with **Entity Framework Core** - Local data persistence
- **CommunityToolkit.Mvvm** - MVVM architecture with source generators
- **Plugin.LocalNotification** - Cross-platform local notifications
- **xUnit** / **FluentAssertions** / **Moq** - Testing framework

## 📋 Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Visual Studio 2022 (17.12+) or VS Code with C# Dev Kit
- MAUI workloads installed:
  ```bash
  dotnet workload install maui
  ```

### Platform-Specific Requirements

| Platform | Requirements |
|----------|-------------|
| Android | Android SDK, Java JDK 17 |
| iOS | macOS with Xcode 15+ |
| Windows | Windows 10 SDK (10.0.19041.0+) |

## 🚀 Getting Started

### Clone the Repository

```bash
git clone https://github.com/YOUR_USERNAME/DidIDoThatApp.git
cd DidIDoThatApp
```

### Restore Dependencies

```bash
dotnet restore
```

### Build and Run

```bash
# Android
dotnet build -f net10.0-android

# Windows
dotnet build -f net10.0-windows10.0.19041.0

# iOS (macOS only)
dotnet build -f net10.0-ios
```

### Run Tests

```bash
dotnet test DidIDoThatApp.Tests/DidIDoThatApp.Tests.csproj
```

## 📁 Project Structure

```
DidIDoThatApp/
├── DidIDoThatApp/                 # Main MAUI application
│   ├── Converters/                # Value converters for XAML bindings
│   ├── Data/                      # Database context and configuration
│   ├── Docs/                      # Product requirements and documentation
│   ├── Helpers/                   # Utility classes and constants
│   ├── Models/                    # Data models (Category, TaskItem, TaskLog)
│   ├── Platforms/                 # Platform-specific code
│   │   ├── Android/
│   │   ├── iOS/
│   │   ├── MacCatalyst/
│   │   └── Windows/
│   ├── Resources/                 # Images, fonts, styles
│   ├── Services/                  # Business logic services
│   │   └── Interfaces/
│   ├── ViewModels/                # MVVM ViewModels
│   └── Views/                     # XAML pages
├── DidIDoThatApp.Tests/           # Unit tests
│   ├── Converters/
│   ├── Helpers/
│   ├── Services/
│   └── ViewModels/
└── .github/workflows/             # CI/CD pipelines
```

## 🏗️ Architecture

The app follows the **MVVM (Model-View-ViewModel)** pattern:

- **Models**: Plain C# classes representing data entities
- **Views**: XAML pages for the UI
- **ViewModels**: Handle UI logic and state, using `CommunityToolkit.Mvvm`
- **Services**: Business logic and data access, injected via DI

### Key Services

| Service | Purpose |
|---------|---------|
| `TaskService` | CRUD operations for tasks |
| `CategoryService` | CRUD operations for categories |
| `TaskLogService` | Completion logging |
| `NotificationService` | Local notification scheduling |
| `SettingsService` | User preferences |
| `ExportService` | JSON data export |

## 🧪 Testing

The project includes comprehensive unit tests:

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test file
dotnet test --filter "FullyQualifiedName~TaskServiceTests"
```

**Current Coverage**: 142 tests covering:
- Service layer business logic
- ViewModel commands and state
- Value converters
- Status calculation helpers
- Export functionality

## 🔄 CI/CD

GitHub Actions workflows are configured for:

- **Develop Build** (`develop` branch): Runs tests, builds Android debug APK
- **Release Build** (`main` branch): Runs tests, builds Android release APK

Artifacts are uploaded and available for download from the Actions tab.

## 📖 Documentation

- [Product Requirements Document](DidIDoThatApp/Docs/prd_did_i_do_that.md)

## 🗺️ Roadmap

### MVP (Current)
- [x] Category management
- [x] Task CRUD operations
- [x] Completion logging with history
- [x] Due date and status calculation
- [x] Dashboard with overdue/due soon sections
- [x] Local notifications
- [x] Settings page
- [x] JSON data export

### Future Enhancements
- [ ] Cloud sync and backups
- [ ] Family/household sharing
- [ ] Photo attachments for tasks
- [ ] Location-based reminders
- [ ] CSV export
- [ ] Wearable companion apps

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- [.NET MAUI](https://github.com/dotnet/maui) - Cross-platform framework
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) - MVVM toolkit
- [Plugin.LocalNotification](https://github.com/thudugala/Plugin.LocalNotification) - Local notifications

---

Made with ❤️ using .NET MAUI
