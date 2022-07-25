using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuarkSelfRotate : MonoBehaviour
{
    public bool isPolarized;

    private bool isUpQuark;

    void Start()
    {
        isPolarized = GameObject.Find("Quarks").GetComponent<AllQuarkScript>().polarized;
        if (gameObject.name.Contains("Up"))
        {
            isUpQuark = true;
        }
        else
        {
            isUpQuark = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        int polarizationAxis = GameObject.Find("Quarks").GetComponent<AllQuarkScript>().polarizationAxis;
        isPolarized = GameObject.Find("Quarks").GetComponent<AllQuarkScript>().polarized;
        
        if (!isPolarized)
        {
            Quaternion rotate = gameObject.GetComponent<ChangeOrbit>().SelfRotation;
            gameObject.transform.Rotate(rotate.eulerAngles.x, rotate.eulerAngles.y + +Random.Range(0f, 10f), rotate.eulerAngles.z + Random.Range(0f, 10f), Space.Self);
        }
        else
        {
            if (polarizationAxis == 0)
            {
                if (isUpQuark)
                {
                    gameObject.transform.Rotate(-5f, 0f, 0f, Space.Self);
                }
                else
                {
                    gameObject.transform.Rotate(5f, 0f, 0f, Space.Self);
                }
            }else if (polarizationAxis == 1)
            {
                if (isUpQuark)
                {
                    gameObject.transform.Rotate(0f, -5f, 0f, Space.Self);
                }
                else
                {
                    gameObject.transform.Rotate(0f, 5f, 0f, Space.Self);
                }
            }
            else if(polarizationAxis == 2)
            {
                if (isUpQuark)
                {
                    gameObject.transform.Rotate(0f, 0f, -5f, Space.Self);
                }
                else
                {
                    gameObject.transform.Rotate(0f, 0f, 5f, Space.Self);
                }
            }
        }
    }

}
