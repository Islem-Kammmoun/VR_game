using UnityEngine;
using UnityEngine.UI;
using VRGame.Core;

namespace VRGame.UI
{
    /// <summary>
    /// Drives the win/lose result panels.
    /// Try Again restarts the experience by returning to the Intro flow.
    /// Back to Intro (optional) also returns to the intro panel.
    /// </summary>
    public class ResultUIController : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private Button tryAgainButton;
        [SerializeField] private Button backToIntroButton;

        private void Start()
        {
            if (gameManager == null) Debug.LogError("[ResultUIController] GameManager is not assigned.");
            if (tryAgainButton == null) Debug.LogError("[ResultUIController] tryAgainButton is not assigned.");

            if (tryAgainButton != null)
                tryAgainButton.onClick.AddListener(OnTryAgain);

            if (backToIntroButton != null)
                backToIntroButton.onClick.AddListener(OnBackToIntro);
        }

        /// <summary>
        /// Restarts the UI flow (Intro -> Mode -> Ask AI...).
        /// Use this if you want panels to show again, not just respawn in-place.
        /// </summary>
        private void OnTryAgain()
        {
            if (gameManager != null) gameManager.ShowIntro();
        }

        /// <summary>Returns to the intro screen (optional flow).</summary>
        private void OnBackToIntro()
        {
            if (gameManager != null) gameManager.ShowIntro();
        }
    }
}