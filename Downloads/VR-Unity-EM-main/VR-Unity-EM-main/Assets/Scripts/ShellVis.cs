using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellVis : MonoBehaviour
{
    bool isVisible;
    public GameObject shell;
    // Start is called before the first frame update
    void Start()
    {
        isVisible = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(isVisible){
            shell.SetActive(true);
        }
        else{
            shell.SetActive(false);
        }
    }
    public void ToggleVisibility(){
        isVisible = !isVisible;
    }
}
