using UnityEngine;

namespace VRGame.UI
{
    public class RestartButtonUI : MonoBehaviour
    {
        private VRGame.Core.GameManager _gm;

        private void Start()
        {
            _gm = FindFirstObjectByType<VRGame.Core.GameManager>();
            if (_gm == null)
                Debug.LogError("[RestartButtonUI] No GameManager found in the scene.");
        }

        public void Restart()
        {
            if (_gm == null) return;

            // Always works (even if player is mid-fall and Lose() hasn't fired yet)
            _gm.RestartToIntroFromAnywhere();
        }
    }
}