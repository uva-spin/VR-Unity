using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitialVel : MonoBehaviour
{
    public Vector3 force;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Rigidbody>().AddForce(force, ForceMode.Impulse);
    }
}
