using UnityEngine;
using System;

[DisallowMultipleComponent]
public class UniqueID : MonoBehaviour
{
    [SerializeField, HideInInspector] // Makes the field serialized but hidden in the Inspector
    private string uniqueID;

    public string ID => uniqueID;

    private void Awake()
    {
        if (string.IsNullOrEmpty(uniqueID))
        {
            uniqueID = Guid.NewGuid().ToString();
            Debug.Log($"Generated new unique ID for {gameObject.name}: {uniqueID}");
        }
        else
        {
            Debug.Log($"Existing unique ID for {gameObject.name}: {uniqueID}");
        }
    }

}

