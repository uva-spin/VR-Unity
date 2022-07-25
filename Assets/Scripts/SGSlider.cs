using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class SGSlider : MonoBehaviour
{
    public Spawner spawner;
    public TextMeshProUGUI SGText;
    // Start is called before the first frame update


    void Start()
    {
        float numOfSG = spawner.gluonsToSpawn / 10000f;
        gameObject.GetComponent<Slider>().value = numOfSG;
        SGText.text = numOfSG.ToString();
        Debug.Log(numOfSG.ToString());

    }

    public void UpdateNumOfSG()
    {
        int numOfSG = (int)Math.Round(gameObject.GetComponent<Slider>().value * 10000f);
        spawner.changeSGQuantity(numOfSG);
        SGText.text = numOfSG.ToString();
        Debug.Log(numOfSG);
    }

}
