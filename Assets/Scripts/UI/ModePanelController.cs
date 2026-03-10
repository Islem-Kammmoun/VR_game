using UnityEngine;
using UnityEngine.UI;
using VRGame.Core;

namespace VRGame.UI
{
    /// <summary>
    /// Drives the Mode panel that appears after the Start button.
    /// Offers two choices: Ask AI for help, or Start Alone.
    /// </summary>
    public class ModePanelController : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private Button askAiButton;
        [SerializeField] private Button startAloneButton;

        private void Start()
        {
            if (gameManager == null) Debug.LogError("[ModePanelController] GameManager is not assigned.");
            if (askAiButton == null) Debug.LogError("[ModePanelController] askAiButton is not assigned.");
            if (startAloneButton == null) Debug.LogError("[ModePanelController] startAloneButton is not assigned.");

            if (askAiButton != null)
                askAiButton.onClick.AddListener(OnAskAiClicked);

            if (startAloneButton != null)
                startAloneButton.onClick.AddListener(OnStartAloneClicked);
        }

        private void OnAskAiClicked()
        {
            if (gameManager != null) gameManager.AskAI();
        }

        private void OnStartAloneClicked()
        {
            if (gameManager != null) gameManager.StartAlone();
        }
    }
}
