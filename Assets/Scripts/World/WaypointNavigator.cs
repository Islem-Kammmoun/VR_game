using UnityEngine;

namespace VRGame.World
{
    /// <summary>
    /// Holds a wrong and a correct sequence of waypoint Transforms and teleports
    /// the player through them one step at a time.
    /// Attach this component to the same GameObject as GameManager (or any scene object)
    /// and assign the waypoint parent Transforms in the Inspector.
    /// </summary>
    public class WaypointNavigator : MonoBehaviour
    {
        [Header("Player")]
        [SerializeField] private Transform xrOriginRoot;

        [Header("Waypoint Parents")]
        [Tooltip("Parent of W1..W4 – wrong route ending in the hole.")]
        [SerializeField] private Transform wrongRouteParent;

        [Tooltip("Parent of C1..C5 – correct route ending at the lake.")]
        [SerializeField] private Transform correctRouteParent;

        private Transform[] _waypoints = new Transform[0];
        private int _currentIndex;
        private CharacterController _characterController;

        /// <summary>True when there is at least one unvisited waypoint remaining.</summary>
        public bool HasNextWaypoint => _currentIndex < _waypoints.Length;

        private void Start()
        {
            if (xrOriginRoot == null)
                Debug.LogError("[WaypointNavigator] xrOriginRoot is not assigned.");
            if (wrongRouteParent == null)
                Debug.LogWarning("[WaypointNavigator] wrongRouteParent is not assigned.");
            if (correctRouteParent == null)
                Debug.LogWarning("[WaypointNavigator] correctRouteParent is not assigned.");

            // Cache the CharacterController so we don't pay GetComponentInChildren on every teleport.
            if (xrOriginRoot != null)
                _characterController = xrOriginRoot.GetComponentInChildren<CharacterController>();
        }

        // ── Public API ──────────────────────────────────────────────────────────

        /// <summary>
        /// Selects the wrong or correct waypoint sequence and resets the index to the start.
        /// Must be called each time the player requests a new route.
        /// </summary>
        /// <param name="useCorrect">Pass <c>true</c> to use C1..C5, <c>false</c> for W1..W4.</param>
        public void SetRoute(bool useCorrect)
        {
            Transform parent = useCorrect ? correctRouteParent : wrongRouteParent;

            if (parent == null)
            {
                Debug.LogWarning($"[WaypointNavigator] Route parent for '{(useCorrect ? "correct" : "wrong")}' route is not assigned.");
                _waypoints = new Transform[0];
                _currentIndex = 0;
                return;
            }

            _waypoints = new Transform[parent.childCount];
            for (int i = 0; i < parent.childCount; i++)
                _waypoints[i] = parent.GetChild(i);

            _currentIndex = 0;
        }

        /// <summary>
        /// Teleports the player to the next waypoint and advances the internal index.
        /// Does nothing when all waypoints have been visited.
        /// </summary>
        public void TeleportNext()
        {
            if (!HasNextWaypoint)
            {
                Debug.LogWarning("[WaypointNavigator] TeleportNext called but no more waypoints.");
                return;
            }

            if (xrOriginRoot == null) return;

            Vector3 targetPosition = _waypoints[_currentIndex].position;

            // Disable CharacterController around the teleport to avoid physics conflicts.
            if (_characterController != null) _characterController.enabled = false;

            xrOriginRoot.position = targetPosition;

            if (_characterController != null) _characterController.enabled = true;

            _currentIndex++;
        }
    }
}
