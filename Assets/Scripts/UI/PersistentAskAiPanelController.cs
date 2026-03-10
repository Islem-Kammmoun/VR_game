using UnityEngine;
using UnityEngine.UI;
using VRGame.Core;

namespace VRGame.UI
{
    /// <summary>
    /// Drives the small persistent panel that is always visible during gameplay.
    /// Contains an "Ask AI" button (always active) and a "Verify Route and Sources"
    /// button that is enabled by GameManager only after the player's first loss.
    /// </summary>
    public class PersistentAskAiPanelController : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;

        [Tooltip("Button that opens the AI route-follow panel.")]
        [SerializeField] private Button askAiButton;

        [Tooltip("Button that opens the Verify Sources panel. " +
                 "GameManager controls the visibility of this button's GameObject.")]
        [SerializeField] private Button verifyButton;

        private void Start()
        {
            if (gameManager == null) Debug.LogError("[PersistentAskAiPanelController] GameManager is not assigned.");
            if (askAiButton == null) Debug.LogError("[PersistentAskAiPanelController] askAiButton is not assigned.");

            if (askAiButton != null)
                askAiButton.onClick.AddListener(OnAskAiClicked);

            if (verifyButton != null)
                verifyButton.onClick.AddListener(OnVerifyClicked);
        }

        private void OnAskAiClicked()
        {
            if (gameManager != null) gameManager.AskAI();
        }

        private void OnVerifyClicked()
        {
            if (gameManager != null) gameManager.ShowVerifySourcesPanel();
        }
    }
}
