using UnityEngine;
using UnityEngine.UI;
using VRGame.Core;

namespace VRGame.UI
{
    /// <summary>
    /// Drives the Route Follow panel that shows the AI message and lets the player
    /// teleport step-by-step through the current waypoint sequence.
    /// </summary>
    public class RouteFollowPanelController : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;

        [Tooltip("Button that teleports the player to the next waypoint.")]
        [SerializeField] private Button nextWaypointButton;

        [Tooltip("Optional button to close the panel and resume free roaming.")]
        [SerializeField] private Button closeButton;

        private void Start()
        {
            if (gameManager == null) Debug.LogError("[RouteFollowPanelController] GameManager is not assigned.");
            if (nextWaypointButton == null) Debug.LogError("[RouteFollowPanelController] nextWaypointButton is not assigned.");

            if (nextWaypointButton != null)
                nextWaypointButton.onClick.AddListener(OnNextWaypointClicked);

            if (closeButton != null)
                closeButton.onClick.AddListener(OnCloseClicked);
        }

        private void OnEnable()
        {
            // Refresh button state every time the panel becomes visible.
            UpdateButtonState();
        }

        private void OnNextWaypointClicked()
        {
            if (gameManager != null)
            {
                gameManager.TeleportNextWaypoint();
                UpdateButtonState();
            }
        }

        private void OnCloseClicked()
        {
            if (gameManager != null) gameManager.CloseRouteFollowPanel();
        }

        private void UpdateButtonState()
        {
            if (nextWaypointButton != null && gameManager != null)
                nextWaypointButton.interactable = gameManager.HasNextWaypoint;
        }
    }
}
