using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("UpdateCounter", 0f, 0.1f);
    }

    void UpdateCounter() {
        GetComponent<TMP_Text>().text = "FPS: " + Mathf.RoundToInt(Time.timeScale / Time.deltaTime);
    }
}
