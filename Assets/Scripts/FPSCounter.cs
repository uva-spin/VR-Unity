using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    // Start is called before the first frame update

    float time = 0f;

    void Update() {
        time -= Time.timeScale;
        if (time <= 0)
        {
            GetComponent<TMP_Text>().text = "FPS: " + Mathf.RoundToInt(Time.timeScale / Time.deltaTime);
            time = 10f;
        }
    }
}
