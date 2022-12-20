using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempBound : MonoBehaviour
{
    [SerializeField]public float strength = 10;
    public float logBase = 1.4f;
    GameObject fluxCenter;
    private void Start()
    {
        fluxCenter = GameObject.FindGameObjectWithTag("FluxCenter");
    }
    private void Update()
    {
        Vector3 direction = -(fluxCenter.transform.position - transform.position);
        fluxCenter.GetComponent<Rigidbody>().AddForce(Mathf.Log(direction.magnitude, logBase) * direction.normalized * strength * Time.deltaTime);
    }
}
