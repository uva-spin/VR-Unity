using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempBound : MonoBehaviour
{
    [SerializeField]public float strength = 10;
    [SerializeField][Tooltip("Do not have this larger that 1")]public float factor = 0.9f;
    private void Update()
    {
        foreach (TypeQuark q in FindObjectsOfType<TypeQuark>()) {
            if (q.GetComponent<Rigidbody>()) {
                Vector3 direction = -(q.transform.position - transform.position);
                q.GetComponent<Rigidbody>().AddForce(Mathf.Log(direction.magnitude, 1.4f) * direction.normalized * strength * Time.deltaTime);


            }
        }
    }
}
