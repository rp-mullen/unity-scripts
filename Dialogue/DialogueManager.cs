using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText; // UI text for dialogue
    public GameObject choicesContainer; // Container for choice buttons
    public Button choiceButtonPrefab;   // Prefab for choices

    [Header("Typewriter Effect Settings")]
    public float textSpeed = 0.05f;     // Normal speed per character
    public float fastTextSpeed = 0.02f; // Faster speed after punctuation
    public float pauseTime = 0.3f;      // Pause time for periods
    public AudioSource beepSource;      // Audio source for beep sound
    public AudioClip beepClip;          // Beep sound for typing

    [Header("Dialogue Skipping Settings")]
    public float minSkipTime = 1.5f;    // Minimum time before skipping is allowed

    public KeyCode nextKey = KeyCode.E;

    // New fields for handling the parsed conversation.
    private ConversationCommand currentConversation;
    private int currentCommandIndex;
    
    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private float skipTimer = 0f; // Timer to track time since dialogue started
    private bool isDialogueActive = false;

    void Start()
    {
        dialoguePanel.SetActive(false);
    }

    /// <summary>
    /// Starts the dialogue using a ConversationCommand parsed from your custom text.
    /// </summary>
    public void StartDialogue(ConversationCommand conversation)
    {
        isDialogueActive = true;
        dialoguePanel.SetActive(true);
        currentConversation = conversation;
        currentCommandIndex = 0;
        skipTimer = 0f; // Reset skip timer
        ProcessCurrentCommand();
    }

    void Update()
    {
        if (isTyping)
        {
            skipTimer += Time.deltaTime;
        }
        if (isDialogueActive && Input.GetKeyDown(nextKey))
        {
            if (isTyping)
            {
                if (skipTimer >= minSkipTime)
                {
                    SkipTyping();
                }
            }
            else
            {
                AdvanceDialogue();
            }
        }
    }

    /// <summary>
    /// Processes the next command from the conversation.
    /// </summary>
    void ProcessCurrentCommand()
    {
        if (currentConversation == null || currentCommandIndex >= currentConversation.Commands.Count)
        {
            EndDialogue();
            return;
        }

        DialogueCommand command = currentConversation.Commands[currentCommandIndex];
        // Process based on command type.
        if (command is DialogueLineCommand lineCmd)
        {
            DisplayDialogue(lineCmd);
        }
        else if (command is ConditionCommand condCmd)
        {
            // For example, you might update some internal state.
            Debug.Log("Processing condition: " + condCmd.Condition);
            // Immediately advance after processing a condition.
            currentCommandIndex++;
            ProcessCurrentCommand();
        }
        else if (command is EmoteCommand emoteCmd)
        {
            // Trigger an emote animation or similar.
            Debug.Log("Processing emote: " + emoteCmd.Emote);
            // Immediately advance after processing an emote.
            currentCommandIndex++;
            ProcessCurrentCommand();
        }
        else
        {
            // Unknown command type, so simply skip it.
            currentCommandIndex++;
            ProcessCurrentCommand();
        }
    }

    /// <summary>
    /// Displays a dialogue line using the typewriter effect.
    /// </summary>
    void DisplayDialogue(DialogueLineCommand lineCmd)
    {
        if (isTyping && typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        dialogueText.text = "";
        typingCoroutine = StartCoroutine(TypeText(lineCmd.Text));
        ClearChoices();

        // If the dialogue line has conditional tags that indicate choices, create buttons.
        // (This is a simple example—adjust based on your engine's design.)
        if (lineCmd.ConditionalTags != null && lineCmd.ConditionalTags.Count > 0)
        {
            foreach (string tag in lineCmd.ConditionalTags)
            {
                Button choiceButton = Instantiate(choiceButtonPrefab, choicesContainer.transform);
                choiceButton.GetComponentInChildren<TextMeshProUGUI>().text = tag;
                // For this example, clicking a choice simply advances to the next command.
                choiceButton.onClick.AddListener(() => { AdvanceDialogue(); });
            }
        }
    }

    IEnumerator TypeText(string text)
    {
        isTyping = true;
        skipTimer = 0f;
        dialogueText.text = "";
        float currentSpeed = textSpeed;

        for (int i = 0; i < text.Length; i++)
        {
            char letter = text[i];
            dialogueText.text += letter;

            if (beepSource != null && beepClip != null && letter != ' ')
            {
                beepSource.PlayOneShot(beepClip);
            }

            // Slow down on punctuation.
            if (letter == '.')
            {
                yield return new WaitForSeconds(pauseTime);
                currentSpeed = fastTextSpeed;
            }
            else if (char.IsWhiteSpace(letter) || letter == ',' || letter == '!')
            {
                currentSpeed = fastTextSpeed;
            }
            else
            {
                currentSpeed = textSpeed;
            }

            yield return new WaitForSeconds(currentSpeed);
        }

        isTyping = false;
        ShowChoices();
    }

    void SkipTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        // Instantly display the full text for the current dialogue line.
        if (currentConversation.Commands[currentCommandIndex] is DialogueLineCommand lineCmd)
        {
            dialogueText.text = lineCmd.Text;
        }
        isTyping = false;
        ShowChoices();
    }

    void AdvanceDialogue()
    {
        // Advance to the next command.
        currentCommandIndex++;
        ProcessCurrentCommand();
    }

    /// <summary>
    /// Shows choices if available, or auto-advances after a delay if none are present.
    /// </summary>
    void ShowChoices()
    {
        // In this example, if the current command is a dialogue line and there are no choices,
        // we auto-advance after a delay.
        if (currentCommandIndex < currentConversation.Commands.Count && currentConversation.Commands[currentCommandIndex] is DialogueLineCommand)
        {
            Invoke(nameof(AdvanceDialogue), 1f);
        }
    }

    void ClearChoices()
    {
        foreach (Transform child in choicesContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }

    void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        dialogueText.text = "";
        ClearChoices();
        isDialogueActive = false;
        Object.FindFirstObjectByType<PlayerStateMachine>().SetState(PlayerState.Idle);
        
    }
}
