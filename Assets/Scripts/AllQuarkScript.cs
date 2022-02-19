using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AllQuarkScript : MonoBehaviour
{
    public Slider speedSlider;
    public float quarkSpeed = 3;
    public bool polarized = true;
    public Transform quark1;
    public Transform quark2;
    public Transform quark3;
    ChangeOrbit c1;
    ChangeOrbit c2;
    ChangeOrbit c3;

    public int polarizationAxis = 0;



    // Start is called before the first frame update
    void Start()
    {
        c1 = quark1.GetComponent<ChangeOrbit>();
        c2 = quark2.GetComponent<ChangeOrbit>();
        c3 = quark3.GetComponent<ChangeOrbit>();

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void TogglePolarization(){
        if(polarized){
            c1.SetPolarization(false);
            c2.SetPolarization(false);
            c3.SetPolarization(false);
            polarized = false;
        }
        else if(!polarized){
            c1.SetPolarization(true);
            c2.SetPolarization(true);
            c3.SetPolarization(true);
            polarized = true;
        }
        
    }

    public void TogglePolX(){
        c1.polarizationAxis = 0;
        c2.polarizationAxis = 0;
        c3.polarizationAxis = 0;
    }
    public void TogglePolY(){
        c1.polarizationAxis = 1;
        c2.polarizationAxis = 1;
        c3.polarizationAxis = 1;
    }
    public void TogglePolZ(){
        c1.polarizationAxis = 2;
        c2.polarizationAxis = 2;
        c3.polarizationAxis = 2;
    }
}
