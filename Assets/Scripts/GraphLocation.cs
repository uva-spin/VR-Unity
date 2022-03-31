using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphLocation : MonoBehaviour
{
    public GameObject graph;
    public GameObject dot;

    void OnCollisionEnter(Collision collision)
    {
        //GameObject point = Instantiate(dot, graph.transform); 
        //point.SetActive(true);
        Debug.Log("quark has collided: " + collision);
    }
}
