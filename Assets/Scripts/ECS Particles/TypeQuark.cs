using DG.Tweening;
using UnityEngine;
using System;

public class TypeQuark : MonoBehaviour
{
    [Tooltip("Is this particle positive (true) or negative (false)")] public bool isPositive = false;
    [Tooltip("What type of charged particle is this?")] public Charges chargeType = Charges.UNCHARGED;

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
    [SerializeField] protected LineRenderer forceLine;
    [SerializeField] protected LineRenderer spinLine;
    public float trailWidth = 3.5f;
    public float trailTime = 0.8f;

    public ParticleSystem partSys1;
    public ParticleSystem partSys2;
    public ParticleSystem partSys3;
    private ParticleSystem.MainModule ma1;
    private ParticleSystem.MainModule ma2;
    private ParticleSystem.MainModule ma3;

    public Vector3 spin = Vector3.up * 0.5f;

    private void Awake()
    {
        LineRenderer[] lines = GetComponentsInChildren<LineRenderer>();
        trailRenderer = GetComponent<TrailRenderer>();

        if (!forceLine)
        {
            if (lines.Length > 0) forceLine = lines[1];
            if (lines.Length > 1) spinLine = lines[0];
        }
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

    public LineRenderer setVisualLine(VisualLines lineType, Vector3 start, Vector3 end) {
        LineRenderer lr = getLineType(lineType);
        if (lr) lr.SetPositions(new Vector3[] { start, end });
        return lr;
    }

    public LineRenderer setVisualLine(VisualLines lineType, Vector3 direction) {
        LineRenderer lr = getLineType(lineType);
        if (lr) lr.SetPositions(new Vector3[] { transform.position, transform.position + direction });
        return lr;
    }

    public LineRenderer getLineType(VisualLines lineType) {
        switch (lineType)
        {
            case VisualLines.FORCELINE:
                return forceLine;
            case VisualLines.SPINLINE:
                return spinLine;
            default:
                return null;
        }
    }

    //Returns float value for charge types
    public float getCharge()
    {
        switch (chargeType)
        {
            case Charges.UNCHARGED:
                return 0;
            case Charges.DOWN:
                return 1 / 3f;
            case Charges.UP:
                return 2 / 3f;
            default:
                return 1;
        }
    }
}
/** 
 * Type of line
 * Used to distinguish different visual lines
 * **/
public enum VisualLines { 
    FORCELINE, SPINLINE
}


/** 
 * Uncharged    = 0 charge
 * Down         = 1/3 charge
 * Up           = 2/3 charge
 * Full         = 1 charge
 * Probe        = 1 charge, no EM influence
 * Field        = 1 charge, not EM influenced
**/
public enum Charges { 
    UNCHARGED, DOWN, UP, FULL, PROBE, FIELD
}
