using System.Collections.Generic;
using UnityEngine;
using TMPro; // Required for TextMeshPro
using System.IO;




public class SaveSystem : MonoBehaviour
{

    public GameObject MenuScriptHolder;
    private MonoBehaviour MenuScript;

    private void Start()
    {
        MenuScript = MenuScriptHolder.GetComponent<MenuScript>();

    }



}
