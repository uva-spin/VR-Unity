///Sets spin values for each quark every update frame
///Note: values can only be spin up or spin down at current moment and only works along z-axis

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PartonSpinAdjuster : MonoBehaviour
{
    Vector3 position = Vector3.zero;

    [Tooltip("Threshold for total parton spin error allowed (Larger values are less realistic but usually slower to calculate)")] public float Threshold = 0.1f;
    [Tooltip("Scale multiplier for momentum contribution to total parton spin")] public float Scale = 1;
<<<<<<< HEAD
    [Tooltip("Randomizes quark order when iterating to make spin appear random")] bool pseudoRandomSpin = true;

    [Tooltip("Use the old parton spin sum rule?")] bool oldSystem = false;
=======
    [Tooltip("Randomizes quark order when iterating to make spin appear random")] public bool pseudoRandomSpin = true;
>>>>>>> f1ffde9f13d571f47d36e720280d6dcc50271ec8

    float mostSpin = 0;

    float average = 0;
    int totalFrames = 0;

    Vector3 spinDir = Vector3.up * 0.5f;

    bool spinLinesVis = true;

    [Tooltip("Magnetic Field used for polarization")] [SerializeField] StaticField polarizationField;
    float value = 2000;

<<<<<<< HEAD
    List<GameObject> pairs = new List<GameObject>();
    List<Vector3> values = new List<Vector3>();

    Vector3 totalSpin = Vector3.zero;

    private void Awake()
    {
        position = transform.position;
        if (polarizationField) value = polarizationField.Value;

        addPair(FindObjectOfType<AllQuarkScript>().gameObject, Vector3.forward * 0.5f);
=======
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
>>>>>>> f1ffde9f13d571f47d36e720280d6dcc50271ec8
    }

    public void addPair(GameObject obj, Vector3 value) {
        pairs.Add(obj); values.Add(value);
        totalSpin += value;
        values = SnapVectors(values, spinDir - totalSpin);
        totalSpin = spinDir;
    }
    public void removePair(GameObject obj) {
        int i;
        for (i = 0; i < pairs.Count; i++) {
            if (pairs[i] == obj) break;
        }
        pairs.RemoveAt(i);
        Vector3 shift = values[i]; values.RemoveAt(i);
        values = SnapVectors(values, shift);
    }
    List<Vector3> SnapVectors(List<Vector3> vectorParts, Vector3 shift) {
        if (values.Count < 1) return null;
        Vector3 tweakAmount = shift / vectorParts.Count;
        for (int i = 0; i < vectorParts.Count; i++) vectorParts[i] += tweakAmount;
        return vectorParts;
    }

    public void spinVis() {
        spinLinesVis = !spinLinesVis;
    }

    public void SetPolarization(PolarizationAxis direction) {
        Vector3 spinOld = spinDir;
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

        if (spinDir != spinOld) values = SnapVectors(values, spinDir - spinOld);

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

    //1: Figure out the total spin + ang momentum each pair currently has
    //2: Subtract that value from the total value the pair should have to get the shift
    //3: Add shift/[# of quarks in pair/triad] to each spin to get to preferred value
    //4: Normalize spins to 1/2
    //5: Repeat steps 1 and 2
    //6: Repeat step 3 but for angular momentums instead of spins

    Vector3[] spinValues = new Vector3[3];
    Vector3[] momentums = new Vector3[3];
    Vector3 calculatedSpin = Vector3.zero;
    Vector3 totalShift = Vector3.zero;

    List<Vector3> allmomentums = new List<Vector3>();
    List<Vector3> oldmomentums = new List<Vector3>();

    public Transform[] aligners = new Transform[3];

    // Update is called once per frame
    void Update()
    {
        if (oldSystem) { Old(); return; }

        for (int i = 0; i < pairs.Count; i++) {
            allmomentums.Clear();
            oldmomentums.Clear();

            TypeQuark[] quarkPairs = pairs[i].GetComponentsInChildren<TypeQuark>();
            totalShift = values[i] - getTotalSpin(quarkPairs, false); //1 and 2

            spinValues = SnapVectors(spinValues.ToList(), totalShift).ToArray(); //3
            for (int j = 0; j < quarkPairs.Length; j++) {
                quarkPairs[j].spin = spinValues[j].normalized * 0.5f; //4
            }
            totalShift = values[i] - getTotalSpin(quarkPairs, true); //5

            allmomentums = SnapVectors(allmomentums, totalShift/Scale); //6

            for (int j = 0; j < allmomentums.Count; j++) { //Re-orient cross product direction
                aligners[0].LookAt(oldmomentums[j]);
                aligners[1].LookAt(quarkPairs[j].GetComponent<Rigidbody>().velocity, aligners[0].transform.up);
                aligners[2].LookAt(transform.position - quarkPairs[j].transform.position, aligners[0].transform.up);
                aligners[0].LookAt(allmomentums[j], aligners[0].transform.up);

                quarkPairs[j].GetComponent<Rigidbody>().velocity = aligners[1].transform.forward;
                quarkPairs[j].transform.position = transform.position - aligners[2].forward;

                float magnitude = (oldmomentums[j].magnitude > 0) ? allmomentums[j].magnitude / oldmomentums[j].magnitude : 0;
                quarkPairs[j].GetComponent<Rigidbody>().velocity *= magnitude;
            }
        }
    }

    Vector3 getTotalSpin(TypeQuark[] quarkPairs, bool momentumsCalculated) {
        calculatedSpin = Vector3.zero;
        for (int j = 0; j < quarkPairs.Length; j++) { //1
            if (!momentumsCalculated) {
                momentums[j] = Vector3.Cross(quarkPairs[j].GetComponent<Rigidbody>().mass * quarkPairs[j].GetComponent<Rigidbody>().velocity, quarkPairs[j].transform.position - position);
                allmomentums.Add(momentums[j]);
                oldmomentums.Add(momentums[j]);
            }
            calculatedSpin += momentums[j];
        }
        calculatedSpin *= Scale;
        for (int j = 0; j < quarkPairs.Length; j++) {
            spinValues[j] = quarkPairs[j].spin;
            calculatedSpin += quarkPairs[j].spin;
        }
        return calculatedSpin; //2
    }

    void Old() { 
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
