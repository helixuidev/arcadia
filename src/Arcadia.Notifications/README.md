# Arcadia.Notifications

Toast notifications and notification center for Blazor — auto-dismiss, stacking, real-time via SignalR.

## Install

```bash
dotnet add package Arcadia.Notifications
```

## Quick Start

Register the service in `Program.cs`:

```csharp
builder.Services.AddScoped<ToastService>();
```

Add the toast container to your layout:

```razor
<HelixToastContainer Position="ToastPosition.TopRight" />
```

Show notifications from any component:

```csharp
@inject ToastService ToastService

ToastService.ShowSuccess("Record saved successfully.");
ToastService.ShowError("Connection lost.");
ToastService.ShowWarning("Disk space running low.");
ToastService.ShowInfo("New version available.");
```

## Features

- **Toast types** — Success, Error, Warning, Info, Custom
- **Notification center** — bell icon with unread count, dropdown panel, grouping by type/date
- **Real-time** — SignalR integration for live notification delivery
- **Configurable** — auto-dismiss duration, max visible, stacking, click-to-dismiss

## Key Features

Auto-dismiss · Stacking with max limit · Notification history · Action buttons · ARIA live region accessible · All Blazor render modes · .NET 5–10

**[Docs](https://arcadiaui.com/docs/notifications)** · **[Demo](https://arcadiaui.com/playground/)** · **[GitHub](https://github.com/ArcadiaUIDev/arcadia)**
