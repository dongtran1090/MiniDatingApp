# Mini Dating App Prototype – Clique83 Technical Test (2026)

## Overview
A simple mini dating app prototype inspired by Breeze Dating App.  
The goal is to demonstrate the ability to build a complete feature end-to-end with clear logic and readable code.

Core features:
- Create profiles and persist data
- Like profiles and create matches (mutual likes)
- After a match, both users select availability in the next 3 weeks
- The system finds the **first common time slot** and suggests a date

---

## Features (Per Requirements)

### A) Profile Creation
- Create a profile with: **Name, Age, Gender, Bio, Email**
- Profiles are persisted in the database (data remains after reload)

### B) List & Like + Match
- Display all created profiles
- Each profile has a **Like** button
- **Match logic**:
  - If User A likes User B **and** User B likes User A → show **“It’s a Match”**
  - Matches are persisted in the database

### C) Simple Date Suggestion (Availability)
- After a match, both users can select availability within the next **3 weeks**
- Input format: **Date + From time – To time**
- When both sides have submitted availability:
  - Find the **first common overlapping slot**
  - If found → ✅ “You have a date at: [date] [time]”
  - If not found → “No common time found. Please choose again.”

---

## Tech Stack
- ASP.NET Core MVC
- Entity Framework Core
- SQL Server 2022
- Bootstrap 5

Note: No complex authentication. A “current user” is selected via dropdown and stored in Session.

---

## Data Storage
Data is stored in **SQL Server 2022** via **EF Core**, so profiles/matches/availability persist after reload.

Main entities:
- **Profile**: Name, Age, Gender, Bio, Email (Email is unique)
- **Like**: (LikerId, LikedId), unique per pair
- **Match**: (UserLowId, UserHighId), unique per pair
- **AvailabilitySlot**: MatchId, ProfileId, StartTime, EndTime
- **DateSuggestion** (optional): MatchId, StartTime, EndTime

---

## Project Structure
- **Models/**: EF entities (Profile, Like, Match, AvailabilitySlot, DateSuggestion)
- **Data/**: AppDbContext (relationships, indexes, delete behaviors)
- **Services/**: MatchService (business logic: match + first common slot)
- **Controllers/**:
  - ProfilesController (create, select current user, like)
  - MatchesController (list matches, add availability, suggest date)
- **Views/**: Razor views for Profiles and Matches

---

## Match Logic (Mutual Like)
When User A likes User B:
1. Insert Like(A, B) if it doesn’t exist
2. Check if Like(B, A) already exists
3. If yes, create a Match using a normalized pair:
   - `UserLowId = min(A, B)`
   - `UserHighId = max(A, B)`
4. Show **“It’s a Match”**

Normalization ensures A–B and B–A produce only **one** match record.

---

## First Common Slot Logic
Input:
- Sorted availability slots from both users (within the next 3 weeks)

Algorithm (interval intersection using two pointers):
1. Sort slots of A and B by StartTime
2. Use pointers `i` and `j`
3. Compute overlap:
   - `overlapStart = max(A[i].Start, B[j].Start)`
   - `overlapEnd = min(A[i].End, B[j].End)`
4. If `overlapStart < overlapEnd` → found the **first common slot**
5. Otherwise, advance the pointer of the interval that ends earlier
6. If no overlap exists → return “No common time found”

---

## Edge Cases Handled
- Unique email validation
- Must select current user before liking
- Prevent liking self
- Prevent duplicate likes
- Availability validation:
  - EndTime > StartTime
  - Slot must be within the next 3 weeks
  - Prevent overlapping slots for the same user

---

## Improvements (If More Time)
- Better calendar UI for availability selection
- Edit/delete availability slots
- Add loading states and better notifications
- Add unit tests for match and slot-intersection logic

---

## Feature Suggestions
- Chat after match
- Suggest top 3 common time slots (instead of only the first)
- Profile discovery filters (age/gender/interests)

---

## How to Run
### Prerequisites
- .NET SDK 9.0+
- SQL Server 2022 (or LocalDB)

### Setup
1. Configure connection string in `appsettings.json`
2. Run migrations:
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
3. Run the app:
dotnet run