using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticField : TypeQuark
{
    [Tooltip("Electric Field = true, Magnetic Field = false")]public bool type = false;
    [Tooltip("Globally affect all charges = true, Locally contained = false")] public bool scope = false;
    [Tooltip("Field Strength")]public float Value = 0;
}
