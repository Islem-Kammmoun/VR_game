using UnityEngine;
using UnityEngine.UI;

namespace VRGame.UI
{
    /// <summary>
    /// Plays a click sound for every Unity UI Button in the scene.
    /// Re-scans occasionally to hook buttons created at runtime.
    /// </summary>
    public class GlobalUIButtonSFX : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip clickClip;
        [Range(0f, 1f)][SerializeField] private float volume = 1f;

        [Header("Runtime buttons")]
        [Tooltip("How often to scan for new Buttons (seconds).")]
        [SerializeField] private float scanInterval = 1f;

        private float nextScanTime;

        private void Awake()
        {
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();

            if (audioSource == null)
                Debug.LogError("[GlobalUIButtonSFX] Missing AudioSource on the same GameObject.");

            if (clickClip == null)
                Debug.LogError("[GlobalUIButtonSFX] Missing clickClip reference.");
        }

        private void OnEnable()
        {
            HookAllButtons();
            nextScanTime = Time.unscaledTime + scanInterval;
        }

        private void Update()
        {
            if (Time.unscaledTime >= nextScanTime)
            {
                HookAllButtons();
                nextScanTime = Time.unscaledTime + scanInterval;
            }
        }

        private void HookAllButtons()
        {
            var buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
            foreach (var btn in buttons)
            {
                if (btn == null) continue;

                // Prevent duplicates if we scan multiple times
                btn.onClick.RemoveListener(PlayClick);
                btn.onClick.AddListener(PlayClick);
            }
        }

        private void PlayClick()
        {
            if (audioSource == null || clickClip == null) return;
            audioSource.PlayOneShot(clickClip, volume);
        }
    }
}