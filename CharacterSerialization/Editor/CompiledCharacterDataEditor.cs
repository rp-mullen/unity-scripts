using UnityEngine;
using UnityEditor;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;

[CustomEditor(typeof(CompiledCharacterData))]
public class CompiledCharacterDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector first (so you can assign the TextAsset, etc.)
        DrawDefaultInspector();

        CompiledCharacterData dataAsset = (CompiledCharacterData)target;

        if (dataAsset.binaryData != null)
        {
            if (GUILayout.Button("Deserialize and Show Data"))
            {
                // Attempt to deserialize the binary data.
                BinaryFormatter bf = new BinaryFormatter();
                MemoryStream ms = new MemoryStream(dataAsset.binaryData.bytes);
                try
                {
                    CharacterDataSerializable characterData = (CharacterDataSerializable)bf.Deserialize(ms);
                    ms.Close();

                    // Display some of the deserialized fields.
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Deserialized Character Data", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("Name:", characterData.characterName);
                    EditorGUILayout.LabelField("Race:", characterData.race.ToString());
                    EditorGUILayout.LabelField("Class:", characterData.characterClass.ToString());
                    EditorGUILayout.LabelField("Height Multiplier:", characterData.heightMultiplier.ToString());
                    EditorGUILayout.LabelField("Hair Color:", characterData.hairColor.ToString());
                    EditorGUILayout.LabelField("Skin Color:", characterData.skinColor.ToString());
                    EditorGUILayout.LabelField("Eye Color:", characterData.eyeColor.ToString());
                    EditorGUILayout.LabelField("Lips Color:", characterData.lipsColor.ToString());
                    EditorGUILayout.LabelField("Dialogue Text:");
                    EditorGUILayout.HelpBox(characterData.dialogueText, MessageType.None);

                    // Optionally, display additional fields (like blendshapes, equipment, schedule, etc.)
                }
                catch (Exception ex)
                {
                    EditorGUILayout.HelpBox("Deserialization failed: " + ex.Message, MessageType.Error);
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No binary data assigned.", MessageType.Info);
        }
    }
}
