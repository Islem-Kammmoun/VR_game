using UnityEngine;

namespace VRGame.World
{
    /// <summary>
    /// Renders a route through a list of waypoint Transforms using a LineRenderer.
    /// Supports two routes: a wrong route (old map) and a correct route (updated map).
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class RouteRenderer : MonoBehaviour
    {
        [Header("Line")]
        [SerializeField] private LineRenderer line;

        [Header("Waypoint Parents")]
        [SerializeField] private Transform wrongRouteParent;
        [SerializeField] private Transform correctRouteParent;

        [Header("Options")]
        [SerializeField] private bool autoCollectChildren = true;
        [SerializeField] private bool setColorOnShow = true;

        [Header("Colors")]
        [SerializeField] private Color wrongColor = new Color(1f, 0.85f, 0f);   // yellow-ish
        [SerializeField] private Color correctColor = new Color(0f, 0.5f, 1f);  // blue

        private void Awake()
        {
            if (line == null) line = GetComponent<LineRenderer>();
            if (line == null) Debug.LogError("[RouteRenderer] LineRenderer component not found.");
            Hide();
        }

        // ── Public API ──────────────────────────────────────────────────────────

        /// <summary>Shows the wrong route using the wrongRouteParent's child transforms as waypoints.</summary>
        public void ShowWrongRoute()
        {
            if (wrongRouteParent == null)
            {
                Debug.LogError("[RouteRenderer] wrongRouteParent is not assigned.");
                return;
            }
            ShowRoute(wrongRouteParent, setColorOnShow ? wrongColor : Color.white);
        }

        /// <summary>Shows the correct route using the correctRouteParent's child transforms as waypoints.</summary>
        public void ShowCorrectRoute()
        {
            if (correctRouteParent == null)
            {
                Debug.LogError("[RouteRenderer] correctRouteParent is not assigned.");
                return;
            }
            ShowRoute(correctRouteParent, setColorOnShow ? correctColor : Color.white);
        }

        /// <summary>Hides the route by disabling the LineRenderer.</summary>
        public void Hide()
        {
            if (line != null) line.enabled = false;
        }

        // ── Private helpers ─────────────────────────────────────────────────────

        private void ShowRoute(Transform parent, Color color)
        {
            if (line == null) return;

            Transform[] waypoints = CollectChildren(parent);

            if (waypoints.Length < 2)
            {
                Debug.LogWarning($"[RouteRenderer] Route '{parent.name}' has fewer than 2 waypoints.");
                Hide();
                return;
            }

            line.positionCount = waypoints.Length;
            for (int i = 0; i < waypoints.Length; i++)
                line.SetPosition(i, waypoints[i].position);

            if (setColorOnShow)
            {
                line.startColor = color;
                line.endColor = color;
            }

            line.enabled = true;
        }

        private Transform[] CollectChildren(Transform parent)
        {
            if (!autoCollectChildren || parent == null)
                return new Transform[0];

            Transform[] children = new Transform[parent.childCount];
            for (int i = 0; i < parent.childCount; i++)
                children[i] = parent.GetChild(i);

            return children;
        }
    }
}
