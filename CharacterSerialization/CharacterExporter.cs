using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class BinaryCharacterExporter : MonoBehaviour
{
    // Reference to your ScriptableObject holding character info.
    public CharacterDataSerializable characterData;

    /// <summary>
    /// Exports the character data as a binary .char file.
    /// </summary>
    /// <param name="filePath">
    /// The target file path (without extension, or with .char).
    /// The exporter will ensure the path ends with ".char".
    /// </param>
    public void ExportCharacter(string filePath)
    {
        // Ensure filePath ends with ".char"
        if (!filePath.EndsWith(".char", System.StringComparison.OrdinalIgnoreCase))
        {
            filePath += ".char";
        }

        // Create the serializable container from the ScriptableObject.
        CharacterDataSerializable data = new CharacterDataSerializable
        {
            characterName = characterData.characterName,
            race = characterData.race,
            characterClass = characterData.characterClass,
            blendshapes = characterData.blendshapes,
            heightMultiplier = characterData.heightMultiplier,
            hairColor = characterData.hairColor,
            skinColor = characterData.skinColor,
            eyeColor = characterData.eyeColor,
            lipsColor = characterData.lipsColor,
            equipmentLoadout = characterData.equipmentLoadout,
            dialogueText = characterData.dialogueText,
            dailySchedule = characterData.dailySchedule
        };

        // Serialize the container to binary.
        BinaryFormatter bf = new BinaryFormatter();
        using (FileStream file = File.Create(filePath))
        {
            bf.Serialize(file, data);
        }

        Debug.Log("Character exported as binary to: " + filePath);
    }
}
