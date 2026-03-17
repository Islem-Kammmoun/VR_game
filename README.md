# VR Forest Route Game (Ask AI / Verify Sources)

This is a Unity VR game where the player asks an “AI” for a route through a forest, then chooses whether to **follow the route immediately** (risking a wrong path) or **verify sources** to unlock the correct route.

The gameplay is driven mainly by `Assets/Scripts/Core/GameManager.cs`, plus:

- `RouteRenderer` (shows the visible route line: wrong vs correct)
- `WaypointNavigator` (teleports the player waypoint-by-waypoint)
- `RouteFollowPanelController` (controls the RouteFollow UI phases: decision vs following)

---

## Core Concept

The player gets navigation help from an in-game “AI”, but:

- **Default (unverified) guidance is WRONG**
- If the player **verifies sources**, the game shows a verification message sequence and then unlocks the **CORRECT route**

There is also a “lose hole” at waypoint **W4** (a trigger). The player can fall into it and find the lose UI panel in the lower level.

---

## Game States

`GameManager.GameState`:

- **Intro**: Intro UI panel shown
- **ModeSelect**: Mode selection UI panel shown
- **Playing**: main gameplay UI shown (Ask AI, route follow UI, etc.)
- **Won**: Win panel shown
- **Lost**: Lost state reached (LosePanel-1 is always shown in the scene, and the state is used for game logic/cleanup)

> Note: In the current setup, **LosePanel-1 is always active from the beginning**, but the game still uses `Lost` state to represent that the player lost.

---

## Main UI Panels

These are the “main screens” controlled by `GameManager.UpdatePanels()`:

- `IntroPanel`
- `ModePanel`
- `WinPanel`

Lose UI:

- **LosePanel-1** is placed in the hole/lower ground area and is configured to be **always shown** (always active).  
  The Lose flow can optionally snap it in front of the player camera when a loss happens.

Overlay UI (shown during Playing):

- `RouteFollowPanel`
- `VerifySourcesPanel`
- `VerificationProcessPanel`

---

## Route System

There are two routes:

- **Wrong route**: W1 → W4 (leads to the hole / lose trigger)
- **Correct route**: C1 → C5 (unlocked after verification)

`RouteRenderer` is responsible for the route visualization:

- `ShowWrongRoute()`
- `ShowCorrectRoute()`
- `Hide()`

`WaypointNavigator` is responsible for teleport navigation:

- `SetRoute(bool useCorrectRoute)`
- `TeleportNext()`
- `HasNextWaypoint`

---

## Game Flow (Step-by-Step)

### 1) Start / Intro

- Game starts in **Intro** state.
- `IntroPanel` is shown.
- Player is typically at the spawn location.

### 2) Start Playing

- UI action calls `GameManager.StartAlone()` (or similar), which sets state to **Playing**.
- `persistentAskAiPanel` becomes visible.

### 3) Ask AI (Default = Wrong Route)

- Player presses **Ask AI** button.
- `GameManager.AskAI()` runs:
  - Sets state to **Playing** (if not already)
  - Selects route based on `_useCorrectRoute`
  - By default `_useCorrectRoute = false` → WRONG route is selected
  - Shows `RouteFollowPanel` in **Decision phase** (handled by `RouteFollowPanelController.OnEnable()`)

### 4) Decision Phase (RouteFollowPanel)

The player chooses one of these:

#### A) Continue Anyway (No Verification)

- Calls `GameManager.ForceWrongRouteAndOpenFollow()`
- Forces `_useCorrectRoute = false`
- Shows wrong route visualization
- Forces `RouteFollowPanelController.EnterFollowingPhase()`
  - This makes the panel show **Teleport Next** (following phase)

#### B) Verify Sources

- Calls `GameManager.ShowVerifySourcesPanel()`
- Hides `RouteFollowPanel`
- Opens `VerifySourcesPanel`

### 5) VerifySourcesPanel → Look for Updated Sources

- Calls `GameManager.UpdateSourceAndNewRoute()`
- Runs a timed message sequence inside `VerificationProcessPanel`:
  1. “Looking for newer information in the forest signs…”
  2. “Attention: holes next to rocks.”
  3. “Updating sources…”
- After the sequence:
  - `_useCorrectRoute = true`
  - `AskAI()` is called (to open the route follow UI)
  - `EnterFollowingPhase()` is forced so the panel is **Follow phase only**
    - “text + Teleport Next only”
    - no Continue Anyway / Verify buttons

### 6) Following Phase (Teleportation)

- Player uses **Teleport Next**.
- `GameManager.TeleportNextWaypoint()`:
  - Teleports to the next waypoint
  - Keeps the RouteFollowPanel snapped in front of the player
  - Closes the panel when there are no more waypoints

### 7) Win Condition

- When the correct route is completed (C5), game triggers:
  - `GameManager.Win()`
  - State becomes **Won**
  - `WinPanel` is shown

### 8) Play Again (WinPanel)

- `WinPanel` has a **Play Again** button:
  - OnClick → `GameManager.PlayAgainFromWin()`
- This resets the run and returns to **Intro** (Option A).

---

## Lose Flow (Hole at W4)

- Waypoint **W4** is a hole trigger that causes a loss.
- When losing, game triggers:
  - `GameManager.Lose(reason)`
  - `_hasLostOnce = true`
  - State becomes **Lost**
  - The lose reason text can be written into LosePanel-1 (`loseReasonText`)
  - Optionally the LosePanel-1 is snapped in front of the player (if a RectTransform is assigned)

### Try Again (LosePanel-1)

- LosePanel-1 has a **Try Again** button:
  - OnClick → `GameManager.TryAgainFromLose()`
- This resets the run and returns to **Intro** (Option A).

---

## Inspector Setup Checklist (Important)

### GameManager references

Assign these in the Unity Inspector:

- Player:
  - `xrOriginRoot`
  - `spawnPoint`
- UI panels:
  - `introPanel`
  - `modePanel`
  - `winPanel`
- Lose panel:
  - `losePanelLower` = LosePanel-1
  - `loseReasonText` (optional)
  - `losePanelLowerRect` (optional; only if you want snapping)
- Overlay panels:
  - `routeFollowPanel`
  - `verifySourcesPanel`
  - `verificationProcessPanel`
  - `verificationProcessPanelRect`
  - `verificationProcessText`
- Snapping:
  - `xrCamera`
  - `routeFollowPanelRect`

### Button wiring

- Ask AI button → `GameManager.AskAI()`
- RouteFollowPanel Continue Anyway → `GameManager.ForceWrongRouteAndOpenFollow()`
- RouteFollowPanel Verify Sources → `GameManager.ShowVerifySourcesPanel()`
- VerifySourcesPanel Look for Updated Sources → `GameManager.UpdateSourceAndNewRoute()`
- RouteFollowPanel Teleport Next → `GameManager.TeleportNextWaypoint()`
- WinPanel Play Again → `GameManager.PlayAgainFromWin()`
- LosePanel-1 Try Again → `GameManager.TryAgainFromLose()`

---

## Notes / Design Intent

- The “signs” in the forest are assumed to exist in the world; the game **does not enable/disable sign GameObjects**.
- The “verification” is represented through the **VerificationProcessPanel** message sequence, then switching routes.
- The “wrong route” is intended to teach the player to verify sources.

---
