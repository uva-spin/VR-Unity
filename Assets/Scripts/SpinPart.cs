using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinPart : MonoBehaviour
{
    float currentRotation;
    public float speed = 5;

    // Update is called once per frame
    void LateUpdate()
    {
        float speedEdit = speed * Time.timeScale;
        currentRotation += speedEdit;
        transform.rotation = Quaternion.Euler(0,currentRotation,0);
        if (currentRotation > 360)
            currentRotation = 0;
    }
}
