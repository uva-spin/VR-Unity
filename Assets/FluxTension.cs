using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluxTension : MonoBehaviour
{
    TypeQuark[] quarks;
    [Tooltip("How elastic are flux tubes at pulling quarks together? (A constant)")] public float A = 1;
    public float B = 1;
    public float C = 1;

    // Start is called before the first frame update
    void Start()
    {
        quarks = gameObject.GetComponentsInChildren<TypeQuark>();
    }

    // Update is called once per frame
    void Update()
    {
        foreach (TypeQuark quark in quarks) {
            Vector3 totalForce = Vector3.zero;//getForce(quark.transform.position, transform.position) * 100;
            foreach (TypeQuark quark2 in quarks)
            {
                if (quark == quark2)
                    continue;
                totalForce += getForce(quark.transform.position, quark2.transform.position);
            }
            quark.GetComponent<Rigidbody>().AddForce(totalForce, ForceMode.Force); //Apply a force to each quark in the direction of the flux tube center
        }
    }

    Vector3 getForce(Vector3 pos1, Vector3 pos2) {
        Vector3 r = pos2 - pos1;
        //F = Ar + Br^Cr
        Vector3 force = ((A * r + (Mathf.Pow(B * r.magnitude, C * r.magnitude) * r.normalized)) * Time.deltaTime);

        return force;
    }
}
