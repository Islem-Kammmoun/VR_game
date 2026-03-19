using UnityEngine;
using TMPro;
using System.Collections;
using VRGame.World;
using VRGame.UI;
using System;

namespace VRGame.Core
{
    /// <summary>
    /// Updated behavior:
    /// - Keep your whole current flow (AskAI / Verify / Update sequence / routes).
    /// - Replace the old LosePanel with ONLY the new one in the hole (LosePanel-1).
    /// - Do NOT freeze the player on Lose (player can fall and then see LosePanel-1).
    /// - Try Again on LosePanel-1 -> go back to Intro panel first (Option A).
    /// - WinPanel Play Again also goes back to Intro panel (Option A).
    /// - LosePanel-1 is ALWAYS SHOWN from the beginning (never disabled by GameManager).
    ///
    /// FIX:
    /// - Win panel not showing after 2nd time reaching C5:
    ///   We must "reset" the win state when restarting (and ensure winPanel is disabled in Intro/Playing),
    ///   plus make sure Win() always runs even if you were already in Won before.
    ///
    /// NEW:
    /// - OnRunRestarted event: fired whenever a run restarts (Play Again / Try Again).
    ///   WinLoseTrigger listens to this to reset its internal one-shot flag (_fired).
    ///
    /// NEW (Restart button support):
    /// - RestartToIntroFromAnywhere(): can be called at any time (even while falling),
    ///   and will restart back to Intro reliably.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        /// <summary>
        /// Raised whenever the current "run" restarts, so one-shot triggers (lake/hole) can reset.
        /// </summary>
        public static event Action OnRunRestarted;

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

        [Header("Lose UI (LosePanel-1 ALWAYS SHOWN)")]
        [Tooltip("Assign your new LosePanel-1 here (the one placed in the hole / lower ground).")]
        [SerializeField] private GameObject losePanelLower;
        [Tooltip("Optional: text inside LosePanel-1 for the reason message.")]
        [SerializeField] private TMP_Text loseReasonText;

        [Header("Playing UI")]
        [SerializeField] private GameObject persistentAskAiPanel;
        [SerializeField] private GameObject verifyButtonObject; // optional UX

        [Header("Overlay Panels")]
        [SerializeField] private GameObject routeFollowPanel;
        [SerializeField] private GameObject verifySourcesPanel;

        [Header("Verification Process Panel")]
        [Tooltip("Panel shown while 'Look for Updated Sources' sequence runs.")]
        [SerializeField] private GameObject verificationProcessPanel;
        [Tooltip("RectTransform of the VerificationProcessPanel root (for snapping).")]
        [SerializeField] private RectTransform verificationProcessPanelRect;
        [Tooltip("TMP text inside VerificationProcessPanel.")]
        [SerializeField] private TMP_Text verificationProcessText;
        [SerializeField] private float verificationPanelDistance = 1.5f;
        [SerializeField] private float verificationPanelHeightOffset = -0.1f;

        [Header("UI Snap (shared)")]
        [Tooltip("XR Origin Main Camera Transform. Used to snap panels into view.")]
        [SerializeField] private Transform xrCamera;

        [Header("UI Snap (RouteFollowPanel)")]
        [Tooltip("RectTransform of the RouteFollowPanel root to reposition/rotate.")]
        [SerializeField] private RectTransform routeFollowPanelRect;
        [SerializeField] private float routePanelDistance = 1.5f;
        [SerializeField] private float routePanelHeightOffset = -0.1f;

        [Header("UI Snap (LosePanel-1)")]
        [Tooltip("Optional: RectTransform of LosePanel-1 root if you want it snapped in front of player on Lose.")]
        [SerializeField] private RectTransform losePanelLowerRect;
        [SerializeField] private float losePanelDistance = 1.5f;
        [SerializeField] private float losePanelHeightOffset = -0.1f;

        [Header("Update sequence timing")]
        [Tooltip("Seconds each message stays on screen during 'Look for Updated Sources'.")]
        [SerializeField] private float messageDurationSeconds = 10f;

        private GameState _state = GameState.Intro;

        // Optional UX only (do NOT use this to gate whether verification works).
        private bool _hasLostOnce = false;

        // Wrong route by default, correct route only after verification/update completes.
        private bool _useCorrectRoute = false;

        private CharacterController _characterController;

        // Prevent stacking multiple update sequences if button clicked multiple times.
        private Coroutine _updateSourcesRoutine;

        private void Start()
        {
            _characterController = xrOriginRoot != null ? xrOriginRoot.GetComponentInChildren<CharacterController>() : null;

            ValidateReferences();

            // Ensure process panel starts hidden
            SetActive(verificationProcessPanel, false);

            // IMPORTANT: LosePanel-1 should ALWAYS be shown
            SetActive(losePanelLower, true);

            SetState(GameState.Intro);
        }

