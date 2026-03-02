using UnityEngine;
using UnityEngine.UI;
using VRGame.Core;

namespace VRGame.UI
{
    /// <summary>
    /// Drives the intro panel. Hooks a start button to GameManager.StartGame().
    /// </summary>
    public class IntroUIController : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private Button startButton;

        private void Start()
        {
            if (gameManager == null) Debug.LogError("[IntroUIController] GameManager is not assigned.");
            if (startButton == null) Debug.LogError("[IntroUIController] startButton is not assigned.");

            if (startButton != null)
                startButton.onClick.AddListener(OnStartClicked);
        }

        private void OnStartClicked()
        {
            if (gameManager != null) gameManager.StartGame();
        }
    }
}
