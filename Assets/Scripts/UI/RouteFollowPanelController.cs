using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VRGame.Core;

namespace VRGame.UI
{
    /// <summary>
    /// RouteFollowPanel flow:
    /// 1) Decision phase: show text + (Continue Anyway, Verify Sources)
    /// 2) Following phase: show text + (Teleport Next)
    ///
    /// Requirements from our conversation:
    /// - RouteFollowPanel "Continue Anyway" must show the WRONG route (W1..W4),
    ///   same as VerifySourcesPanel "Continue Anyway".
    /// - So this script delegates Continue Anyway to GameManager.ForceWrongRouteAndOpenFollow().
    /// - GameManager will call EnterFollowingPhase() so Teleport Next is visible.
    /// </summary>
    public class RouteFollowPanelController : MonoBehaviour
    {
        private enum Phase { Decision, Following }

        [SerializeField] private GameManager gameManager;

        [Header("UI")]
        [SerializeField] private TMP_Text messageText;

        [Header("Decision Buttons")]
        [SerializeField] private Button continueAnywayButton;
        [SerializeField] private Button verifySourcesButton;

        [Header("Follow Route")]
        [SerializeField] private Button nextWaypointButton;

        [Tooltip("Optional button to close the panel and resume free roaming.")]
        [SerializeField] private Button closeButton;

        private Phase _phase = Phase.Decision;

        private void Start()
        {
            if (gameManager == null) Debug.LogError("[RouteFollowPanelController] GameManager is not assigned.");
            if (nextWaypointButton == null) Debug.LogError("[RouteFollowPanelController] nextWaypointButton is not assigned.");
            if (continueAnywayButton == null) Debug.LogError("[RouteFollowPanelController] continueAnywayButton is not assigned.");
            if (verifySourcesButton == null) Debug.LogError("[RouteFollowPanelController] verifySourcesButton is not assigned.");
            if (messageText == null) Debug.LogWarning("[RouteFollowPanelController] messageText is not assigned (optional).");

            if (continueAnywayButton != null)
                continueAnywayButton.onClick.AddListener(OnContinueAnywayClicked);

            if (verifySourcesButton != null)
                verifySourcesButton.onClick.AddListener(OnVerifySourcesClicked);

            if (nextWaypointButton != null)
                nextWaypointButton.onClick.AddListener(OnNextWaypointClicked);

            if (closeButton != null)
                closeButton.onClick.AddListener(OnCloseClicked);
        }

        private void OnEnable()
        {
            // Whenever AskAI opens the panel, start in Decision phase.
            SetPhase(Phase.Decision);
        }

        /// <summary>
        /// Called by GameManager after it forces the wrong route (or whenever we want to
        /// jump directly into the teleport-following UI).
        /// </summary>
        public void EnterFollowingPhase()
        {
            SetPhase(Phase.Following);
            UpdateTeleportButtonState();
        }

        private void OnContinueAnywayClicked()
        {
            // Continue Anyway must start/show the WRONG route.
            // Delegate to GameManager so both Continue Anyway buttons behave the same.
            if (gameManager != null)
                gameManager.ForceWrongRouteAndOpenFollow();
        }

        private void OnVerifySourcesClicked()
        {
            // Open the verify sources panel.
            if (gameManager != null)
                gameManager.ShowVerifySourcesPanel();
        }

        private void OnNextWaypointClicked()
        {
            if (gameManager != null)
            {
                gameManager.TeleportNextWaypoint();
                UpdateTeleportButtonState();
            }
        }

        private void OnCloseClicked()
        {
            if (gameManager != null)
                gameManager.CloseRouteFollowPanel();
        }

        private void SetPhase(Phase phase)
        {
            _phase = phase;

            // Text
            if (messageText != null)
                messageText.text = "Follow this route to reach the lake.";

            // Decision buttons visible only in Decision phase
            if (continueAnywayButton != null) continueAnywayButton.gameObject.SetActive(_phase == Phase.Decision);
            if (verifySourcesButton != null) verifySourcesButton.gameObject.SetActive(_phase == Phase.Decision);

            // Teleport button visible only in Following phase
            if (nextWaypointButton != null) nextWaypointButton.gameObject.SetActive(_phase == Phase.Following);
        }

        private void UpdateTeleportButtonState()
        {
            if (_phase != Phase.Following) return;

            if (nextWaypointButton != null && gameManager != null)
                nextWaypointButton.interactable = gameManager.HasNextWaypoint;
        }
    }
}