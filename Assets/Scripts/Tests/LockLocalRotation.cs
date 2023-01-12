using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockLocalRotation : MonoBehaviour
{
    // Start is called before the first frame update
    float x, y, z;
    void Start()
    {
        x = transform.rotation.x;
        y = transform.rotation.y;
        z = transform.rotation.z;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(x, y, z);
    }
}
