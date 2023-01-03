using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ParticleCenterScript : MonoBehaviour
{
    Transform parent;
    List<Transform> partList;
    public int particleCount = 3;
    public int stretchScale = 100;
    // Start is called before the first frame update
    void Start()
    {
        parent = transform.parent;
        partList = new List<Transform>();
        for(int i = 0 ; i < particleCount; i++){
            partList.Add(parent.GetChild(i));
        }
        
        
    }

    // Update is called once per frame
    void Update(){
        Vector3 sum = new Vector3();
        foreach(Transform part in partList){
            sum += part.position;
        }
        transform.position = sum/particleCount;
        ConnectLines();
    }
    void LateUpdate(){
        partList = new List<Transform>();
        for(int i = 0 ; i < particleCount; i++){
            partList.Add(parent.GetChild(i));
        }
    }

    //stretch and use gluons to connect quarks to the center
    void ConnectLines(){
        float stretchFactor = 0;
        for(int i = 0; i < particleCount; i++){
            Transform currChild = transform.GetChild(i);
            currChild.rotation = Quaternion.FromToRotation(Vector3.down, transform.position - partList[i].position);
            Vector3 length = transform.position - partList[i].position;
            currChild.localScale = new Vector3(transform.localScale.x, length.magnitude*2, transform.localScale.z);
            stretchFactor += length.magnitude;
            
            Transform currChildMesh = currChild.GetChild(0);
            currChildMesh.gameObject.GetComponent<Renderer>().material.SetVector("Tiling Scale", new Vector4(0.5f, 1.5f /( length.magnitude * 2), 0, 0));

        }
        stretchFactor = ((1/(stretchFactor/stretchScale)) -.5f);
        //varying width
        for(int i = 0; i < particleCount; i++){
            Transform currChild = transform.GetChild(i);
            currChild.localScale = new Vector3(stretchFactor,
                                               currChild.localScale.y,
                                               stretchFactor);
            
            
        }
        
    }
}
