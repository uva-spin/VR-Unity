using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GluonMovement : MonoBehaviour
{
    public Transform quark1;
    public Transform quark2;
    public Transform quark3;

    public GameObject red;
    public GameObject blue;
    public GameObject green;

    public Transform parent;

    public Transform particleCenter;
    public int gluonSpeed;
    List<Transform> targetList;
    Queue<Transform> targetQueue;

    public Transform targetPoint;

    public Transform gluon1;
    GameObject newPart;

     public enum gluonColor {Red, Green, Blue, AntiRed, AntiGreen, AntiBlue}

    public Color redCol, greenCol, blueCol, antiRedCol, AntiGreenCol, AntiBlueCol;
    void Start()
    {
        gluon1.position = particleCenter.position;
        targetPoint = quark1;
        targetList = new List<Transform>{particleCenter, quark2, particleCenter, quark3, particleCenter};
        targetQueue = new Queue<Transform>(targetList);
        
    }
    void Update()
    {
        if(Vector3.Distance(gluon1.position, targetPoint.position) < 0.05){
            if(targetPoint.CompareTag("Quark"))
            {
                ParticleInfo info = targetPoint.GetComponent<ParticleInfo>();
                switch(info.partColor){
                    case PartColor.Red:
                        newPart = Instantiate(blue, targetPoint.position, targetPoint.rotation, parent);
                        gluon1.GetComponent<Renderer>().material.SetColor("_Emission", Color.red);
                        newPart.transform.SetSiblingIndex(info.partIdx);
                        break;
                    case PartColor.Blue:
                        newPart = Instantiate(green, targetPoint.position, targetPoint.rotation, parent);
                        gluon1.GetComponent<Renderer>().material.SetColor("_Emission", Color.red);
                        newPart.transform.SetSiblingIndex(info.partIdx);
                        break;
                    case PartColor.Green:
                        newPart = Instantiate(red, targetPoint.position, targetPoint.rotation, parent);
                        gluon1.GetComponent<Renderer>().material.SetColor("_Emission", Color.red);
                        newPart.transform.SetSiblingIndex(info.partIdx);
                        break;
                }
                Destroy(targetPoint.gameObject);
                targetPoint = newPart.transform;
            }
            targetQueue.Enqueue(targetPoint);
            targetPoint = targetQueue.Dequeue();
        }

        gluon1.position = Vector3.MoveTowards(gluon1.position, targetPoint.position, Time.deltaTime * gluonSpeed);
    }
}
