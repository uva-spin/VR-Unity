using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuNavigate : MonoBehaviour
{
    public GameObject[] menus;

    public void ChangeMenu(int tab)
    {
        for (int i = 0; i < menus.Length; i++)
            menus[i].SetActive(i == tab);
    }
    
    /*
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    */
}
