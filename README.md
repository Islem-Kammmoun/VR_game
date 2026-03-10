# VR Game – Verify Before You Rely

A VR navigation prototype built with **Unity 6000.2.10f1** and the **XR Interaction Toolkit**.
The player wakes up in a forest and must find a water lake to survive. A rule-based AI assistant
can give directions — but it initially offers the **wrong route** based on an outdated map.
To win safely the player must question the AI's sources, choose to update them, and only then
follow the corrected path to the lake.

---

## Gameplay Flow

```
IntroPanel ──[Start]──► ModePanel
                           │
              ┌────────────┴────────────┐
        [Start Alone]             [Ask AI]
              │                        │
              ▼                        ▼
          Playing ◄──────────── Playing + RouteFollowPanel
        (free roam)              (wrong route W1→W4)
              │                        │
        (persistent Ask AI btn)   (teleport steps)
              │                        │
              └───────────────────► HoleTrigger
                                        │
                                    LosePanel
                                        │
                                   [Try Again]
                                        │
                                    Playing
                              (Ask AI  +  Verify btn)
                                        │
                              [Verify Route & Sources]
                                        │
                                 VerifySourcesPanel
                              "route based on 1974 map"
                                   │         │
                         [Update Source]  [Continue Anyway]
                                   │         │
                             correct route  wrong route
                             C1→C5          W1→W4
                                   │
                               LakeTrigger
                                   │
                               WinPanel
```

---

## Unity Version

```
6000.2.10f1
```

---

## How to Open the Project

1. Install **Unity 6000.2.10f1** from Unity Hub.
2. In Unity Hub choose **Open → Add project from disk** and select this repository folder.
3. Wait for Unity to import all assets (first open may take several minutes).
4. Open the target scene:
   **Assets/Scenes/VerifyBeforeYouRely_Level.unity**

---

## Scene Wiring – VerifyBeforeYouRely_Level

Follow these steps in the Unity Editor to wire up the gameplay scripts.

### 1. Open the Scene

```
Assets/Scenes/VerifyBeforeYouRely_Level.unity
```

### 2. Tag the XR Origin as "Player"

1. Select the **XR Origin** (or **XR Origin Hands (XR Rig)**) GameObject in the Hierarchy.
2. In the Inspector, set **Tag** to `Player`.
   > ⚠️ Only tag the XR Origin root — do **not** tag UI objects as Player.

### 3. Create the PlayerSpawn Transform

1. Create an empty GameObject named `PlayerSpawn`.
2. Place it at the starting area.
3. Rotate it so its blue (forward) arrow faces the intended starting direction.

### 4. Create Trigger Zones

#### HoleTrigger

1. Create an empty GameObject named `HoleTrigger`.
2. Add a **BoxCollider** → check **Is Trigger**.
3. Position and scale it to cover the hole/pit area.

#### LakeTrigger

1. Create an empty GameObject named `LakeTrigger`.
2. Add a **BoxCollider** → check **Is Trigger**.
3. Position and scale it to cover the water lake area.

### 5. Attach WinLoseTrigger Components

1. Select `HoleTrigger` → **Add Component → VRGame.World → Win Lose Trigger**.
   - **Type** = `Hole` | **Lose Reason** = `"You fell into a hole."`
2. Select `LakeTrigger` → **Add Component → VRGame.World → Win Lose Trigger**.
   - **Type** = `Lake`

> `WinLoseTrigger` finds the `GameManager` automatically via `FindFirstObjectByType`.

### 6. Create Route Waypoints

#### Wrong route (W1 → W4, last point inside HoleTrigger)

1. Create an empty GameObject named `RoutePointsWrong`.
2. Add **four** child GameObjects named `W1`, `W2`, `W3`, `W4`.
   - Place them to form a misleading path; put `W4` inside the `HoleTrigger` collider.

#### Correct route (C1 → C5, last point inside LakeTrigger)

1. Create an empty GameObject named `RoutePointsCorrect`.
2. Add **five** child GameObjects named `C1`, `C2`, `C3`, `C4`, `C5`.
   - Place them along the safe path; put `C5` inside the `LakeTrigger` collider.

### 7. Add the RouteRenderer *(optional — for visual route lines)*

1. Create an empty GameObject named `RouteRenderer`.
2. Add a **LineRenderer** component (set width and material as desired).
3. Add Component → **VRGame.World → Route Renderer**.
4. Assign:
   - **Wrong Route Parent** → `RoutePointsWrong`
   - **Correct Route Parent** → `RoutePointsCorrect`

### 8. Add the WaypointNavigator

