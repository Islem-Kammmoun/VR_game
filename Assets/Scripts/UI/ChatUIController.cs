using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VRGame.AI;

namespace VRGame.UI
{
    /// <summary>
    /// Handles the in-game chat panel. Forwards player messages to the
    /// AIAssistantController and displays the conversation.
    /// </summary>
    public class ChatUIController : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private TMP_Text conversationText;
        [SerializeField] private Button sendButton;
        [SerializeField] private AIAssistantController assistant;

        private void Start()
        {
            ValidateReferences();

            if (sendButton != null)
                sendButton.onClick.AddListener(Send);

            if (inputField != null)
                inputField.onSubmit.AddListener(_ => Send());
        }

        private void ValidateReferences()
        {
            if (inputField == null) Debug.LogError("[ChatUIController] inputField is not assigned.");
            if (conversationText == null) Debug.LogError("[ChatUIController] conversationText is not assigned.");
            if (sendButton == null) Debug.LogError("[ChatUIController] sendButton is not assigned.");
            if (assistant == null) Debug.LogError("[ChatUIController] assistant is not assigned.");
        }

        /// <summary>Reads the current input, queries the AI, and appends the exchange to the log.</summary>
        private void Send()
        {
            if (inputField == null || assistant == null) return;

            string msg = inputField.text.Trim();
            if (string.IsNullOrEmpty(msg)) return;

            string response = assistant.ProcessUserMessage(msg);
            AppendConversation(msg, response);

            inputField.text = string.Empty;
            inputField.ActivateInputField();
        }

        private void AppendConversation(string playerMsg, string aiResponse)
        {
            if (conversationText == null) return;
            conversationText.text += $"Player: {playerMsg}\nAI: {aiResponse}\n\n";
        }
    }
}
