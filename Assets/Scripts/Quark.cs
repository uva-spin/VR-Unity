using DG.Tweening;
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    /// <summary>
    /// Color of sphere around quark.
    /// </summary>

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

    [SerializeField] private AnimationCurve trailSizeCorrectionCurve;

    //electricty and magnetism variable
    [SerializeField] private List<Quark> collidingQuarks;
    private const float constantOfProportionality = /*9000000000*/100;// constant of proportionality
    [SerializeField] private float forceMultiplier; // change this to adjust the force, default is 1, all quarks should have same value
    [SerializeField] private float charge; // +2/3 for UP quarks and -1/3 for DOWN quarks

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
    public void OnTriggerEnter(Collider other)
    {
        if(other.gameObject != null && other.gameObject.GetComponent<Quark>() != null && !collidingQuarks.Contains(other.gameObject.GetComponent<Quark>()))
        {
            collidingQuarks.Add(other.gameObject.GetComponent<Quark>()); // assumes it only collides with quarks, change if it interacts with other particles
        }
            

    }
    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject != null && other.gameObject.GetComponent<Quark>() != null)
        {
            collidingQuarks.Remove(other.gameObject.GetComponent<Quark>()); // assumes it only collides with quarks, change if it interacts with other particles
        }
    }
    private void FixedUpdate()
    {
        Vector3 totalForce = new Vector3();
        foreach(Quark quark in collidingQuarks)
        {
            //use sqr magnitude bc it is faster to computer (sqrt is expensive) and we need r^2 for equation of charge interactions
            float distSquared = (gameObject.transform.position - quark.gameObject.transform.position).sqrMagnitude;
            float force = this.charge * quark.charge * forceMultiplier * constantOfProportionality / distSquared;
            Vector3 direction = (gameObject.transform.position - quark.gameObject.transform.position).normalized;// TODO figure this out
            totalForce += force * direction;
        }
        //Debug.Log(totalForce);
        gameObject.GetComponent<Rigidbody>().AddForce(totalForce, ForceMode.Force);
    }

    private void UpdateColor()
    {
        // sphereMaterial.SetColor("_Color", QuarkSettings.GetColorRGB(quarkColor));
        // coreMaterial.SetColor("_Color", QuarkSettings.GetColorRGB(quarkColor));
        // glowMaterial.SetColor("_Color", QuarkSettings.GetColorRGB(quarkColor));
        // trailRenderer.material.SetColor("_Color", QuarkSettings.GetColorRGB(quarkColor));
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

        UpdateColor();
    }

    private void Start()
    {
        //Debug.Log(constantOfProportionality);
        if (collidingQuarks == null)
        {
            collidingQuarks = new List<Quark>();
        }
    }

    private void Update()
    {
        float tNormalized = Mathf.Clamp01(changeOrbit.CurrentOrbitRadius / changeOrbit.MaxRadius);
        float coeff = trailSizeCorrectionCurve.Evaluate(tNormalized);

        trailRenderer.time = trailTime * coeff;
        trailRenderer.widthMultiplier = trailWidth;

    }

    public void FlashToColor(QuarkColor toColor, Action onComplete = null)
    {
        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(QuarkSettings.GetColorRGB(quarkColor), 0),
                // new GradientColorKey(Color.white * 2f, 0.3f),
                new GradientColorKey(QuarkSettings.GetColorRGB(toColor), 1)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(QuarkSettings.GetColorRGB(quarkColor).a, 0),
                new GradientAlphaKey(1, 0.3f),
                new GradientAlphaKey(QuarkSettings.GetColorRGB(toColor).a, 1)
            }
        );

        sphereMaterial.DOGradientColor(g, "_Color", QuarkSettings.Instance.GluonFlashTime).OnComplete(()=>
        {
            SetColor(toColor);
            onComplete?.Invoke();
        });

        glowMaterial.DOGradientColor(g, "_Color", QuarkSettings.Instance.GluonFlashTime).OnComplete(()=>
        {
            SetColor(toColor);
            onComplete?.Invoke();
        });

        trailRenderer.material.DOGradientColor(g, "_Color", QuarkSettings.Instance.TrailColorLerp);

    }
}