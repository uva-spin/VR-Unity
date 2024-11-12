using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

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
public class Quark : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Color of sphere around quark")]
    public QuarkColor quarkColor;

    [SerializeField]
    private GameObject quarkSphere;

    [SerializeField]
    private GameObject quarkCore;

    [SerializeField]
    private GameObject quarkGlow;

    private TrailRenderer trailRenderer;

    public Material sphereMaterial;
    public Material coreMaterial;
    public Material glowMaterial;

    public float Radius = 0.5f;
    private ChangeOrbit changeOrbit;

    public float trailWidth = 3.5f;
    public float trailTime = 0.8f;
    public float f = 0.0f;

    public ParticleSystem partSys1;
    public ParticleSystem partSys2;
    public ParticleSystem partSys3;

    private ParticleSystem.MainModule ma1;
    private ParticleSystem.MainModule ma2;
    private ParticleSystem.MainModule ma3;

    [SerializeField]
    private AnimationCurve trailSizeCorrectionCurve;

    [SerializeField]
    private Slider particleSlider;  // Reference to the UI slider component

    public float maxEmissionRate = 100f;  // Maximum emission rate for particles
    public float maxParticleSize = 1.0f;  // Maximum size for particles

    /// <summary>
    /// Set color to quark.
    /// </summary>
    /// <param name="newColor"></param>
    public void SetColor(QuarkColor newColor)
    {
        quarkColor = newColor;
        UpdateColor();
    }

    public float GetEstimatedSpeed()
    {
        return changeOrbit.EstimatedSpeed;
    }

    private void UpdateColor()
    {
        ma1 = partSys1.main;
        ma2 = partSys2.main;
        ma3 = partSys3.main;

        ma1.startColor = QuarkSettings.GetColorRGB(quarkColor);
        ma2.startColor = QuarkSettings.GetColorRGB(quarkColor);
        ma3.startColor = QuarkSettings.GetColorRGB(quarkColor);
    }

    /// <summary>
    /// Get quark's color.
    /// </summary>
    /// <returns></returns>
    public QuarkColor GetColor()
    {
        return quarkColor;
    }

    private void Awake()
    {
        sphereMaterial = quarkSphere.GetComponent<Renderer>().material;
        coreMaterial = quarkCore.GetComponent<Renderer>().material;
        glowMaterial = quarkGlow.GetComponent<Renderer>().material;
        trailRenderer = GetComponent<TrailRenderer>();

        changeOrbit = GetComponent<ChangeOrbit>();

        if (particleSlider != null)
        {
            particleSlider.onValueChanged.AddListener(UpdateParticleEmission);
        }

        UpdateColor();
    }

    private void Start()
    {
        if (particleSlider != null)
        {
            UpdateParticleEmission(particleSlider.value);
        }
    }

    private void Update()
    {
        float tNormalized = Mathf.Clamp01(changeOrbit.CurrentOrbitRadius / changeOrbit.MaxRadius);
        float coeff = trailSizeCorrectionCurve.Evaluate(tNormalized);

        trailRenderer.time = trailTime * coeff;
        trailRenderer.widthMultiplier = trailWidth;
    }

    public void UpdateParticleEmission(float sliderValue)
    {
        ma1 = partSys1.main;
        ma2 = partSys2.main;
        ma3 = partSys3.main;

        float emissionRate = Mathf.Lerp(0, maxEmissionRate, sliderValue);
        float particleSize = Mathf.Lerp(0.1f, maxParticleSize, sliderValue);

        var emission1 = partSys1.emission;
        emission1.rateOverTime = emissionRate;

        var emission2 = partSys2.emission;
        emission2.rateOverTime = emissionRate;

        var emission3 = partSys3.emission;
        emission3.rateOverTime = emissionRate;

        ma1.startSize = particleSize;
        ma2.startSize = particleSize;
        ma3.startSize = particleSize;
    }

    public void FlashToColor(QuarkColor toColor, Action onComplete = null)
    {
        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(QuarkSettings.GetColorRGB(quarkColor), 0),
                new GradientColorKey(QuarkSettings.GetColorRGB(toColor), 1)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(QuarkSettings.GetColorRGB(quarkColor).a, 0),
                new GradientAlphaKey(1, 0.3f),
                new GradientAlphaKey(QuarkSettings.GetColorRGB(toColor).a, 1)
            }
        );

        sphereMaterial.DOGradientColor(g, "_Color", QuarkSettings.Instance.GluonFlashTime).OnComplete(() =>
        {
            SetColor(toColor);
            onComplete?.Invoke();
        });

        glowMaterial.DOGradientColor(g, "_Color", QuarkSettings.Instance.GluonFlashTime).OnComplete(() =>
        {
            SetColor(toColor);
            onComplete?.Invoke();
        });

        trailRenderer.material.DOGradientColor(g, "_Color", QuarkSettings.Instance.TrailColorLerp);
    }
}