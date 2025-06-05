using UnityEngine;
[DefaultExecutionOrder(-100)]
public class DayNightCycleManager : MonoBehaviour
{
    public static DayNightCycleManager Instance { get; private set; }

    [Header("Time Settings")]
    [Range(0f, 24f)] public float timeOfDay = 12f;
    public float dayLengthInSeconds = 120f; // Full 24h cycle in 2 minutes
    public bool autoUpdateTime = true;

    public float TimeOfDay => timeOfDay;
    public float NormalizedTime => timeOfDay / 24f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Yeet the duplicate
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Keep across scenes
    }


    void Update()
    {
        if (!autoUpdateTime || dayLengthInSeconds <= 0f) return;

        float delta = (24f / dayLengthInSeconds) * Time.deltaTime;
        timeOfDay += delta;

        if (timeOfDay > 24f)
            timeOfDay -= 24f;
    }

    public void SetTime(float hour)
    {
        timeOfDay = Mathf.Repeat(hour, 24f);
    }

    public void SetTimeScale(float newDayLength)
    {
        dayLengthInSeconds = Mathf.Max(0.01f, newDayLength);
    }
}
