using UnityEngine;

namespace VRGame.World
{
    /// <summary>
    /// Attach to a trigger collider. Calls GameManager.Win() for a lake trigger
    /// or GameManager.Lose() for a hole trigger when the player enters.
    /// Also checks OnTriggerStay to handle teleporting/starting inside the trigger.
    ///
    /// Important:
    /// - This trigger must be able to fire again after "Play Again" without reloading the scene.
    /// - We reset the internal _fired flag whenever GameManager restarts the run.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class WinLoseTrigger : MonoBehaviour
    {
        public enum TriggerType { Hole, Lake }

        [SerializeField] private TriggerType type = TriggerType.Hole;
        [SerializeField] private string loseReason = "You fell into a hole.";

        private Core.GameManager _gameManager;
        private bool _fired;

        private void Awake()
        {
            // Subscribe early so we don't miss the event
            Core.GameManager.OnRunRestarted += HandleRunRestarted;
        }

        private void OnDestroy()
        {
            Core.GameManager.OnRunRestarted -= HandleRunRestarted;
        }

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

            // Safe default: allow firing at start
            _fired = false;
        }

        private void OnEnable()
        {
            // Still reset when re-enabled (works if you ever disable/enable triggers)
            _fired = false;
        }

        private void HandleRunRestarted()
        {
            // This is the important part: allow firing again after Play Again / Try Again
            _fired = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            TryFire(other);
        }

        private void OnTriggerStay(Collider other)
        {
            // Important for teleporting into the trigger (OnTriggerEnter may not fire).
            TryFire(other);
        }

        private void TryFire(Collider other)
        {
            if (_fired) return;
            if (_gameManager == null) return;
            if (!IsPlayer(other.transform)) return;

            _fired = true;

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