        private void ValidateReferences()
        {
            if (xrOriginRoot == null) Debug.LogError("[GameManager] xrOriginRoot is not assigned.");
            if (spawnPoint == null) Debug.LogError("[GameManager] spawnPoint is not assigned.");

            if (introPanel == null) Debug.LogError("[GameManager] introPanel is not assigned.");
            if (modePanel == null) Debug.LogError("[GameManager] modePanel is not assigned.");
            if (winPanel == null) Debug.LogError("[GameManager] winPanel is not assigned.");
            if (losePanelLower == null) Debug.LogError("[GameManager] losePanelLower (LosePanel-1) is not assigned.");

            if (persistentAskAiPanel == null) Debug.LogError("[GameManager] persistentAskAiPanel is not assigned.");
            if (routeFollowPanel == null) Debug.LogError("[GameManager] routeFollowPanel is not assigned.");
            if (verifySourcesPanel == null) Debug.LogError("[GameManager] verifySourcesPanel is not assigned.");

            if (verificationProcessPanel == null) Debug.LogWarning("[GameManager] verificationProcessPanel is not assigned. Update sequence messages won't show.");
            if (verificationProcessText == null) Debug.LogWarning("[GameManager] verificationProcessText is not assigned.");
            if (verificationProcessPanelRect == null) Debug.LogWarning("[GameManager] verificationProcessPanelRect is not assigned.");

            if (waypointNavigator == null) Debug.LogWarning("[GameManager] waypointNavigator is not assigned; teleportation will not work.");

            if (xrCamera == null) Debug.LogWarning("[GameManager] xrCamera is not assigned (optional). Panel snapping will not work.");
            if (routeFollowPanelRect == null) Debug.LogWarning("[GameManager] routeFollowPanelRect is not assigned (optional). RouteFollowPanel snapping will not work.");
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
        /// Global restart button: always restarts to Intro, regardless of current state.
        /// Use this for a "Restart" button that must work even if the player is mid-fall,
        /// or if Lose() hasn't been triggered yet.
        /// </summary>
        public void RestartToIntroFromAnywhere()
        {
            RestartRunToIntro();
        }

        /// <summary>
        /// WinPanel "Play Again" button: restart run and show Intro panel (Option A).
        /// </summary>
        public void PlayAgainFromWin()
        {
            RestartRunToIntro();
        }

        /// <summary>
        /// LosePanel-1 "Try Again" button: restart run and show Intro panel (Option A).
        /// </summary>
        public void TryAgainFromLose()
        {
            RestartRunToIntro();
        }

        private void RestartRunToIntro()
        {
            // NEW: reset one-shot triggers (lake/hole) and any other listeners
            OnRunRestarted?.Invoke();

            // Stop update sequence if running
            if (_updateSourcesRoutine != null)
            {
                StopCoroutine(_updateSourcesRoutine);
                _updateSourcesRoutine = null;
            }

            // Reset run (wrong route again until they verify)
            _useCorrectRoute = false;

            // Hide overlays / process (DO NOT hide LosePanel-1)
            SetActive(routeFollowPanel, false);
            SetActive(verifySourcesPanel, false);
            SetActive(verificationProcessPanel, false);
            SetVerificationProcessText(string.Empty);

            // IMPORTANT: make sure win panel is OFF immediately (so it can show again next win)
            SetActive(winPanel, false);

            // Hide route visual
            HideRoute();

            // Respawn at spawn
            TeleportPlayerToSpawn();

            // Ensure movement enabled
            SetPlayerMovementEnabled(true);

            // Keep LosePanel-1 always shown
            SetActive(losePanelLower, true);

            // Intro panel again
            SetState(GameState.Intro);
        }

        /// <summary>
        /// AskAI opens RouteFollowPanel in Decision phase (Continue Anyway + Verify Sources).
        /// </summary>
        public void AskAI()
        {
            if (_state != GameState.Playing)
                SetState(GameState.Playing);

            // Close verify-related panels when asking AI
            SetActive(verifySourcesPanel, false);
            SetActive(verificationProcessPanel, false);

            // Prepare navigator route
            if (waypointNavigator != null)
                waypointNavigator.SetRoute(_useCorrectRoute);

            // Show correct/wrong visualization
            if (_useCorrectRoute) ShowCorrectRoute();
            else ShowWrongRoute();

            // Show route panel (Decision phase handled in RouteFollowPanelController.OnEnable())
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
            SetActive(routeFollowPanel, false);
            SetActive(verificationProcessPanel, false);
            SetActive(verifySourcesPanel, true);
        }

        /// <summary>
        /// "Look for Updated Sources" button:
        /// show VerificationProcessPanel messages one-by-one (10s each),
        /// then unlock correct route and open RouteFollowPanel directly in FOLLOWING phase.
        /// </summary>
        public void UpdateSourceAndNewRoute()
        {
            SetActive(verifySourcesPanel, false);
            SetActive(routeFollowPanel, false);

            if (_updateSourcesRoutine != null)
                StopCoroutine(_updateSourcesRoutine);

            _updateSourcesRoutine = StartCoroutine(UpdateSourcesSequence());
        }

        private IEnumerator UpdateSourcesSequence()
        {
            SetActive(verificationProcessPanel, true);
            SnapVerificationProcessPanelToView();

            SetVerificationProcessText("Looking for newer information in the forest signs…");
            yield return new WaitForSeconds(messageDurationSeconds);

            SnapVerificationProcessPanelToView();
            SetVerificationProcessText("Attention: holes next to rocks.");
            yield return new WaitForSeconds(messageDurationSeconds);

            SnapVerificationProcessPanelToView();
            SetVerificationProcessText("Updating sources…");
            yield return new WaitForSeconds(messageDurationSeconds);

            SetActive(verificationProcessPanel, false);

            // Unlock correct route (NO LOSS REQUIRED)
            _useCorrectRoute = true;

            // Open correct route panel
            AskAI();

            // Force RouteFollowPanel into Following phase (Teleport Next only)
            if (routeFollowPanel != null)
            {
                RouteFollowPanelController ctrl = routeFollowPanel.GetComponent<RouteFollowPanelController>();
                if (ctrl != null)
                    ctrl.EnterFollowingPhase();
            }

            _updateSourcesRoutine = null;
        }

        /// <summary>
        /// Continue Anyway => wrong route + Following phase.
        /// </summary>
        public void ForceWrongRouteAndOpenFollow()
        {
            if (_state != GameState.Playing)
                SetState(GameState.Playing);

            _useCorrectRoute = false;

            SetActive(verifySourcesPanel, false);
            SetActive(verificationProcessPanel, false);

            if (waypointNavigator != null)
                waypointNavigator.SetRoute(false);

            ShowWrongRoute();

            SetActive(routeFollowPanel, true);
            SnapRouteFollowPanelToView();

            if (routeFollowPanel != null)
            {
                RouteFollowPanelController ctrl = routeFollowPanel.GetComponent<RouteFollowPanelController>();
                if (ctrl != null)
                    ctrl.EnterFollowingPhase();
            }
        }

        public void ContinueAnyway()
        {
            ForceWrongRouteAndOpenFollow();
        }

        public void Win()
        {
            SetActive(routeFollowPanel, false);
            SetActive(verifySourcesPanel, false);
            SetActive(verificationProcessPanel, false);

            // Ensure win panel toggles off->on if needed
            if (winPanel != null && winPanel.activeSelf)
                winPanel.SetActive(false);

            SetState(GameState.Won);
        }

        /// <summary>
        /// Lose uses LosePanel-1 and does NOT freeze the player.
        /// </summary>
        public void Lose(string reason)
        {
            if (_state != GameState.Playing) return;

            _hasLostOnce = true;

            SetActive(routeFollowPanel, false);
            SetActive(verifySourcesPanel, false);
            SetActive(verificationProcessPanel, false);

            if (loseReasonText != null)
                loseReasonText.text = reason;

            // Keep LosePanel-1 always shown
            SetActive(losePanelLower, true);

            // Optional: snap LosePanel-1 in front of player camera.
            SnapLosePanelLowerToView();

            SetState(GameState.Lost);
        }

        public void Respawn()
        {
            // Not used by Try Again (Try Again goes to Intro), but kept for compatibility.
            if (xrOriginRoot == null || spawnPoint == null) return;

            SetPlayerMovementEnabled(true);
            TeleportPlayerToSpawn();
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

        public void ShowIntro()
        {
            ResetToIntroRun();
            SetState(GameState.Intro);
        }

        // ── Private helpers ─────────────────────────────────────────────────────

        private void ResetToIntroRun()
        {
            // Back to wrong route until they verify again
            _useCorrectRoute = false;

            // Hide overlays/process only (do NOT hide LosePanel-1)
            SetActive(routeFollowPanel, false);
            SetActive(verifySourcesPanel, false);
            SetActive(verificationProcessPanel, false);
            HideRoute();

            SetVerificationProcessText(string.Empty);

            TeleportPlayerToSpawn();

            // Keep LosePanel-1 always shown
            SetActive(losePanelLower, true);

            // Ensure win panel is off in intro
            SetActive(winPanel, false);
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

        private void SetVerificationProcessText(string message)
        {
            if (verificationProcessText != null)
                verificationProcessText.text = message;
        }

        // ── Snapping helpers ────────────────────────────────────────────────────

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

        private void SnapVerificationProcessPanelToView()
        {
            if (verificationProcessPanel == null || !verificationProcessPanel.activeInHierarchy) return;
            SnapPanelToView(verificationProcessPanelRect, verificationPanelDistance, verificationPanelHeightOffset);
        }

        private void SnapLosePanelLowerToView()
        {
            if (losePanelLowerRect == null || xrCamera == null) return;
            SnapPanelToView(losePanelLowerRect, losePanelDistance, losePanelHeightOffset);
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

            SetActive(persistentAskAiPanel, _state == GameState.Playing);

            if (verifyButtonObject != null)
                verifyButtonObject.SetActive(_hasLostOnce);

            if (_state != GameState.Playing)
            {
                SetActive(routeFollowPanel, false);
                SetActive(verifySourcesPanel, false);
                SetActive(verificationProcessPanel, false);
                HideRoute();
            }

            // Ensure LosePanel-1 is ALWAYS shown
            if (losePanelLower != null && !losePanelLower.activeSelf)
                losePanelLower.SetActive(true);
        }

        private static void SetActive(GameObject go, bool active)
        {
            if (go != null) go.SetActive(active);
        }
    }
}