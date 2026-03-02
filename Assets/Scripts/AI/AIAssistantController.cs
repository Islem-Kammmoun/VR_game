using UnityEngine;
using VRGame.Core;

namespace VRGame.AI
{
    /// <summary>
    /// Rule-based AI assistant. Responds to player text messages with keyword matching.
    /// No external APIs are used.
    /// </summary>
    public class AIAssistantController : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;

        private bool _userAskedVerification = false;
        private bool _usingUpdatedMap = false;

        private void Start()
        {
            if (gameManager == null)
                Debug.LogError("[AIAssistantController] GameManager reference is not assigned.");
        }

        // ── Public API ──────────────────────────────────────────────────────────

        /// <summary>
        /// Processes a player message and returns an AI response string.
        /// Keywords are matched case-insensitively.
        /// </summary>
        public string ProcessUserMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return "I didn't quite catch that. Try asking me for directions or to verify my sources.";

            string lower = message.ToLowerInvariant();

            // 1. Verification / sources
            if (ContainsAny(lower, "verify", "verification", "source", "sources", "proof", "check", "confirm"))
            {
                _userAskedVerification = true;
                return "Good thinking! I should be transparent: I've been using an old map that may " +
                       "not reflect current terrain. To get the most accurate directions, you can ask " +
                       "me to 'update' my map or 'search again' so I can use the latest data.";
            }

            // 2. Update / new map
            if (ContainsAny(lower, "update", "new map", "latest", "search again", "refresh"))
            {
                _usingUpdatedMap = true;
                return "I've updated my sources and loaded the latest map. My previous directions were " +
                       "based on outdated data and would have led you somewhere dangerous. " +
                       "Ask me for the route again to see the corrected path.";
            }

            // 3. Route / path
            if (ContainsAny(lower, "road", "route", "path", "way", "directions",
                                   "show me the road", "show me the way"))
            {
                if (!_usingUpdatedMap)
                {
                    if (gameManager != null) gameManager.ShowWrongRoute();
                    return "According to my map, head north-east through the dense trees. " +
                           "The path looks clear — follow the marked route! " +
                           "(Note: these directions are based on my current map.)";
                }
                else
                {
                    if (gameManager != null) gameManager.ShowCorrectRoute();
                    return "With the updated map I can now see the correct path. " +
                           "Turn south-west past the large boulders and you'll reach the water lake. " +
                           "Follow the blue route — stay on the safe trail and you'll make it!";
                }
            }

            // 4. Fallback
            return "I can help you find water to survive! You can ask me:\n" +
                   "• \"Show me the road\" — to see the route\n" +
                   "• \"What sources are you using?\" — to verify my data\n" +
                   "• \"Update your map\" — to get fresh directions";
        }

        // ── Private helpers ─────────────────────────────────────────────────────

        private static bool ContainsAny(string text, params string[] keywords)
        {
            foreach (string kw in keywords)
            {
                if (text.Contains(kw)) return true;
            }
            return false;
        }
    }
}
