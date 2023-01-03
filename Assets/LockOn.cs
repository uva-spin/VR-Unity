using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockOn : MonoBehaviour
{
    public Transform interest;
    Vector3 offset;
    [Tooltip("How snappy is the camera?")]public float speed = 20;

    private void Start()
    {
        if (interest) offset = transform.position - interest.position;
    }

    void Update()
    {
        if (interest) transform.position = Vector3.Lerp(transform.position, interest.position + offset, Time.fixedDeltaTime * speed);
    }
}
