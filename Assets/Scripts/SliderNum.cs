using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderNum : MonoBehaviour
{
    public Slider slider;
    public Quark quark;  // Reference to the Quark script

    // Start is called before the first frame update
    void Start()
    {
        slider = transform.parent.GetComponent<Slider>();

        // Check if there's a Quark component on the same GameObject or elsewhere
        if (quark == null)
        {
            quark = FindObjectOfType<Quark>(); // Alternatively, assign manually in the Inspector
        }

        // Add a listener to call UpdateParticleEmission whenever the slider value changes
        slider.onValueChanged.AddListener(UpdateQuarkEmission);
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<TextMeshProUGUI>().text = slider.value.ToString("F2");
    }

    // Custom method to update particle emission in Quark script
    private void UpdateQuarkEmission(float value)
    {
        if (quark != null)
        {
            quark.UpdateParticleEmission(value);
        }
    }
}