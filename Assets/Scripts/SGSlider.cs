using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class SGSlider : MonoBehaviour
{
    public Spawner spawner;
    public TextMeshProUGUI text;
    // Start is called before the first frame update


    void Start()
    {
        string sliderName = gameObject.name;

        switch (sliderName)
        {
            case "Sea Gluon":
                float numOfSG = spawner.gluonsToSpawn;
                text.text = numOfSG.ToString();
                gameObject.GetComponent<Slider>().value = numOfSG / 10000f;
                break;
            case "Momentum":
                float momentum = spawner.seaGluonMovementComponent.momentum;
                text.text = momentum.ToString();
                gameObject.GetComponent<Slider>().value = momentum / 10f;
                break;
            case "Valance Quark Weight":
                float VQW = spawner.seaGluonMovementComponent.valenceQuarkWeight;
                text.text = VQW.ToString();
                gameObject.GetComponent<Slider>().value = VQW / 5f;
                break;
            default:
                break;
        }

        
        

    }
     

    public void UpdateNumOfSG()
    {
        int numOfSG = (int)Math.Round(gameObject.GetComponent<Slider>().value * 10000f);
        spawner.changeSetting(numOfSG);
        text.text = numOfSG.ToString();
    }

    public void UpdateMomentum()
    {
        int momentum = (int)Math.Round(gameObject.GetComponent<Slider>().value * 10f);
        spawner.seaGluonMovementComponent.valenceQuarkWeight = momentum;
        spawner.changeSetting(-1);
        text.text = momentum.ToString();
    }

    public void UpdateVQW()
    {
        int VQW = (int)Math.Round(gameObject.GetComponent<Slider>().value * 5f);
        spawner.seaGluonMovementComponent.valenceQuarkWeight = VQW;
        spawner.changeSetting(-1);
        text.text = VQW.ToString();
    }

}
