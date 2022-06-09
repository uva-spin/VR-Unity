using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphLocation : MonoBehaviour
{
    public GameObject graph, dot, measurePoint;

    // manually set position of game object since unity's and won't let us calc velocity otherwise
    private void Update()
    {
        measurePoint.transform.position = new Vector3(0, 0, 0);
    }

    /*
     * plot a point on the graph relative to where the proton intersects with
     * the measurePoint. eventually, changing the size and color of the dot
     * depending on it's impact velocity, and the size of the dot depending
     * on the location distribution (this will be a separate graph)
     *
     * Prof. Keller also mentioned that being able to export this data would
     * be a nice-to-have
     */
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("quark relative velocity: " + collision.relativeVelocity);
        Debug.Log("quark contact: " + collision.GetContact(0).point);

        Vector3 graphPoint = collision.GetContact(0).point;
        GameObject point = Instantiate(dot, graphPoint, new Quaternion(), graph.transform);
        point.SetActive(true);
    }
}
