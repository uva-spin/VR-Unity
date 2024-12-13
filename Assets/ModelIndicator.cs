using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ModelIndicator : MonoBehaviour
{
    public TextMeshProUGUI text;
    public GameObject csv;
    public GameObject cartesian;

    //this is where old model is
    public GameObject menu;
    private MenuScript menuScript;


    private MonoBehaviour cartesianScript;


    // Start is called before the first frame update
    void Start()
    {
        cartesianScript = cartesian.GetComponent<CartesianModel>();
        menuScript = menu.GetComponent<MenuScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if (csv.activeInHierarchy)
        {
            text.text = "CSV";
        }
        else if (cartesianScript.enabled) 
        {
            text.text = "Cartesian";
        }
        else if (menuScript.getPolarized())
        {
            text.text = "Polarized(Old Model)";
        }
    }
}
