using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinTest : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] float torqueForce;
    [SerializeField] bool impulse;
    Rigidbody rigidbody;
    Vector3 torque;
    void Start()
    {
        rigidbody = this.gameObject.GetComponent<Rigidbody>();
        torque = new Vector3(0, 0, torqueForce);
        if (impulse)
        {
            torque = new Vector3(0, 0, torqueForce);
            rigidbody.AddTorque(torque);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!impulse)
        {
            torque = new Vector3(0, 0, torqueForce);
            rigidbody.AddTorque(torque);
            Debug.Log("Torque: " + torque);
        }
        
    }

}
