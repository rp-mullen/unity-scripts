using UnityEngine;

[System.Serializable]
public class DialogueChoice
{
    public string choiceText; // The text of the choice
    public DialogueNode nextNode; // The dialogue node this choice leads to
}
