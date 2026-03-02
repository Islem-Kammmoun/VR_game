using UnityEngine;

namespace VRGame.World
{
    /// <summary>
    /// Attach to a trigger collider. Calls GameManager.Win() for a lake trigger
    /// or GameManager.Lose() for a hole trigger when the player enters.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class WinLoseTrigger : MonoBehaviour
    {
        public enum TriggerType { Hole, Lake }

        [SerializeField] private TriggerType type = TriggerType.Hole;
        [SerializeField] private string loseReason = "You fell into a hole.";

        private Core.GameManager _gameManager;

        private void Start()
        {
            _gameManager = FindFirstObjectByType<Core.GameManager>();
            if (_gameManager == null)
                Debug.LogError("[WinLoseTrigger] No GameManager found in the scene.");

            // Ensure the collider is set as a trigger.
            Collider col = GetComponent<Collider>();
            if (!col.isTrigger)
            {
                col.isTrigger = true;
                Debug.LogWarning("[WinLoseTrigger] Collider was not set as a trigger; corrected automatically.");
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_gameManager == null) return;
            if (!IsPlayer(other.transform)) return;

            switch (type)
            {
                case TriggerType.Hole:
                    _gameManager.Lose(loseReason);
                    break;
                case TriggerType.Lake:
                    _gameManager.Win();
                    break;
            }
        }

        /// <summary>
        /// Walks up the transform hierarchy to determine whether the collider
        /// belongs to the player (any ancestor tagged "Player").
        /// </summary>
        private static bool IsPlayer(Transform t)
        {
            while (t != null)
            {
                if (t.CompareTag("Player")) return true;
                t = t.parent;
            }
            return false;
        }
    }
}
