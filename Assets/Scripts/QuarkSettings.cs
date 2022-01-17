using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "Color", menuName = "ScriptableObjects/QuarkColor", order = 1)]
public class QuarkSettings : ScriptableObject
{
    [Header("Quark Red Color")]
    [ColorUsage(true, true)]
    public Color QuarkRed;

    [ColorUsage(true, true)]
    public Color GluonRed;

    [ColorUsage(true, true)]
    public Color AntiRed;

    [Header("Quark Green Color")]
    [ColorUsage(true, true)]
    public Color QuarkGreen;

    [ColorUsage(true, true)]
    public Color GluonGreen;

    [ColorUsage(true, true)]
    public Color AntiGreen;

    [Header("Quark Blue Color")]
    [ColorUsage(true, true)]
    public Color QuarkBlue;

    [ColorUsage(true, true)]
    public Color GluonBlue;

    [ColorUsage(true, true)]
    public Color AntiBlue;

    public float GluonSpeed = 400.0f;

    public float GluonFlashTime = 0.2f;

    public float QuarkSpeed = 10;

    public float QuarkMoveRadius = 1000;

    public float GlobalSpeedMultiplier = 1.0f;

    public float GlobalSlowdownCoefficient = 0.2f;

    public float TrailColorLerp = 0.2f;

    private bool slowMode = false;

    public void ToggleSlowMode()
    {
        if (!slowMode)
        {
            GlobalSpeedMultiplier *= GlobalSlowdownCoefficient;
            slowMode = true;
        }
        else
        {
            GlobalSpeedMultiplier /= GlobalSlowdownCoefficient;
            slowMode = false;
        }
    }

    public static QuarkSettings Instance
    {
        get
        {
            return Resources.Load<QuarkSettings>("QuarkSettings");
        }
    }

    public static Color GetColorRGB(QuarkColor quarkColor, bool gluon = false)
    {
        if (!gluon)
        {
            switch (quarkColor)
            {
                case QuarkColor.Red: return Instance.QuarkRed;
                case QuarkColor.Green: return Instance.QuarkGreen;
                case QuarkColor.Blue: return Instance.QuarkBlue;
                case QuarkColor.AntiRed: return Instance.AntiRed;
                case QuarkColor.AntiGreen: return Instance.AntiGreen;
                case QuarkColor.AntiBlue: return Instance.AntiBlue;

                default: return Instance.QuarkBlue;
            }
        }
        else
        {
            switch (quarkColor)
            {
                case QuarkColor.Red: return Instance.GluonRed;
                case QuarkColor.Green: return Instance.GluonGreen;
                case QuarkColor.Blue: return Instance.GluonBlue;
                case QuarkColor.AntiRed: return Instance.AntiRed;
                case QuarkColor.AntiGreen: return Instance.AntiGreen;
                case QuarkColor.AntiBlue: return Instance.AntiBlue;

                default: return Instance.QuarkBlue;
            }
        }
    }
}