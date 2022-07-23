using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour
{
    public GameObject menu;
    public GameObject speedSlider;
    public GameObject trailWidthSlider;
    public Slider sliderComp;
    public Slider trailWidths;
    public GameObject openMenuButton;
    public GameObject closeMenuButton;

    public GameObject quark1;
    public GameObject quark2;
    public GameObject quark3;

    Quark quark1comp;
    Quark quark2comp;
    Quark quark3comp;

    public bool menuActive = false;
    public bool openMenuButtonActive = true;
    public bool closeMenuButtonActive = false;
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

        if(menuActive)
            menu.SetActive(true);
        else
            menu.SetActive(false);

        if(openMenuButtonActive){
            openMenuButton.SetActive(true);
            closeMenuButton.SetActive(false);
        }
        else{
            openMenuButton.SetActive(false);
            closeMenuButton.SetActive(true);
        }
    }

    public void MenuShow(){
        menuActive = true;
        closeMenuButtonActive=true;
        openMenuButtonActive = false;
    }
    public void MenuHide(){
        menuActive = false;
        closeMenuButtonActive=false;
        openMenuButtonActive = true;
    }
}
