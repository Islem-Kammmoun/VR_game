using UnityEngine;
using UnityEngine.UI;
using VRGame.Core;

namespace VRGame.UI
{
    /// <summary>
    /// Drives the Verify Sources panel shown after the player's first loss.
    /// Presents the source information and lets the player decide whether to
    /// update the map or continue with the old (wrong) route.
    /// </summary>
    public class VerifySourcesPanelController : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;

        [Tooltip("Button: update the source and receive the correct route (C1..C5).")]
        [SerializeField] private Button updateSourceButton;

        [Tooltip("Button: ignore the source warning and keep using the wrong route (W1..W4).")]
        [SerializeField] private Button continueAnywayButton;

        private void Start()
        {
            if (gameManager == null) Debug.LogError("[VerifySourcesPanelController] GameManager is not assigned.");
            if (updateSourceButton == null) Debug.LogError("[VerifySourcesPanelController] updateSourceButton is not assigned.");
            if (continueAnywayButton == null) Debug.LogError("[VerifySourcesPanelController] continueAnywayButton is not assigned.");

            if (updateSourceButton != null)
                updateSourceButton.onClick.AddListener(OnUpdateSourceClicked);

            if (continueAnywayButton != null)
                continueAnywayButton.onClick.AddListener(OnContinueAnywayClicked);
        }

        private void OnUpdateSourceClicked()
        {
            if (gameManager != null) gameManager.UpdateSourceAndNewRoute();
        }

        private void OnContinueAnywayClicked()
        {
            if (gameManager != null) gameManager.ContinueAnyway();
        }
    }
}
