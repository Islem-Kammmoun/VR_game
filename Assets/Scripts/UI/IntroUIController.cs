using UnityEngine;
using UnityEngine.UI;
using VRGame.Core;

namespace VRGame.UI
{
    public class IntroUIController : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private Button startButton;

        private void Start()
        {
            Debug.Log("[IntroUIController] Start() running on: " + gameObject.name);

            if (gameManager == null) Debug.LogError("[IntroUIController] GameManager is not assigned.");
            if (startButton == null) Debug.LogError("[IntroUIController] startButton is not assigned.");

            if (startButton != null)
                startButton.onClick.AddListener(OnStartClicked);
        }

        private void OnStartClicked()
        {
            Debug.Log("[IntroUIController] Start button clicked!");

            if (gameManager != null)
            {
                Debug.Log("[IntroUIController] Calling gameManager.ShowModePanel()");
                gameManager.ShowModePanel();
            }
            else
            {
                Debug.LogError("[IntroUIController] Clicked, but gameManager is null.");
            }
        }
    }
}