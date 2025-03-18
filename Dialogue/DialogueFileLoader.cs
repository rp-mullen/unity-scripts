using UnityEngine;
using System.IO;

public static class DialogueFileLoader
{
    /// <summary>
    /// Loads a conversation from a dialogue text file. The file is expected to have a .dlg extension.
    /// If the provided file path does not end with ".dlg", the extension is automatically appended.
    /// </summary>
    /// <param name="filePath">The full path to the dialogue file (without extension, or with .dlg).</param>
    /// <returns>A ConversationCommand object representing the parsed dialogue, or null if the file wasn't found.</returns>
    public static ConversationCommand LoadConversationFromFile(string filePath)
    {
        // Ensure filePath ends with ".dlg"
        if (!filePath.EndsWith(".dlg", System.StringComparison.OrdinalIgnoreCase))
        {
            filePath += ".dlg";
        }

        if (!File.Exists(filePath))
        {
            Debug.LogError("Dialogue file not found: " + filePath);
            return null;
        }

        string content = File.ReadAllText(filePath);
        // Parse the file's content into a ConversationCommand using your DialogueParser.
        ConversationCommand conversation = DialogueParser.Parse(content);
        return conversation;
    }

    /// <summary>
    /// Loads a conversation from a TextAsset. The asset is expected to have a .dlg extension in its name.
    /// </summary>
    /// <param name="dialogueAsset">The TextAsset containing the dialogue text.</param>
    /// <returns>A ConversationCommand object representing the parsed dialogue, or null if the asset is null.</returns>
    public static ConversationCommand LoadConversationFromTextAsset(TextAsset dialogueAsset)
    {
        if (dialogueAsset == null)
        {
            Debug.LogError("Dialogue TextAsset is null.");
            return null;
        }

        // Optionally, warn if the TextAsset's name doesn't end with ".dlg"
        if (!dialogueAsset.name.EndsWith(".dlg", System.StringComparison.OrdinalIgnoreCase))
        {
            Debug.LogWarning("Dialogue TextAsset does not have the expected .dlg extension: " + dialogueAsset.name);
        }

        ConversationCommand conversation = DialogueParser.Parse(dialogueAsset.text);
        return conversation;
    }
}
