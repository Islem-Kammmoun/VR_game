using UnityEngine;
using TMPro;
using VRGame.World;

namespace VRGame.Core
{
    /// <summary>
    /// Manages game state transitions and coordinates all UI panels, route display,
    /// waypoint teleportation, and player respawning for the Verify-Before-You-Rely flow.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public enum GameState { Intro, ModeSelect, Playing, Won, Lost }

        [Header("Player")]
        [SerializeField] private Transform xrOriginRoot;
        [SerializeField] private Transform spawnPoint;

        [Header("Route (optional)")]
        [SerializeField] private RouteRenderer routeRenderer;

        [Header("Waypoint Navigator")]
        [SerializeField] private WaypointNavigator waypointNavigator;

        [Header("Main UI Panels")]
        [SerializeField] private GameObject introPanel;
        [SerializeField] private GameObject modePanel;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private GameObject losePanel;

        [Header("Playing UI")]
        [SerializeField] private GameObject persistentAskAiPanel;
        [SerializeField] private GameObject verifyButtonObject; // child of persistentAskAiPanel; shown only after first loss

        [Header("Overlay Panels")]
        [SerializeField] private GameObject routeFollowPanel;
        [SerializeField] private GameObject verifySourcesPanel;

        [Header("Status (optional)")]
        [SerializeField] private TMP_Text statusText;

        private GameState _state = GameState.Intro;
        private bool _hasLostOnce = false;
        private bool _useCorrectRoute = false;

        private void Start()
        {
            ValidateReferences();
            SetState(GameState.Intro);
        }

        private void ValidateReferences()
        {
            if (xrOriginRoot == null) Debug.LogError("[GameManager] xrOriginRoot is not assigned.");
            if (spawnPoint == null) Debug.LogError("[GameManager] spawnPoint is not assigned.");
            if (introPanel == null) Debug.LogError("[GameManager] introPanel is not assigned.");
            if (modePanel == null) Debug.LogError("[GameManager] modePanel is not assigned.");
            if (winPanel == null) Debug.LogError("[GameManager] winPanel is not assigned.");
            if (losePanel == null) Debug.LogError("[GameManager] losePanel is not assigned.");
            if (persistentAskAiPanel == null) Debug.LogError("[GameManager] persistentAskAiPanel is not assigned.");
            if (routeFollowPanel == null) Debug.LogError("[GameManager] routeFollowPanel is not assigned.");
            if (verifySourcesPanel == null) Debug.LogError("[GameManager] verifySourcesPanel is not assigned.");
            if (waypointNavigator == null) Debug.LogWarning("[GameManager] waypointNavigator is not assigned; teleportation will not work.");
        }

        // ── Public API ──────────────────────────────────────────────────────────

        /// <summary>Called by the Start button on the Intro panel. Shows the mode-selection panel.</summary>
        public void ShowModePanel()
        {
            SetState(GameState.ModeSelect);
        }

        /// <summary>Called by "Start Alone" on the Mode panel. Enters Playing state without showing a route.</summary>
        public void StartAlone()
        {
            SetState(GameState.Playing);
        }

        /// <summary>
        /// Called by any "Ask AI" button (Mode panel or persistent panel).
        /// Enters Playing state if not already, initialises the waypoint route,
        /// and shows the RouteFollow overlay panel.
        /// </summary>
        public void AskAI()
        {
            if (_state != GameState.Playing)
                SetState(GameState.Playing);

            if (waypointNavigator != null)
                waypointNavigator.SetRoute(_useCorrectRoute);

            // Optionally render the route line.
            if (_useCorrectRoute) ShowCorrectRoute();
            else ShowWrongRoute();

            SetActive(routeFollowPanel, true);
        }

        /// <summary>Closes the RouteFollow overlay without changing game state.</summary>
        public void CloseRouteFollowPanel()
        {
            SetActive(routeFollowPanel, false);
        }

        /// <summary>
        /// Advances the player to the next waypoint in the current route sequence.
        /// Once all waypoints have been visited the RouteFollow panel is automatically
        /// closed (the final teleport will typically fire a Win/Lose trigger anyway).
        /// </summary>
        public void TeleportNextWaypoint()
        {
            if (waypointNavigator == null) return;
            waypointNavigator.TeleportNext();
            // Auto-close the RouteFollow panel when the last waypoint is consumed;
            // the final waypoint sits inside a Win/Lose trigger that handles state.
            if (!waypointNavigator.HasNextWaypoint)
                CloseRouteFollowPanel();
        }

