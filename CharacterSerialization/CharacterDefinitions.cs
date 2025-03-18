using System;
using UnityEngine;
using System.Collections.Generic;

public enum Race
{
    Human,
    Elf,
    Dwarf,
    Orc
}

public enum Class
{
    Warrior,
    Mage,
    Rogue,
    Archer
}

[Serializable]
public class DailySchedule
{
    [Serializable]
    public class ScheduleEntry
    {
        [Tooltip("Start time in hours (0 - 24). For example, 8.5 = 8:30 AM")]
        public float startTime;
        
        [Tooltip("End time in hours (0 - 24). For example, 17 = 5:00 PM")]
        public float endTime;
        
        [Tooltip("Description of the activity (e.g., 'Working', 'Sleeping', 'Training')")]
        public string activity;
        
        [Tooltip("Location where the activity takes place (e.g., 'Home', 'Marketplace', 'Barracks')")]
        public string location;
    }

    [Tooltip("List of scheduled activities for the day")]
    public List<ScheduleEntry> entries = new List<ScheduleEntry>();
}

 [Serializable]
    public class BlendshapeData {
        string blendShapeName;
        float blendWeight;
    }