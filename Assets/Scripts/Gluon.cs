using UnityEngine;
using System;
using PathCreation;
using DG.Tweening;

/// <summary>
/// This class implements gluon movement through flux tube.
/// </summary>
public class Gluon : MonoBehaviour
{
    public event Action OnColorReachedQuark;

    public  QuarkColor Color { get; private set; }
    public  QuarkColor AntiColor { get; private set; }

    private GameObject colorGObj;
    private GameObject antiColorGObj;

    private Material colorMat;
    private Material anticolorSpiderMat;
    private Material anticolorGlowMat;

    private Quark sourceQuark;
    private Quark targetQuark;
    private FluxTube fluxTube;

    private QuarkColor sourceQuarkColor;
    private QuarkColor targetQuarkColor;

    private PathCreator pathCreator;

    private float normalizedDistanceTravelled;

    /// <summary>
    /// Create gluon.
    /// </summary>
    public static Gluon CreateGluon(QuarkColor color, QuarkColor antiColor, FluxTube fluxTube, Quark sourceQuark, Quark targetQuark)
    {
        var go = Instantiate<GameObject>(Resources.Load<GameObject>("Gluon"));

        var result = go.AddComponent<Gluon>();

        result.colorGObj = result.transform.Find("GluonColor").gameObject;
        result.colorMat = result.colorGObj.GetComponentInChildren<Renderer>().material;
        result.colorMat.SetColor("_EmissionColor", QuarkSettings.GetColorRGB(color, true));

        result.anticolorSpiderMat = result.transform.Find("Spider").GetComponent<Renderer>().material;
        result.anticolorSpiderMat.SetColor("_Color", QuarkSettings.GetColorRGB(antiColor, true) + new Color(1, 0.5f, 0.5f, 0));

        result.anticolorGlowMat = result.transform.Find("Glow").GetComponent<Renderer>().material;
        result.anticolorGlowMat.SetColor("_Color", QuarkSettings.GetColorRGB(antiColor, true));

        result.Color = color;
        result.AntiColor = antiColor;

        result.sourceQuark = sourceQuark;
        result.targetQuark = targetQuark;
        result.fluxTube = fluxTube;
        fluxTube.SetGluon(result);

        result.InitGluon();

        return result;
    }

    private void InitGluon()
    {
        transform.position = targetQuark.transform.position;

        sourceQuarkColor = sourceQuark.GetColor();
        targetQuarkColor = targetQuark.GetColor();
        sourceQuark.FlashToColor(targetQuarkColor);

        CreatePath();
        UpdatePath();

        // To move gluon through flux tube we estimate time based on initial path length.
        // Then we tween normalized coefficient with that time.
        // Then each frame we compute gluon position based on that normalized value and current path
        normalizedDistanceTravelled = 0.0f;
        float t = pathCreator.path.length / QuarkSettings.Instance.GluonSpeed;

        var tween = DOTween.To(() => normalizedDistanceTravelled, v => normalizedDistanceTravelled = v, 1.0f, t);
        tween.SetEase(Ease.InOutQuad);
        tween.OnComplete(() => { GluonReachedQuark(); });
    }

    private void CreatePath()
    {
        var pathGO = new GameObject("GluonPath");
        pathCreator = pathGO.AddComponent<PathCreator>();

        Vector3[] points = new Vector3[3];
        BezierPath bezierPath = new BezierPath(points);
        bezierPath.ControlPointMode = BezierPath.ControlMode.Mirrored;
        pathCreator.bezierPath = bezierPath;
    }

    private void UpdatePath()
    {
        var d = (sourceQuark.transform.position + targetQuark.transform.position) / 2f; // Center point between quarks.
        var v = d - fluxTube.GetCenterPosition(); // Vector between d and center of flux tube
        var c = fluxTube.GetCenterPosition() + v.normalized * 0.1f; // the point for center point of the curve

        var b = (targetQuark.transform.position - sourceQuark.transform.position);
        var c1 = c - b.normalized * 0.25f;


        pathCreator.bezierPath.MovePoint(0, sourceQuark.transform.position);
        pathCreator.bezierPath.MovePoint(2, c1);
        pathCreator.bezierPath.MovePoint(3, c);
        pathCreator.bezierPath.MovePoint(6, targetQuark.transform.position);
    }

    private void GluonReachedQuark()
    {
        targetQuark.FlashToColor(sourceQuarkColor, OnColorReachedQuark);
        float destroyDelay = QuarkSettings.Instance.GluonFlashTime;
        GameObject.Destroy(gameObject, destroyDelay);
    }

    private void OnDisable()
    {
        if (pathCreator != null)
            Destroy(pathCreator.gameObject);
    }

    private void LateUpdate()
    {
        if (Camera.main != null)
            transform.LookAt(Camera.main.transform);

        UpdatePath();
        UpdateGluonPosition();
    }

    private void UpdateGluonPosition()
    {
        // Get current normalized distance which is tweened by DoTween and project it onto current path.
        float pathDistance = pathCreator.path.length * Mathf.Clamp(normalizedDistanceTravelled, 0.0f, 1.0f);
        Vector3 newPos = pathCreator.path.GetPointAtDistance(pathDistance, EndOfPathInstruction.Stop);
        transform.position = newPos;
    }
}
