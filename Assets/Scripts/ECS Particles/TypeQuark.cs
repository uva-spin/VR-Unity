using DG.Tweening;
using UnityEngine;
using System;

public class TypeQuark : MonoBehaviour
{
    [Tooltip("Is this particle charged?")] public bool isCharged = false;
    [Tooltip("Does this particle have a +2/3 charge (true), or -1/3 (false)?")] public bool charge = false;

    /// <summary>
    /// Color of sphere around quark.
    /// </summary>

    [SerializeField]
    [Tooltip("Color of sphere around quark")]
    public QuarkColor quarkColor;

    public Material sphereMaterial;
    public Material coreMaterial;
    public Material glowMaterial;

    protected TrailRenderer trailRenderer;
    protected LineRenderer forceLine;
    public float trailWidth = 3.5f;
    public float trailTime = 0.8f;

    public ParticleSystem partSys1;
    public ParticleSystem partSys2;
    public ParticleSystem partSys3;
    private ParticleSystem.MainModule ma1;
    private ParticleSystem.MainModule ma2;
    private ParticleSystem.MainModule ma3;

    private void Awake()
    {
        trailRenderer = GetComponent<TrailRenderer>();
        forceLine = GetComponent<LineRenderer>();

        UpdateColor();
    }

    /// <summary>
    /// Set color to quark.
    /// </summary>
    /// <param name="newColor"></param>
    public void SetColor(QuarkColor newColor)
    {
        quarkColor = newColor;
        UpdateColor();
    }

    protected void UpdateColor()
    {
        // sphereMaterial.SetColor("_Color", QuarkSettings.GetColorRGB(quarkColor));
        // coreMaterial.SetColor("_Color", QuarkSettings.GetColorRGB(quarkColor));
        // glowMaterial.SetColor("_Color", QuarkSettings.GetColorRGB(quarkColor));
        // trailRenderer.material.SetColor("_Color", QuarkSettings.GetColorRGB(quarkColor));
        ma1 = partSys1.main;
        ma2 = partSys2.main;
        ma3 = partSys3.main;
        if (partSys1) ma1.startColor = QuarkSettings.GetColorRGB(quarkColor);
        if (partSys2) ma2.startColor = QuarkSettings.GetColorRGB(quarkColor);
        if (partSys3) ma3.startColor = QuarkSettings.GetColorRGB(quarkColor);
    }

    /// <summary>
    /// Get quark's color.
    /// </summary>
    /// <returns></returns>
    public QuarkColor GetColor()
    {
        return quarkColor;
    }

    public void FlashToColor(QuarkColor toColor, Action onComplete = null)
    {
        Gradient g = new Gradient();
        Gradient gInv = new Gradient();

        Color[] normal = { QuarkSettings.GetColorRGB(quarkColor), QuarkSettings.GetColorRGB(toColor) };
        g.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(normal[0], 0),
                // new GradientColorKey(Color.white * 2f, 0.3f),
                new GradientColorKey(normal[1], 1)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(normal[0].a, 0),
                new GradientAlphaKey(1, 0.3f),
                new GradientAlphaKey(normal[1].a, 1)
            }
        );
        Color[] invt = { new Color(1f - normal[0].r, 1f - normal[0].g, 1f - normal[0].b, normal[0].a), new Color(1f - normal[1].r, 1f - normal[1].g, 1f - normal[1].b, normal[1].a) };
        gInv.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(invt[0], 0),
                new GradientColorKey(invt[1], 1)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(invt[0].a, 0),
                new GradientAlphaKey(1, 0.3f),
                new GradientAlphaKey(invt[1].a, 1)
            }
        );

        if (sphereMaterial)
        {
            sphereMaterial.DOGradientColor(g, "_Color", QuarkSettings.Instance.GluonFlashTime).OnComplete(() =>
            {
                SetColor(toColor);
                onComplete?.Invoke();
            });
        }
        if (glowMaterial)
        {
            glowMaterial.DOGradientColor(g, "_Color", QuarkSettings.Instance.GluonFlashTime).OnComplete(() =>
            {
                SetColor(toColor);
                onComplete?.Invoke();
            });
        }
        if (trailRenderer) trailRenderer.material.DOGradientColor(g, "_Color", QuarkSettings.Instance.TrailColorLerp);
        if (forceLine) forceLine.material.DOGradientColor(gInv, "_Color", QuarkSettings.Instance.TrailColorLerp);
    }
}
