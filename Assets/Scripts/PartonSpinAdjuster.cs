///Sets spin values for each quark every update frame
///Note: values can only be spin up or spin down at current moment and only works along z-axis

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartonSpinAdjuster : MonoBehaviour
{
    Vector3 position = Vector3.zero;

    [Tooltip("Threshold for total parton spin error allowed (Larger values are less realistic but usually slower to calculate)")] public float Threshold = 0.1f;
    [Tooltip("Scale multiplier for momentum contribution to total parton spin")] public float Scale = 1;
    [Tooltip("Randomizes quark order when iterating to make spin appear random")] public bool pseudoRandomSpin = true;

    float mostSpin = 0;

    float average = 0;
    int totalFrames = 0;

    Vector3 spinDir = Vector3.up * 0.5f;

    bool spinLinesVis = true;

    [Tooltip("Magnetic Field used for polarization")] [SerializeField] StaticField polarizationField;
    float value = 2000;

    private void Start()
    {
        position = transform.position;
        if (polarizationField) value = polarizationField.Value;
    }

    public void spinVis() {
        spinLinesVis = !spinLinesVis;
    }

    public void SetPolarization(PolarizationAxis direction) {
        switch (direction)
        {
            case PolarizationAxis.X:
                spinDir = Vector3.right * 0.5f;
                break;
            case PolarizationAxis.Y:
                spinDir = Vector3.up * 0.5f;
                break;
            case PolarizationAxis.Z:
                spinDir = Vector3.forward * 0.5f;
                break;
            case PolarizationAxis.NONE:
                spinDir = Vector3.zero;
                break;
            default:
                break;
        }

        if (polarizationField) {
            if (spinDir.magnitude > 0)
            {
                polarizationField.Value = value;
                //polarizationField.transform.LookAt(transform.position + spinDir);
                polarizationField.transform.up = spinDir;
            }
            else
                polarizationField.Value = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        float totalSpin = 0;

        TypeQuark[] quarks = FindObjectsOfType<TypeQuark>();

        if (pseudoRandomSpin)
        {
            for (int i = 0; i < quarks.Length; i++)
            {
                TypeQuark q = quarks[i];
                int o = Mathf.RoundToInt(Random.value * (quarks.Length - 1));
                quarks[i] = quarks[o];
                quarks[o] = q;
            }
        }
        Vector3 angMom = new Vector3();
        Vector3 spinLeft = (spinDir.magnitude == 0) ? Random.onUnitSphere * 0.5f : spinDir; //If not polarized, pick a random spin direction for the proton!
        Vector3 pointTo = spinLeft;
        Vector3[] spins = new Vector3[quarks.Length];

        for (int i = 0; i < quarks.Length; i++) {
            if (quarks[i].GetComponent<Rigidbody>()) {
                Vector3 vel = quarks[i].GetComponent<Rigidbody>().velocity;
                angMom += Vector3.Cross(quarks[i].GetComponent<Rigidbody>().mass * vel, quarks[i].transform.position - position);
            }
        }
        angMom *= Scale;
        spinLeft -= angMom;

        Vector3 euler = transform.eulerAngles;

        for (int i = 0; i < quarks.Length; i++) {
            Vector3 dir = pointTo - spinLeft;
            if (i != quarks.Length - 1 && dir.magnitude < 1f) //If the next spin can take us on or inside of the 1/2 radius sphere, orbit the sphere
            {
                Vector2 ortho = Random.insideUnitCircle.normalized * (Mathf.Sqrt((1f - Mathf.Pow(dir.magnitude, 2)) / 4f));
                transform.LookAt(dir);
                spins[i] = (transform.right * ortho.x + transform.up * ortho.y + dir / 2).normalized * 0.5f;
                //Debug.Log(spins[i]);
                spinLeft += spins[i];

            }
            else //Otherwise go towards the center
            {
                spins[i] = dir.normalized * 0.5f;
                spinLeft += spins[i];
            }
            transform.eulerAngles = euler;
            quarks[i].setVisualLine(VisualLines.SPINLINE, (spinLinesVis) ? spins[i] : Vector3.zero);
        }

        average = (totalFrames * average + Mathf.Abs(totalSpin)) / (totalFrames + 1);
        totalFrames++;

        Debug.Log(angMom.magnitude + " | " + spinLeft + " | " + spinLeft.magnitude);

        //Debug.Log(totalSpin + " spin | " + ups + " ^ " + (quarks.Length - ups) + " v  | " + angMom[most][2] + " " + quarks[most].GetType() + " | " + average + " | " + mostSpin);

        if (Mathf.Abs(totalSpin) > Mathf.Abs(mostSpin)) mostSpin = totalSpin;
    }
}
