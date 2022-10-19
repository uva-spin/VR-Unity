using DG.Tweening;
using System;
using UnityEngine;

public enum QuarkColor
{
    Red,
    Green,
    Blue,
    GluonRed,
    GluonGreen,
    GluonBlue,
    AntiRed,
    AntiGreen,
    AntiBlue
}

/// <summary>
/// Set color and flavor of quark.
/// </summary>
public class Quark : TypeQuark
{
    [SerializeField]
    private GameObject quarkSphere;

    [SerializeField]
    private GameObject quarkCore;

    [SerializeField]
    private GameObject quarkGlow;

    public float Radius = 0.5f;

    private ChangeOrbit changeOrbit;

    public float f = 0.0f;

    [SerializeField]
    private AnimationCurve trailSizeCorrectionCurve;

    public float GetEstimatedSpeed()
    {
        return changeOrbit.EstimatedSpeed;
    }

    private void Awake()
    {
        sphereMaterial = quarkSphere.GetComponent<Renderer>().material;
        coreMaterial = quarkCore.GetComponent<Renderer>().material;
        glowMaterial = quarkGlow.GetComponent<Renderer>().material;
        trailRenderer = GetComponent<TrailRenderer>();
        forceLine = GetComponent<LineRenderer>();

        changeOrbit = GetComponent<ChangeOrbit>();

        UpdateColor();
    }

    private void Update()
    {
        float tNormalized = Mathf.Clamp01(changeOrbit.CurrentOrbitRadius / changeOrbit.MaxRadius);
        float coeff = trailSizeCorrectionCurve.Evaluate(tNormalized);

        trailRenderer.time = trailTime * coeff;
        trailRenderer.widthMultiplier = trailWidth;
        if (forceLine) forceLine.widthMultiplier = trailWidth / 10f;
    }
}