using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour
{
    public GameObject menu;
    public GameObject speedSlider;
    public GameObject trailWidthSlider;
    public Slider sliderComp;
    public Slider trailWidths;
    public InputActionReference controllerMenuButton; // menu toggle button on vr controller
    public GameObject hmd;
    public float menuDistance;

    public GameObject quark1;
    public GameObject quark2;
    public GameObject quark3;

    Quark quark1comp;
    Quark quark2comp;
    Quark quark3comp;

    // Start is called before the first frame update
    void Start()
    {
        sliderComp = speedSlider.GetComponent<Slider>();
        trailWidths = trailWidthSlider.GetComponent<Slider>();
        quark1comp = quark1.GetComponent<Quark>();
        quark2comp = quark2.GetComponent<Quark>();
        quark3comp = quark3.GetComponent<Quark>();
        
    }

    // Update is called once per frame
    void Update()
    {
        Time.timeScale = sliderComp.value;
        quark1comp.trailWidth = trailWidths.value;
        quark2comp.trailWidth = trailWidths.value;
        quark3comp.trailWidth = trailWidths.value;

        if (controllerMenuButton.action.triggered)
        {
            menu.SetActive(!menu.activeSelf);
            // move menu according to headset
            if (menu.activeSelf)
                menu.transform.SetPositionAndRotation(
                    hmd.transform.position + hmd.transform.forward * menuDistance, hmd.transform.rotation);
        }
    }
}
