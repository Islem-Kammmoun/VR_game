using UnityEngine;
using TMPro;
using VRGame.World;

namespace VRGame.Core
{
    /// <summary>
    /// Manages game state transitions (Intro, Playing, Won, Lost) and
    /// coordinates UI panels, route display and player respawning.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public enum GameState { Intro, Playing, Won, Lost }

        [Header("Player")]
        [SerializeField] private Transform xrOriginRoot;
        [SerializeField] private Transform spawnPoint;

        [Header("Route")]
        [SerializeField] private RouteRenderer routeRenderer;

        [Header("UI Panels")]
        [SerializeField] private GameObject introPanel;
        [SerializeField] private GameObject chatPanel;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private GameObject losePanel;

        [Header("Status (optional)")]
        [SerializeField] private TMP_Text statusText;

        private GameState _state = GameState.Intro;

        private void Start()
        {
            ValidateReferences();
            SetState(GameState.Intro);
        }

        private void ValidateReferences()
        {
            if (xrOriginRoot == null) Debug.LogError("[GameManager] xrOriginRoot is not assigned.");
            if (spawnPoint == null) Debug.LogError("[GameManager] spawnPoint is not assigned.");
            if (routeRenderer == null) Debug.LogError("[GameManager] routeRenderer is not assigned.");
            if (introPanel == null) Debug.LogError("[GameManager] introPanel is not assigned.");
            if (chatPanel == null) Debug.LogError("[GameManager] chatPanel is not assigned.");
            if (winPanel == null) Debug.LogError("[GameManager] winPanel is not assigned.");
            if (losePanel == null) Debug.LogError("[GameManager] losePanel is not assigned.");
        }

        // ── Public API ──────────────────────────────────────────────────────────

        /// <summary>Hides the intro panel, shows the chat panel, and begins the game.</summary>
        public void StartGame()
        {
            SetState(GameState.Playing);
        }

        /// <summary>Triggers the win state and shows the win panel.</summary>
        public void Win()
        {
            if (_state != GameState.Playing) return;
            SetState(GameState.Won);
        }

        /// <summary>Triggers the lose state with a reason string shown on the lose panel.</summary>
        public void Lose(string reason)
        {
            if (_state != GameState.Playing) return;
            if (statusText != null) statusText.text = reason;
            SetState(GameState.Lost);
        }

        /// <summary>Teleports the player to the spawn point and resumes Playing state.</summary>
        public void Respawn()
        {
            if (xrOriginRoot == null || spawnPoint == null) return;

            // Temporarily disable any CharacterController to avoid collision glitches.
            CharacterController cc = xrOriginRoot.GetComponentInChildren<CharacterController>();
            if (cc != null) cc.enabled = false;

            xrOriginRoot.position = spawnPoint.position;
            Vector3 euler = xrOriginRoot.eulerAngles;
            xrOriginRoot.rotation = Quaternion.Euler(euler.x, spawnPoint.eulerAngles.y, euler.z);

            if (cc != null) cc.enabled = true;

            SetState(GameState.Playing);
        }

        /// <summary>Shows the wrong (old-map) route via the RouteRenderer.</summary>
        public void ShowWrongRoute()
        {
            if (routeRenderer != null) routeRenderer.ShowWrongRoute();
        }

        /// <summary>Shows the correct route via the RouteRenderer.</summary>
        public void ShowCorrectRoute()
        {
            if (routeRenderer != null) routeRenderer.ShowCorrectRoute();
        }

        /// <summary>Hides the route visualization.</summary>
        public void HideRoute()
        {
            if (routeRenderer != null) routeRenderer.Hide();
        }

        /// <summary>Returns to the Intro state (shows intro panel, hides others).</summary>
        public void ShowIntro()
        {
            SetState(GameState.Intro);
        }

        // ── Private helpers ─────────────────────────────────────────────────────

        private void SetState(GameState newState)
        {
            _state = newState;
            UpdatePanels();
        }

        private void UpdatePanels()
        {
            SetActive(introPanel, _state == GameState.Intro);
            SetActive(chatPanel, _state == GameState.Playing);
            SetActive(winPanel, _state == GameState.Won);
            SetActive(losePanel, _state == GameState.Lost);

            if (_state != GameState.Playing) HideRoute();
        }

        private static void SetActive(GameObject go, bool active)
        {
            if (go != null) go.SetActive(active);
        }
    }
}