        /// <summary>Whether there is still an unvisited waypoint in the current route.</summary>
        public bool HasNextWaypoint => waypointNavigator != null && waypointNavigator.HasNextWaypoint;

        /// <summary>Shows the Verify Sources overlay panel.</summary>
        public void ShowVerifySourcesPanel()
        {
            SetActive(verifySourcesPanel, true);
        }

        /// <summary>
        /// "Update source and give me new route" button handler.
        /// Switches to the correct route and opens the RouteFollow panel.
        /// </summary>
        public void UpdateSourceAndNewRoute()
        {
            _useCorrectRoute = true;
            SetActive(verifySourcesPanel, false);
            AskAI();
        }

        /// <summary>
        /// "Continue anyway" button handler.
        /// Keeps the wrong route and re-opens the RouteFollow panel.
        /// </summary>
        public void ContinueAnyway()
        {
            SetActive(verifySourcesPanel, false);
            AskAI();
        }

        /// <summary>Triggers the win state and shows the win panel.</summary>
        public void Win()
        {
            if (_state != GameState.Playing) return;
            SetActive(routeFollowPanel, false);
            SetActive(verifySourcesPanel, false);
            SetState(GameState.Won);
        }

        /// <summary>Triggers the lose state with a reason string shown on the lose panel.</summary>
        public void Lose(string reason)
        {
            if (_state != GameState.Playing) return;
            _hasLostOnce = true;
            SetActive(routeFollowPanel, false);
            SetActive(verifySourcesPanel, false);
            if (statusText != null) statusText.text = reason;
            SetState(GameState.Lost);
        }

        /// <summary>Teleports the player to the spawn point and resumes Playing state.</summary>
        public void Respawn()
        {
            if (xrOriginRoot == null || spawnPoint == null) return;

            // Temporarily disable any CharacterController to avoid collision glitches.
            CharacterController cc = xrOriginRoot.GetComponentInChildren<CharacterController>();
            if (cc != null) cc.enabled = false;

            xrOriginRoot.position = spawnPoint.position;
            Vector3 euler = xrOriginRoot.eulerAngles;
            xrOriginRoot.rotation = Quaternion.Euler(euler.x, spawnPoint.eulerAngles.y, euler.z);

            if (cc != null) cc.enabled = true;

            SetState(GameState.Playing);
        }

        /// <summary>Shows the wrong (old-map) route via the RouteRenderer.</summary>
        public void ShowWrongRoute()
        {
            if (routeRenderer != null) routeRenderer.ShowWrongRoute();
        }

        /// <summary>Shows the correct route via the RouteRenderer.</summary>
        public void ShowCorrectRoute()
        {
            if (routeRenderer != null) routeRenderer.ShowCorrectRoute();
        }

        /// <summary>Hides the route visualization.</summary>
        public void HideRoute()
        {
            if (routeRenderer != null) routeRenderer.Hide();
        }

        /// <summary>Returns to the Intro state.</summary>
        public void ShowIntro()
        {
            SetState(GameState.Intro);
        }

        // ── Private helpers ─────────────────────────────────────────────────────

        private void SetState(GameState newState)
        {
            _state = newState;
            UpdatePanels();
        }

        private void UpdatePanels()
        {
            SetActive(introPanel, _state == GameState.Intro);
            SetActive(modePanel, _state == GameState.ModeSelect);
            SetActive(winPanel, _state == GameState.Won);
            SetActive(losePanel, _state == GameState.Lost);
            SetActive(persistentAskAiPanel, _state == GameState.Playing);

            // The Verify button inside the persistent panel is only shown after the first loss.
            if (verifyButtonObject != null)
                verifyButtonObject.SetActive(_hasLostOnce);

            // Hide overlay panels whenever we leave Playing state.
            if (_state != GameState.Playing)
            {
                SetActive(routeFollowPanel, false);
                SetActive(verifySourcesPanel, false);
                HideRoute();
            }
        }

        private static void SetActive(GameObject go, bool active)
        {
            if (go != null) go.SetActive(active);
        }
    }
}
