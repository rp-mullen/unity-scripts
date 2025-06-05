using UnityEngine;

public class DayNightAppearance : MonoBehaviour
{
    [Header("Skybox Material")]
    [SerializeField] private Material skyboxMaterial;

    [Header("Sky Gradients (0 = Midnight, 1 = Next Midnight)")]
    public Gradient topColorGradient;
    public Gradient bottomColorGradient;
    public Gradient cloudColorGradient;

    [Header("Time Phase Settings")]
    public TimeOfDayParameters dayParameters;
    public TimeOfDayParameters duskDawnParameters;
    public TimeOfDayParameters nightParameters;

    [Header("Sunrise/Sunset (Real Hours)")]
    [Range(0f, 24f)] public float sunriseTime = 6f;
    [Range(0f, 24f)] public float sunsetTime = 18f;

    [Header("Fog & Star Settings")]
    public AnimationCurve starAlphaCurve;
    public Gradient fogColorGradient;
    public AnimationCurve sunIntensityCurve;

    [Header("Events")]
    public System.Action OnDay;
    public System.Action OnNight;

    private bool isCurrentlyNight = false;
    private float fadeMargin = 0.5f;

    void Start()
    {
    }

    void Update()
    {
        float t = DayNightCycleManager.Instance?.NormalizedTime ?? 0.5f;
        float hour = t * 24f;

        TimeOfDayParameters currentParams = GetBlendedParameters(hour);

        // Apply skybox colors from gradients
        SetSkyboxColor("_SkyTopColor", topColorGradient.Evaluate(t));
        SetSkyboxColor("_SkyBottomColor", bottomColorGradient.Evaluate(t));
        SetSkyboxColor("_CloudColor", cloudColorGradient.Evaluate(t));

        // Set Star Alpha
        if (skyboxMaterial.HasProperty("_StarAlpha"))
        {
            float starAlpha = starAlphaCurve.Evaluate(t);
            skyboxMaterial.SetFloat("_StarAlpha", starAlpha);
        }

        // Update fog and sun
        RenderSettings.fogColor = fogColorGradient.Evaluate(t);
        if (RenderSettings.sun != null)
        {
            RenderSettings.sun.intensity = sunIntensityCurve.Evaluate(t);
        }

        float lightingFactor = EvaluateLightingFactor(t);

        RenderSettings.ambientIntensity = Mathf.Lerp(
            nightParameters.envLightingIntensityMult,
            currentParams.envLightingIntensityMult,
            lightingFactor
        );

        RenderSettings.reflectionIntensity = Mathf.Lerp(
            nightParameters.envLightingReflectionMult,
            currentParams.envLightingReflectionMult,
            lightingFactor
        );


        // Optional night/day events
        bool nowNight = (hour < sunriseTime - fadeMargin || hour > sunsetTime + fadeMargin);
        if (nowNight && !isCurrentlyNight)
        {
            isCurrentlyNight = true;
            OnNight?.Invoke();
        }
        else if (!nowNight && isCurrentlyNight)
        {
            isCurrentlyNight = false;
            OnDay?.Invoke();
        }
    }

    private float EvaluateLightingFactor(float t)
    {
        if (t < 0.3f)
        {
            return Mathf.InverseLerp(0f, 0.3f, t); // Ramp up
        }
        else if (t < 0.73f)
        {
            return 1f; // Hold at peak
        }
        else
        {
            return Mathf.InverseLerp(1f, 0.73f, t); // Ramp down
        }
    }


    private TimeOfDayParameters GetBlendedParameters(float hour)
    {
        if (hour >= sunriseTime - fadeMargin && hour < sunriseTime + fadeMargin)
            return duskDawnParameters;

        if (hour >= sunriseTime + fadeMargin && hour < sunsetTime - fadeMargin)
            return dayParameters;

        if (hour >= sunsetTime - fadeMargin && hour < sunsetTime + fadeMargin)
            return duskDawnParameters;

        return nightParameters;
    }

    private void SetSkyboxColor(string property, Color color)
    {
        if (skyboxMaterial != null && skyboxMaterial.HasProperty(property))
            skyboxMaterial.SetColor(property, color);
    }
}

[System.Serializable]
public class TimeOfDayParameters
{
    public float envLightingIntensityMult;
    public float envLightingReflectionMult;
    public Color fogColor;
    public float sunIntensity;
}
