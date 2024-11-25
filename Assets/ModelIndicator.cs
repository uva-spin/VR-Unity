using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ModelIndicator : MonoBehaviour
{
    public TextMeshProUGUI text;
    public GameObject csv;
    public GameObject cartesian;

    private MonoBehaviour cartesianScript;


    // Start is called before the first frame update
    void Start()
    {
        cartesianScript = cartesian.GetComponent<CartesianModel>();
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
    }
}
