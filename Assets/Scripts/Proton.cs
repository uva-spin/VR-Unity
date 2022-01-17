using System.Collections.Generic;
using UnityEngine;
// using VRTK;

/// <summary>
/// Logic of movement and color swap quarks inside proton.
/// </summary>
public class Proton : MonoBehaviour
{
    [SerializeField]
    private List<Quark> quarks = new List<Quark>();

    [SerializeField]
    private FluxTube fluxTube;

    // [SerializeField]
    // private MouseOrbit mouseOrbitControl;

    private bool isZoomed = true;

    private Gluon gluon = null;

    private bool gluonIsInProcess;

    private int currentZoom = 0;

    [SerializeField]
    private Vector2[] zoomValues;

    private void Start()
    {
        // UpdateMouseOrbit();
        GenerateQuark();
    }

    public void ZoomIn()
    {
        if (zoomValues.Length == 0) return;

        currentZoom = Mathf.Min(currentZoom + 1, zoomValues.Length - 1);

        // UpdateMouseOrbit();
    }

    public void ZoomOut()
    {
        if (zoomValues.Length == 0) return;

        currentZoom = Mathf.Max(currentZoom - 1, 0);

        // UpdateMouseOrbit();
    }

    // private void UpdateMouseOrbit()
    // {
    //     if (zoomValues.Length == 0) return;

    //     mouseOrbitControl.MinDistance = zoomValues[currentZoom].x;
    //     mouseOrbitControl.MaxDistance = zoomValues[currentZoom].y;
    //     mouseOrbitControl.Distance = zoomValues[currentZoom].y;
    // }

    public void GenerateQuark()
    {
        if (gluonIsInProcess) return;

        Debug.Log("Generate Quarks");
        int sourceID = Random.Range(0, 3);
        int targetID = Random.Range(0, 3);

        while (sourceID == targetID)
            targetID = Random.Range(0, 3);

        var sourceQuark = quarks[sourceID];
        var targetQuark = quarks[targetID];

        var color = sourceQuark.GetColor();

        var anticolor = QuarkColor.AntiBlue;
        switch(targetQuark.GetColor())
        {
            case QuarkColor.Red: anticolor = QuarkColor.AntiRed; break;
            case QuarkColor.Green: anticolor = QuarkColor.AntiGreen; break;
            case QuarkColor.Blue: anticolor = QuarkColor.AntiBlue; break;
        }

        gluon = Gluon.CreateGluon(color, anticolor, fluxTube, sourceQuark, targetQuark);
        gluon.OnColorReachedQuark += OnGluonTransitionComplete;
        gluonIsInProcess = true;
    }

    private void OnGluonTransitionComplete()
    {
        gluonIsInProcess = false;
        fluxTube.SetGluon(null);
        Invoke("GenerateQuark", .05f);
    }
}