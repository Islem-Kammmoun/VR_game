using UnityEngine;

namespace VRGame.UI
{
    /// <summary>
    /// Keeps a world-space UI element pinned to the player's view
    /// (e.g. top-right corner) so it stays visible & usable even if player falls.
    /// Attach this to the UI root (Canvas or a parent GameObject).
    /// </summary>
    public class FollowCameraCornerUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform targetCamera;

        [Header("Placement")]
        [SerializeField] private float distance = 1.2f;
        [SerializeField] private Vector2 viewportAnchor = new Vector2(0.92f, 0.92f); // top-right-ish

        [Header("Facing")]
        [SerializeField] private bool faceCamera = true;

        private void Reset()
        {
            Camera cam = Camera.main;
            if (cam != null) targetCamera = cam.transform;
        }

        private void LateUpdate()
        {
            if (targetCamera == null) return;

            Camera cam = targetCamera.GetComponent<Camera>();
            if (cam == null) cam = Camera.main;
            if (cam == null) return;

            // Place the UI at a point in the camera's view at a fixed distance.
            Vector3 worldPos = cam.ViewportToWorldPoint(new Vector3(viewportAnchor.x, viewportAnchor.y, distance));
            transform.position = worldPos;

            if (faceCamera)
            {
                Vector3 lookDir = transform.position - targetCamera.position;
                lookDir.y = 0f; // keep upright
                if (lookDir.sqrMagnitude > 0.0001f)
                    transform.rotation = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
            }
        }
    }
}