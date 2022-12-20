using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluxTension : MonoBehaviour
{
    TypeQuark[] quarks;
    [Tooltip("How elastic are flux tubes at pulling quarks together? (A constant)")] public float A = 1;
    public float B = 1;
    public float C = 1;
    public float threshold = 1000;

    GameObject fluxCenter;

    public float scale = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        quarks = gameObject.GetComponentsInChildren<TypeQuark>();
        fluxCenter = GameObject.FindGameObjectWithTag("FluxCenter");
    }

    // Update is called once per frame
    void Update()
    {
        foreach (TypeQuark quark in quarks) {
            Vector3 totalForce = Vector3.zero;//getForce(quark.transform.position, transform.position) * 100;
            totalForce += getForce(quark.transform.position, fluxCenter.transform.position);
            if (float.IsNaN(totalForce.x) || float.IsNaN(totalForce.y) || float.IsNaN(totalForce.z)) //NaN Check
                continue;
            quark.GetComponent<Rigidbody>().AddForce(totalForce, ForceMode.Force); //Apply a force to each quark in the direction of the flux tube center
            //fluxCenter.GetComponent<Rigidbody>().AddForce(-scale * totalForce, ForceMode.Force);
        }
    }

    Vector3 getForce(Vector3 pos1, Vector3 pos2) {
        Vector3 r = pos2 - pos1;
        //F = Ar + Br^Cr
        Vector3 force = ((A * r + (Mathf.Pow(B * r.magnitude, C * r.magnitude) * r.normalized)) * Time.deltaTime);

        if (force.magnitude > threshold) {
            force = force.normalized * threshold;
            Debug.Log("Threshold reached!");
        }

        return force;
    }
}
