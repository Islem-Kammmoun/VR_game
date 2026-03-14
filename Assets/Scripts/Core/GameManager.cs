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

        [Header("UI Snap (shared)")]
        [Tooltip("XR Origin Main Camera Transform. Used to snap panels into view.")]
        [SerializeField] private Transform xrCamera;

        [Header("UI Snap (RouteFollowPanel)")]
        [Tooltip("RectTransform of the RouteFollowPanel to reposition/rotate.")]
        [SerializeField] private RectTransform routeFollowPanelRect;
        [Tooltip("How far in front of the camera the RouteFollowPanel should snap (in meters).")]
        [SerializeField] private float routePanelDistance = 1.5f;
        [Tooltip("Vertical offset applied when snapping the RouteFollowPanel.")]
        [SerializeField] private float routePanelHeightOffset = -0.1f;

        [Header("UI Snap (LosePanel)")]
        [Tooltip("RectTransform of the LosePanel to reposition/rotate when losing.")]
        [SerializeField] private RectTransform losePanelRect;
        [Tooltip("How far in front of the camera the LosePanel should snap (in meters).")]
        [SerializeField] private float losePanelDistance = 1.5f;
        [Tooltip("Vertical offset applied when snapping the LosePanel.")]
        [SerializeField] private float losePanelHeightOffset = -0.1f;

        [Header("Status (optional)")]
        [SerializeField] private TMP_Text statusText;

        private GameState _state = GameState.Intro;

        // True after first loss; we keep it true even after Try Again.
        private bool _hasLostOnce = false;

        // When true, AskAI uses the correct route (C1..C5).
        private bool _useCorrectRoute = false;

        // Cache this so we can freeze/unfreeze player when there is no ground (hole).
        private CharacterController _characterController;

        private void Start()
        {
            _characterController = xrOriginRoot != null ? xrOriginRoot.GetComponentInChildren<CharacterController>() : null;

            ValidateReferences();

            // Keep the scene's initial player position on Play.
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

            if (xrCamera == null) Debug.LogWarning("[GameManager] xrCamera is not assigned (optional). Panel snapping will not work.");
            if (routeFollowPanelRect == null) Debug.LogWarning("[GameManager] routeFollowPanelRect is not assigned (optional). RouteFollowPanel snapping will not work.");
            if (losePanelRect == null) Debug.LogWarning("[GameManager] losePanelRect is not assigned (optional). LosePanel snapping will not work.");
        }

        // ── Public API ──────────────────────────────────────────────────────────

        public void ShowModePanel()
        {
            SetState(GameState.ModeSelect);
        }

        public void StartAlone()
        {
            SetState(GameState.Playing);
        }

        /// <summary>
        /// Ask AI flow:
        /// - Before first loss: show wrong route directly.
        /// - After first loss (and before updating sources): force VerifySourcesPanel first.
        /// - After updating sources: show correct route.
        /// </summary>
        public void AskAI()
        {
            if (_state != GameState.Playing)
                SetState(GameState.Playing);

            // NEW: After first loss, require verification before showing any route
            // unless the player already updated sources (useCorrectRoute == true).
            if (_hasLostOnce && !_useCorrectRoute)
            {
                SetActive(routeFollowPanel, false);
                SetActive(verifySourcesPanel, true);
                return;
            }

            if (waypointNavigator != null)
                waypointNavigator.SetRoute(_useCorrectRoute);

            if (_useCorrectRoute) ShowCorrectRoute();
            else ShowWrongRoute();

            SetActive(routeFollowPanel, true);
            SnapRouteFollowPanelToView();
        }

        public void CloseRouteFollowPanel()
        {
            SetActive(routeFollowPanel, false);
        }

        public void TeleportNextWaypoint()
        {
            if (waypointNavigator == null) return;

            waypointNavigator.TeleportNext();
            SnapRouteFollowPanelToView();

            if (!waypointNavigator.HasNextWaypoint)
                CloseRouteFollowPanel();
        }

        public bool HasNextWaypoint => waypointNavigator != null && waypointNavigator.HasNextWaypoint;

        public void ShowVerifySourcesPanel()
        {
            SetActive(verifySourcesPanel, true);
        }

        public void UpdateSourceAndNewRoute()
        {
            _useCorrectRoute = true;
            SetActive(verifySourcesPanel, false);

            // Now AskAI() will pass the gating and show the correct route.
            AskAI();
        }

        public void ContinueAnyway()
        {
            SetActive(verifySourcesPanel, false);

            // Continue anyway keeps wrong route:
            // Since _useCorrectRoute is still false, AskAI() would re-open VerifySourcesPanel.
            // So instead, show wrong route directly.
            ForceShowWrongRoute();
        }

        public void Win()
        {
            if (_state != GameState.Playing) return;
            SetActive(routeFollowPanel, false);
            SetActive(verifySourcesPanel, false);
            SetState(GameState.Won);
        }

        public void Lose(string reason)
        {
            if (_state != GameState.Playing) return;

            _hasLostOnce = true;

            SetActive(routeFollowPanel, false);
            SetActive(verifySourcesPanel, false);

            if (statusText != null) statusText.text = reason;

            // Freeze player so they stop falling in the hole and can interact with UI.
            SetPlayerMovementEnabled(false);

            SetState(GameState.Lost);
            SnapLosePanelToView();
        }

        public void Respawn()
        {
            if (xrOriginRoot == null || spawnPoint == null) return;

            SetPlayerMovementEnabled(true);

            CharacterController cc = _characterController != null
                ? _characterController
                : xrOriginRoot.GetComponentInChildren<CharacterController>();

            if (cc != null) cc.enabled = false;

            xrOriginRoot.position = spawnPoint.position;
            Vector3 euler = xrOriginRoot.eulerAngles;
            xrOriginRoot.rotation = Quaternion.Euler(euler.x, spawnPoint.eulerAngles.y, euler.z);

            if (cc != null) cc.enabled = true;

            SetState(GameState.Playing);
        }

        public void ShowWrongRoute()
        {
            if (routeRenderer != null) routeRenderer.ShowWrongRoute();
        }

        public void ShowCorrectRoute()
        {
            if (routeRenderer != null) routeRenderer.ShowCorrectRoute();
        }

        public void HideRoute()
        {
            if (routeRenderer != null) routeRenderer.Hide();
        }

        /// <summary>
        /// Restart to Intro + teleport to spawn.
        /// IMPORTANT: _hasLostOnce is NOT reset, so after first loss verification stays required on AskAI.
        /// </summary>
        public void ShowIntro()
        {
            ResetToIntroRun();
            SetState(GameState.Intro);
        }

        // ── Private helpers ─────────────────────────────────────────────────────

        private void ResetToIntroRun()
        {
            // Start new run with wrong route again until they update sources.
            _useCorrectRoute = false;

            // IMPORTANT: keep _hasLostOnce as-is so Verify remains unlocked after first loss.
            // _hasLostOnce = false;

            SetActive(routeFollowPanel, false);
            SetActive(verifySourcesPanel, false);
            HideRoute();

            if (statusText != null) statusText.text = string.Empty;

            TeleportPlayerToSpawn();
        }

        private void ForceShowWrongRoute()
        {
            if (_state != GameState.Playing)
                SetState(GameState.Playing);

            if (waypointNavigator != null)
                waypointNavigator.SetRoute(false);

            ShowWrongRoute();

            SetActive(routeFollowPanel, true);
            SnapRouteFollowPanelToView();
        }

        private void TeleportPlayerToSpawn()
        {
            if (xrOriginRoot == null || spawnPoint == null) return;

            SetPlayerMovementEnabled(true);

            CharacterController cc = _characterController != null
                ? _characterController
                : xrOriginRoot.GetComponentInChildren<CharacterController>();

            if (cc != null) cc.enabled = false;

            xrOriginRoot.position = spawnPoint.position;
            Vector3 euler = xrOriginRoot.eulerAngles;
            xrOriginRoot.rotation = Quaternion.Euler(euler.x, spawnPoint.eulerAngles.y, euler.z);

            if (cc != null) cc.enabled = true;
        }

        private void SetPlayerMovementEnabled(bool enabled)
        {
            if (_characterController != null)
                _characterController.enabled = enabled;
        }

        private void SnapPanelToView(RectTransform panelRect, float distance, float heightOffset)
        {
            if (xrCamera == null || panelRect == null) return;

            Vector3 pos = xrCamera.position + xrCamera.forward * distance + Vector3.up * heightOffset;
            panelRect.position = pos;

            Vector3 lookDir = panelRect.position - xrCamera.position;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.0001f)
                panelRect.rotation = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
        }

        private void SnapRouteFollowPanelToView()
        {
            if (routeFollowPanel == null || !routeFollowPanel.activeInHierarchy) return;
            SnapPanelToView(routeFollowPanelRect, routePanelDistance, routePanelHeightOffset);
        }

        private void SnapLosePanelToView()
        {
            if (losePanel == null || !losePanel.activeInHierarchy) return;
            SnapPanelToView(losePanelRect, losePanelDistance, losePanelHeightOffset);
        }

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

            // Verify becomes available after first loss, and remains available after Try Again.
            if (verifyButtonObject != null)
                verifyButtonObject.SetActive(_hasLostOnce);

            // Hide overlays whenever we leave Playing state.
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