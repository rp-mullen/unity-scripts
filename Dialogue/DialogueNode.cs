using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Dialogue Node", menuName = "Dialogue/Node")]
public class DialogueNode : ScriptableObject
{
    [TextArea(3, 5)] public string dialogueText; // The text shown in the UI
    
    public List<DialogueChoice> choices = new List<DialogueChoice>(); // Branching choices

    [Header("Auto-Advance (Optional)")]
    public bool autoAdvance = false; // If true, moves to nextNode automatically

    [Header("Next Node (Embedded)")]
    public bool createNextNode = false; // Toggle to instantiate an embedded next node
    public DialogueNode nextNode; // Direct reference to the next node

    private void OnValidate()
    {
        // Auto-create the next node if toggled
        if (createNextNode && nextNode == null)
        {
            nextNode = CreateInstance<DialogueNode>();
            nextNode.name = name + "_Next"; // Give it a unique name
            createNextNode = false; // Reset toggle after creation
        }
    }
}