1. Select (or create) the `GameManager` GameObject.
2. Add Component → **VRGame.World → Waypoint Navigator**.
3. Assign in the Inspector:
   - **Xr Origin Root** → the XR Origin Transform (same as GameManager's field).
   - **Wrong Route Parent** → `RoutePointsWrong`
   - **Correct Route Parent** → `RoutePointsCorrect`

### 9. Create the GameManager

1. Create an empty GameObject named `GameManager` (or use the existing one).
2. Add Component → **VRGame.Core → Game Manager**.
3. Assign all serialized fields:

   | Field | Value |
   |-------|-------|
   | **Xr Origin Root** | XR Origin Transform |
   | **Spawn Point** | `PlayerSpawn` |
   | **Route Renderer** *(optional)* | `RouteRenderer` GameObject |
   | **Waypoint Navigator** | the `WaypointNavigator` component (step 8) |
   | **Intro Panel** | `IntroPanel` GameObject |
   | **Mode Panel** | `ModePanel` GameObject |
   | **Win Panel** | `WinPanel` GameObject |
   | **Lose Panel** | `LosePanel` GameObject |
   | **Persistent Ask Ai Panel** | `PersistentAskAiPanel` GameObject |
   | **Verify Button Object** | the Verify button's *GameObject* inside PersistentAskAiPanel |
   | **Route Follow Panel** | `RouteFollowPanel` GameObject |
   | **Verify Sources Panel** | `VerifySourcesPanel` GameObject |
   | **Status Text** *(optional)* | a `TMP_Text` inside `LosePanel` |

### 10. Create the UI Canvas and Panels

> Recommended: use a **World Space** canvas in front of the player, or
> **Screen Space – Overlay** for quick prototyping.

#### IntroPanel

1. In your Canvas create a child Panel named `IntroPanel`.
2. Add a `TextMeshPro - Text (UI)` with your intro story.
3. Add a **Button** named `StartButton`.
4. Add Component → **VRGame.UI → Intro UI Controller**.
5. Assign **Game Manager** and **Start Button**.

#### ModePanel *(new)*

1. Create a child Panel named `ModePanel` (starts **inactive** — GameManager activates it).
2. Add a `TextMeshPro - Text (UI)` label, e.g. *"How do you want to proceed?"*.
3. Add a **Button** named `AskAiButton` (label: *"Ask AI for help"*).
4. Add a **Button** named `StartAloneButton` (label: *"Start alone"*).
5. Add Component → **VRGame.UI → Mode Panel Controller**.
6. Assign **Game Manager**, **Ask Ai Button**, **Start Alone Button**.

#### PersistentAskAiPanel *(new)*

1. Create a child Panel named `PersistentAskAiPanel` (starts **inactive**).
2. Add a **Button** named `AskAiButton` (label: *"Ask AI"*).
3. Add a child **Button** named `VerifyButton` (label: *"Verify route and sources"*) —
   start this GameObject **inactive** (GameManager re-enables it after first loss).
4. Add Component → **VRGame.UI → Persistent Ask Ai Panel Controller**.
5. Assign **Game Manager**, **Ask Ai Button**, **Verify Button**.
6. In `GameManager` Inspector drag `VerifyButton`'s *GameObject* to **Verify Button Object**.

#### RouteFollowPanel *(new)*

1. Create a child Panel named `RouteFollowPanel` (starts **inactive**).
2. Add a `TextMeshPro - Text (UI)` with the message:
   *"Follow this route to get to the lake."*
3. Add a **Button** named `NextWaypointButton` (label: *"Next waypoint"*).
4. Add an optional **Button** named `CloseButton` (label: *"Close"*).
5. Add Component → **VRGame.UI → Route Follow Panel Controller**.
6. Assign **Game Manager**, **Next Waypoint Button**, (optional) **Close Button**.

#### VerifySourcesPanel *(new)*

1. Create a child Panel named `VerifySourcesPanel` (starts **inactive**).
2. Add a `TextMeshPro - Text (UI)` with the message:
   *"This route was based on the forest map uploaded by a visitor in 1974."*
3. Add a **Button** named `UpdateSourceButton`
   (label: *"Update source and give me new route"*).
4. Add a **Button** named `ContinueAnywayButton` (label: *"Continue anyway"*).
5. Add Component → **VRGame.UI → Verify Sources Panel Controller**.
6. Assign **Game Manager**, **Update Source Button**, **Continue Anyway Button**.

#### WinPanel

1. Create a child Panel named `WinPanel` (starts **inactive**).
2. Add a label: *"You found the lake! You survived!"*
3. Add a **Button** named `TryAgainButton`.
4. Add Component → **VRGame.UI → Result UI Controller**.
5. Assign **Game Manager** and **Try Again Button**.

#### LosePanel

1. Create a child Panel named `LosePanel` (starts **inactive**).
2. Add a `TextMeshPro - Text (UI)` for the lose reason (drag to **Status Text** in GameManager).
3. Add a **Button** named `TryAgainButton`.
4. Add Component → **VRGame.UI → Result UI Controller**.
5. Assign **Game Manager** and **Try Again Button**.

---

## Panel Visibility Summary

| Panel | Visible when |
|-------|-------------|
| `IntroPanel` | State = **Intro** |
| `ModePanel` | State = **ModeSelect** |
| `PersistentAskAiPanel` | State = **Playing** |
| `VerifyButton` (inside Persistent) | State = Playing **and** player has lost at least once |
| `RouteFollowPanel` | Overlay — shown by `AskAI()`, hidden when last waypoint reached or closed |
| `VerifySourcesPanel` | Overlay — shown by `ShowVerifySourcesPanel()` |
| `WinPanel` | State = **Won** |
| `LosePanel` | State = **Lost** |

---

## Build Settings

The scene **VerifyBeforeYouRely_Level** is already included in
`ProjectSettings/EditorBuildSettings.asset`.

If it is ever missing, add it manually via:
**File → Build Settings → drag `Assets/Scenes/VerifyBeforeYouRely_Level.unity` into the list**.

---

## Package Dependencies

All required packages are already declared in `Packages/manifest.json`:

- **XR Interaction Toolkit** – locomotion, hands, interactors
- **TextMeshPro** – UI text
- **XR Hands** – hand tracking

No additional packages are needed.
