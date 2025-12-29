# Product Requirements Document (PRD)

## Product Name
**Did I Do That?**

## Platform
- iOS
- Android
- Built using **.NET MAUI**

---

## 1. Overview

### Problem Statement
People regularly forget when recurring maintenance tasks were last completed (home, vehicle, health, pets, personal). Memory, notes apps, and calendars are unreliable or cumbersome for this purpose.

### Goal
Provide a simple, offline-first mobile app that clearly answers:

> **“When was the last time I did this?”**

The app should prioritize clarity, speed, and low cognitive load.

### Target Audience
- Homeowners and renters
- Vehicle owners
- Caregivers
- Busy adults who want reminders without complexity

---

## 2. Core Principles

- **Local-first**: Works fully offline
- **Simple**: No gamification, streaks, or social features
- **Low friction**: Minimal setup, fast interactions
- **Trustworthy**: Dates and status must always be correct

---

## 3. Non-Goals (Explicitly Out of Scope for MVP)

- User accounts or authentication
- Cloud sync or backups
- AI recommendations
- Calendar integrations
- Social or sharing features
- Analytics dashboards or charts

---

## 4. User Stories

### Task Management
- As a user, I can create a recurring maintenance task.
- As a user, I can edit or delete an existing task.
- As a user, I can group tasks into categories.

### Task Completion
- As a user, I can mark a task as completed.
- As a user, I can see when it was last completed.
- As a user, I can see how overdue a task is.

### Awareness
- As a user, I can quickly see which tasks are overdue.
- As a user, I can receive a reminder when a task becomes due.

---

## 5. Functional Requirements

### 5.1 Categories

Users can organize tasks into categories.

**Category Fields**
- Id (GUID)
- Name (string, required)
- Icon (string, optional)
- CreatedDate (DateTime)

**Category Actions**
- Create category
- Edit category
- Delete category

---

### 5.2 Tasks (Maintenance Items)

Each task represents a recurring maintenance action.

**Task Fields**
- Id (GUID)
- CategoryId (GUID)
- Name (string, required)
- Description (string, optional)
- FrequencyValue (int)
- FrequencyUnit (Days | Weeks | Months)
- IsReminderEnabled (bool)
- CreatedDate (DateTime)

**Task Actions**
- Create task
- Edit task
- Delete task
- View task list grouped by category

---

### 5.3 Completion Logging

Tasks are never "reset". Each completion is logged.

**TaskLog Fields**
- Id (GUID)
- TaskItemId (GUID)
- CompletedDate (DateTime)

**Completion Rules**
- Marking a task complete creates a new TaskLog entry
- LastCompletedDate is derived from most recent TaskLog
- Manual backdating is optional but allowed

---

## 6. Business Logic

### 6.1 Due Date Calculation

```
If Task has at least one completion:
    DueDate = LastCompletedDate + Frequency
Else:
    Task is considered Overdue
```

### 6.2 Status Rules

- **Up To Date**: DueDate > Now
- **Due Soon**: DueDate within 20% of frequency interval
- **Overdue**: DueDate < Now

Status must update immediately after task completion.

---

## 7. Dashboard Requirements

The dashboard provides a glanceable overview.

### Dashboard Sections
- Overdue Tasks
- Due Soon Tasks
- Recently Completed Tasks

### Constraints
- No charts
- No gamification
- Focus on clarity and readability

---

## 8. Notifications

### Notification Type
- Local notifications only

### Rules
- One notification per task per due event
- Notification scheduled when task becomes due
- Notification schedule recalculated on task completion

### Settings
- Global notifications on/off
- Default reminder lead time (days)

---

## 9. Settings

**MVP Settings**
- Enable/disable notifications
- Default reminder lead time
- Optional: Export data to JSON

---

## 10. Navigation & UI

### Navigation Structure
- Bottom tab navigation:
  - Dashboard
  - Tasks
  - Categories
  - Settings

### Required Screens
- Dashboard
- Task List (by category)
- Task Detail
- Add/Edit Task
- Category Management
- Settings

---

## 11. Technical Requirements

### Architecture
- MVVM pattern
- .NET MAUI
- Dependency Injection via MAUI Host

### Persistence
- Local database only
- SQLite (via EF Core or SQLite-net)

### Offline Behavior
- App must function fully without internet access

---

## 12. Acceptance Criteria

The MVP is considered complete when:

- Categories can be created, edited, and deleted
- Tasks can be created, edited, deleted, and completed
- Task due dates and status are calculated correctly
- Overdue and due-soon tasks are clearly indicated
- Local notifications fire when tasks become due
- App functions entirely offline

---

## 13. Future Enhancements (Post-MVP)

- Cloud sync and backups
- Family or household sharing
- Photo attachments
- Location-based reminders
- CSV export
- Wearable companion apps

---

## 14. Copilot Agent Instructions

When implementing this PRD:

- Do NOT add cloud sync or authentication
- Favor simplicity and readability over abstraction
- Use clear, testable business logic
- Avoid premature optimization
- Use platform-agnostic MAUI controls where possible

---

**End of PRD**

