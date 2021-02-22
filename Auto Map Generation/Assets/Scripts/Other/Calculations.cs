using UnityEngine;

public class Calculations : MonoBehaviour
{
    /// <summary>
    /// Converts a linear scale (from 0.0 - 1.0) into its respective decibel (used for adjusting AudioMixer volumes)
    /// </summary>
    public static float LinearToDecibel (float linear)
    {
        float dB;
        if(linear != 0)
            dB = 20.0f * Mathf.Log10(linear);
        else
            dB = -80.0f;
        return dB;
    }

    /// <summary>
    /// Converts a decibel logarithmic scale into its respective linear scale (used for adjusting AudioMixer volumes)
    /// </summary>
    public static float DecibelToLinear (float dB)
    {
        float linear = Mathf.Pow(10.0f, dB / 20.0f);
        return linear;
    }

}
