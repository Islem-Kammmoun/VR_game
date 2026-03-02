# VR Game – Verify Before You Rely

A VR navigation prototype built with **Unity 6000.2.10f1** and the **XR Interaction Toolkit**.
The player wakes up in a forest and must find a water lake to survive. An in-game AI assistant
can give directions, but it initially provides the **wrong route** because it uses an old map.
The player must question the AI's sources, ask it to update its data, and then ask for the route
again to receive the correct path to the lake.

---

## Story Mechanic

| Step | Player action | AI behaviour |
|------|---------------|--------------|
| 1 | Asks for the road/route | AI shows wrong route → leads to a pit (instant loss) |
| 2 | Asks AI to verify / show sources | AI admits it used an old map and suggests updating |
| 3 | Asks AI to update / new map | AI confirms sources updated |
| 4 | Asks for the road again | AI shows correct route → leads to the lake (win) |

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

1. Select the **XR Origin** (or **XR Origin (XR Rig)**) GameObject in the Hierarchy.
2. In the Inspector, set **Tag** to `Player`.

### 3. Create the PlayerSpawn Transform

1. Create an empty GameObject named `PlayerSpawn`.
2. Place it at the centre of the starting area (the origin, or wherever the player should spawn).
3. Rotate it so its forward direction (blue arrow) points the way the player should face on spawn.

### 4. Create Trigger Zones

#### HoleTrigger

1. Create an empty GameObject named `HoleTrigger`.
2. Add a **BoxCollider** component.
3. Check **Is Trigger**.
4. Position and scale it to cover the hole/pit area.

#### LakeTrigger

1. Create an empty GameObject named `LakeTrigger`.
2. Add a **BoxCollider** component.
3. Check **Is Trigger**.
4. Position and scale it to cover the water lake area.

### 5. Attach WinLoseTrigger Components

1. Select `HoleTrigger` → **Add Component → VRGame.World → Win Lose Trigger**.
   - Set **Type** = `Hole`.
   - Set **Lose Reason** = `"You fell into a hole."` (or any message).
2. Select `LakeTrigger` → **Add Component → VRGame.World → Win Lose Trigger**.
   - Set **Type** = `Lake`.

> **Note:** `WinLoseTrigger` finds the `GameManager` automatically at runtime via
> `FindAnyObjectByType`. No explicit drag-and-drop is needed for that reference.

### 6. Create Route Waypoints

#### Wrong route (leads toward the hole)

1. Create an empty GameObject named `RoutePointsWrong`.
2. Add child GameObjects named `WP_0`, `WP_1`, `WP_2` … placed along the misleading path.
   At least **2 waypoints** are required.

#### Correct route (leads toward the lake)

1. Create an empty GameObject named `RoutePointsCorrect`.
2. Add child GameObjects named `WP_0`, `WP_1`, `WP_2` … placed along the correct path.
   At least **2 waypoints** are required.

### 7. Add the RouteRenderer

1. Create an empty GameObject named `RouteRenderer`.
2. Add a **LineRenderer** component to it (adjust width and material as desired).
3. Add Component → **VRGame.World → Route Renderer**.
4. Assign:
   - **Line** → the `LineRenderer` on the same GameObject.
   - **Wrong Route Parent** → `RoutePointsWrong`
   - **Correct Route Parent** → `RoutePointsCorrect`

### 8. Create the GameManager

1. Create an empty GameObject named `GameManager`.
2. Add Component → **VRGame.Core → Game Manager**.
3. Assign all serialized fields in the Inspector:
   - **Xr Origin Root** → the XR Origin GameObject's Transform.
   - **Spawn Point** → `PlayerSpawn`.
   - **Route Renderer** → the `RouteRenderer` GameObject.
   - **Intro Panel** → (see step 9)
   - **Chat Panel** → (see step 9)
   - **Win Panel** → (see step 9)
   - **Lose Panel** → (see step 9)
   - **Status Text** *(optional)* → a TMP_Text to display the lose reason.

### 9. Create the UI Canvases / Panels

> Recommended: use **World Space** canvases positioned in front of the player,
> or use **Screen Space – Overlay** for quick prototyping.

#### Intro Panel

1. Create a Canvas → add a child Panel named `IntroPanel`.
2. Add a `TextMeshPro - Text (UI)` with the story description.
3. Add a **Button** named `StartButton`.
4. Add Component → **VRGame.UI → Intro UI Controller** on the Panel.
5. Assign **Game Manager** and **Start Button** in the Inspector.

#### Chat Panel

1. Create a child Panel named `ChatPanel`.
2. Inside add:
   - A `TMP_InputField` (name: `InputField`)
   - A `TextMeshPro - Text (UI)` for the conversation log (name: `ConversationText`)
   - A `Button` named `SendButton`
3. Create an empty GameObject named `AIAssistant`.
4. Add Component → **VRGame.AI → AI Assistant Controller**.
5. Assign **Game Manager** in the Inspector.
6. Add Component → **VRGame.UI → Chat UI Controller** on `ChatPanel`.
7. Assign **Input Field**, **Conversation Text**, **Send Button**, and **Assistant** references.

#### Win Panel

1. Create a child Panel named `WinPanel`.
2. Add a label text: "You found the lake! You survived!"
3. Add a **Button** named `TryAgainButton`.
4. Add Component → **VRGame.UI → Result UI Controller**.
5. Assign **Game Manager** and **Try Again Button**.

#### Lose Panel

1. Create a child Panel named `LosePanel`.
2. Add a `TextMeshPro - Text (UI)` as the **Status Text** (drag to GameManager's Status Text field).
3. Add a **Button** named `TryAgainButton`.
4. Add Component → **VRGame.UI → Result UI Controller**.
5. Assign **Game Manager** and **Try Again Button**.

### 10. Assign Panel References in GameManager

Return to the `GameManager` Inspector and drag:
- `IntroPanel` → **Intro Panel**
- `ChatPanel` → **Chat Panel**
- `WinPanel` → **Win Panel**
- `LosePanel` → **Lose Panel**

---

## Keyword Examples

Type these messages into the chat panel during play mode:

| Message | Effect |
|---------|--------|
| `show me the road` | Shows wrong route (old map) |
| `what sources are you using?` | AI admits it uses an old map |
| `update your map` | AI loads updated map |
| `show me the road again` | Shows correct route (updated map) |

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
