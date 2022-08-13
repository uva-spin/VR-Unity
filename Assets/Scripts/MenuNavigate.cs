using System;
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

    void Start()
    {
        const int OPTIONS = 0;
        const int STATS = 1;
        const int SeaGluon = 2;
        menus[OPTIONS].SetActive(true);
        menus[STATS].SetActive(false);
        menus[SeaGluon].SetActive(false);
    }
}
