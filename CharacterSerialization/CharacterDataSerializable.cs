using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CharacterDataSerializable
{
    public string characterName;
    public Race race;
    public Class characterClass;

    public List<BlendshapeData> blendshapes;
    public float heightMultiplier;
    public Color hairColor;
    public Color skinColor;
    public Color eyeColor;
    public Color lipsColor;

    public List<string> equipmentLoadout;
    public string dialogueText;
    public DailySchedule dailySchedule;
}
   

