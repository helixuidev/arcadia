# CLAUDE.md — HelixUI Notifications

## Purpose
In-app notification center: real-time notifications with grouping, actions, and read/unread state.

## Key Features
- Notification bell with unread count badge
- Dropdown panel with notification list
- Grouping: by type, by date, by source
- Actions: mark read, dismiss, action buttons (approve, view, etc.)
- Real-time via SignalR (new notifications appear without refresh)
- Toast notifications (auto-dismiss with configurable duration)
- Notification preferences (mute categories)
- Persistent storage adapter (consumer provides backend)

## Architecture
- `INotificationProvider` interface for data source
- `NotificationHub` for SignalR real-time delivery
- `HelixNotificationCenter.razor` — the bell + dropdown
- `HelixToast.razor` + `HelixToastContainer.razor` — toast system
- Accessible: ARIA live region for new notifications, keyboard navigable list
