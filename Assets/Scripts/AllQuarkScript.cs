using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AllQuarkScript : MonoBehaviour
{
    public Slider speedSlider;
    public Slider forceSlider;
    public float quarkSpeed = 3;
    public bool polarized = true;
    public Transform quark1;
    public Transform quark2;
    public Transform quark3;
    ChangeOrbit c1;
    ChangeOrbit c2;
    ChangeOrbit c3;

    public PolarizationAxis polarizationAxis = PolarizationAxis.Z;

    public bool forceLineVis = true;

    PartonSpinAdjuster spinAdjuster;

    // Start is called before the first frame update
    void Start()
    {
        c1 = quark1.GetComponent<ChangeOrbit>();
        c2 = quark2.GetComponent<ChangeOrbit>();
        c3 = quark3.GetComponent<ChangeOrbit>();

        spinAdjuster = FindObjectOfType<PartonSpinAdjuster>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void TogglePolarization(){
        polarized = !polarized;  //Invert the current polarization, then set new value to each Orbit
        c1.SetPolarization(polarized);
        c2.SetPolarization(polarized);
        c3.SetPolarization(polarized);
        spinAdjuster.SetPolarization(polarized ? polarizationAxis : PolarizationAxis.NONE); //If you are polarized, set the polarization axis of the PSA to the axis, otherwise none
    }

    public void TogglePolDir(int XYZ){
        PolarizationAxis direction = getDirection(XYZ);
        c1.polarizationAxis = direction;
        c2.polarizationAxis = direction;
        c3.polarizationAxis = direction;
        polarizationAxis = direction;

        if (polarized) spinAdjuster.SetPolarization(direction); //Only set PSA axis if polarized, otherwise keep it none
    }

    PolarizationAxis getDirection(int XYZ) {
        switch (XYZ) {
            case 0:
                return PolarizationAxis.X;
            case 1:
                return PolarizationAxis.Y;
            case 2:
                return PolarizationAxis.Z;
            default:
                return PolarizationAxis.NONE;
        }
    }

    public void Disable()
    {
        forceLineVis = !forceLineVis;
    }
    public void SpinLines() {
        FindObjectOfType<PartonSpinAdjuster>().spinVis();
    }
}

public enum PolarizationAxis { 
    X, Y, Z, NONE
}
