# CLAUDE.md — HelixUI Scheduler

## Purpose
Resource scheduling component: calendar views, appointments, room booking, shift planning.

## Key Features
- Views: day, week, month, timeline (resource-based)
- Drag to create, resize to adjust duration, drag to reschedule
- Recurring events (iCal RRULE support)
- Resource columns (rooms, people, equipment)
- Conflict detection and visual indicators
- Time zone aware
- Localization ready (date formats, RTL support)
- Export to iCal/ICS

## Architecture
- `ISchedulerDataSource` for event CRUD
- Event model: start, end, title, resource, recurrence, color, metadata
- Virtual scrolling for timeline view with many resources
- SignalR integration for real-time updates (multi-user booking)
