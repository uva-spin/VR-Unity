using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
//Fancy way of saying "hey, if this is a built application and not the editor, don't use this"
#if (UNITY_EDITOR && !UNITY_ANDROID)
using UnityEditor;
#endif

public class CartesianModel : MonoBehaviour
{
    public int n = 3;
    public int dimensions = 3;
    public float R = 0;

    public float[] m = new float[3] { 2, 1, 1 };
    public float Mc = 1e-05f,
    k = 5;

    //Values
    public Vector3[] x = new Vector3[4] { new Vector3(0.5f, 0, 0), new Vector3(-0.25f, 0.4330127f, 0), new Vector3(-0.25f, -0.4330127f, 0), new Vector3(0, 0, 0) }, 
        x_dot = new Vector3[4] { new Vector3(2, -2, 0), new Vector3(0, 4, -4), new Vector3(-4, 0, 4), new Vector3(0, 0, 0) }, 
        x_dot_dot = new Vector3[4] { new Vector3(), new Vector3(), new Vector3(), new Vector3() }, 
        omega = new Vector3[3] { new Vector3(), new Vector3(), new Vector3() }, 
        omega_dot = new Vector3[3] { new Vector3(), new Vector3(), new Vector3() }, 
        A = new Vector3[4] { new Vector3(0.5f, 0, -0.5f), new Vector3(-1, 1, 0), new Vector3(0, -1, 1), new Vector3(0, 0, 0) };
    public float chaosFactor = 10, chaosShiftChance = 0.25f;
    public float Ra;

    public float dt = 0.001f;

    public float playbackSpeed = 1f;


    public GameObject[] displays;
    public Transform centerOfMass; //This is for display purposes only, modifying this will NOT change the actual center of mass. Modify the quarks instead

    public GameObject Point;

    private Vector3 rotateProton = new Vector3();
    private float randomRotation = 250;

    //Initial values used for resetting without relaunching application
    private Vector3[] _x, _x_dot, _x_dot_dot, _omega, _omega_dot, _A;

    //Default values
    private Vector3[] __x, __x_dot, __x_dot_dot, __omega, __omega_dot, __A;
    private float[] __m;
    private float __Mc, __R, __k, __randomRotation;
    private int __n, __dimensions;

    private StreamWriter fileOutput;
    private int lastSecond = -1;

    public bool writeOutputToFile = false;

    private void Start()
    {
        if (!centerOfMass) centerOfMass = Instantiate(new GameObject()).transform;
        //FindObjectOfType<Pather>().active = true;
        __x = (Vector3[])(_x = x).Clone(); __x_dot = (Vector3[])(_x_dot = x_dot).Clone(); __x_dot_dot = (Vector3[])(_x_dot_dot = x_dot_dot).Clone();
        __omega = (Vector3[])(_omega = omega).Clone(); __omega_dot = (Vector3[])(_omega_dot = omega_dot).Clone(); __A = (Vector3[])(_A = A).Clone();

        __m = (float[])m.Clone(); __Mc = Mc; __n = n; __dimensions = dimensions; __k = k; __R = R; __randomRotation = randomRotation;

        if (writeOutputToFile)
        {
            fileOutput = new StreamWriter(Path.Combine(Application.persistentDataPath, "output.csv"));
            fileOutput.WriteLine("Time (s), x1, y1, z1, x2, y2, z2, x3, y3, z3, xc, yx, zc, x_dot1, y_dot1, z_dot1, x_dot2, y_dot2, z_dot2, x_dot3, y_dot3, z_dot3, x_dotc, y_dotx, z_dotc");
            HandleFileWriting(0);
        }
    }

    void Update()
    {
        //Just a fancy way of asking Runge-Kutta to use the Lagrangian function down below to calculate a bunch of different variables
        System.Func<float, float[], float[]> pendulumFunc = (t, variables) => Lagrangian(t, variables);
        float[][] output = RungeKutta.rk4(pendulumFunc, 0, Time.deltaTime * playbackSpeed, dt,
            x[0][0], x[0][1], x[0][2], x[1][0], x[1][1], x[1][2], x[2][0], x[2][1], x[2][2], x[3][0], x[3][1], x[3][2],
            x_dot[0][0], x_dot[0][1], x_dot[0][2], x_dot[1][0], x_dot[1][1], x_dot[1][2], x_dot[2][0], x_dot[2][1], x_dot[2][2], x_dot[3][0], x_dot[3][1], x_dot[3][2],
            x_dot_dot[0][0], x_dot_dot[0][1], x_dot_dot[0][2], x_dot_dot[1][0], x_dot_dot[1][1], x_dot_dot[1][2],
            x_dot_dot[2][0], x_dot_dot[2][1], x_dot_dot[2][2], x_dot_dot[3][0], x_dot_dot[3][1], x_dot_dot[3][2],
            omega[0][0], omega[0][1], omega[0][2], omega[1][0], omega[1][1], omega[1][2], omega[2][0], omega[2][1], omega[2][2],
            omega_dot[0][0], omega_dot[0][1], omega_dot[0][2], omega_dot[1][0], omega_dot[1][1], omega_dot[1][2], omega_dot[2][0], omega_dot[2][1], omega_dot[2][2]
            );

        int timestamp = 0;


        //Runge Kutta will actually perform multiple calculations between frames to make the output more accurate,
        //so you'll need to reassign each of the transforms and variables by doing something like this.
        //There's probably a more efficient way of doing this, but this way works
        while (timestamp < output.Length)
        {
            float[] frame = output[timestamp];
            assignVariables(frame);
            transform.position = x[3];

            //Debug.Log(x[3]);
            //Debug.Log(x[0]);

            float time = Time.realtimeSinceStartup;

            for (int i = 0; i < n; i++)
            {
                displays[i].transform.localPosition = x[i];
                //displays[i].transform.eulerAngles = omega[i];

                //displays[i].transform.Rotate(Mathf.Cos(time), Mathf.Sin(time), 0, Space.Self);
                if (Point)
                {
                    GameObject p = Instantiate(Point, displays[i].transform.position, Quaternion.identity);
                    p.GetComponent<Renderer>().material = displays[i].GetComponent<Renderer>().material;
                    Destroy(p, 1.5f);
                }
            }


            timestamp++;
        }


        Vector3 ang = new Vector3();
        Vector3 vel = new Vector3();


        //This is the yellow dot thing you see. It is only for display purposes only and has no effect on the simulation.
        //You could literally get rid of these lines and nothing would happen other than the yellow dot won't move anymore
        centerOfMass.position = Vector3.zero;
        for (int i = 0; i < n; i++) {
            GameObject q = displays[i];
            centerOfMass.position += q.transform.position * m[i];
            vel += x_dot[i];
        }
        centerOfMass.position += transform.position * Mc;

        float totalM = 0;
        for (int i = 0; i < n; i++) totalM += m[i];

        centerOfMass.transform.position /= ((totalM + Mc));
        vel /= 3;

        for (int i = 0; i < n; i++) {
            ang += Vector3.Cross(x_dot[i] - vel, x[i] - centerOfMass.position);
        }

        if (Random.Range(0f, 1f) <= chaosShiftChance) ChangeRandomForce();

        if (Random.Range(0f, 1f) <= .01) rotateProton = RandomForce() * randomRotation;

        transform.parent.Rotate(rotateProton * Time.deltaTime);

        HandleFileWriting(Time.realtimeSinceStartup);
    }

    #region Lagrangian and Main Calculations

    //Makes it so instead of having to refer to var[1224234325453] or something like that when inputting variables into the Lagrangian, you can just use x_dot.
    //You technically don't need this, but you probably want this
    private void assignVariables(float[] var)
    {
        x = new Vector3[4]; for (int i = 0; i < 4; i++) { x[i] = new Vector3(var[3 * i], var[3 * i + 1], var[3 * i + 2]); }
        x_dot = new Vector3[4]; for (int i = 0; i < 4; i++) { x_dot[i] = new Vector3(var[3 * i + 12], var[3 * i + 13], var[3 * i + 14]); }
        omega = new Vector3[3]; for (int i = 0; i < 3; i++) { omega[i] = new Vector3(var[3 * i + 36], var[3 * i + 37], var[3 * i + 38]); }
    }

    private float[] Lagrangian(float t, float[] var) //Code implementation of 3.4 of the Overleaf document https://www.overleaf.com/project/63c837cec211a05c6d7a55f5
    {
        float[] output = new float[var.Length];

        assignVariables(var);

        //quarks (x double dot i j)
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < dimensions; j++) {
                x_dot_dot[i][j] = -(k / m[i]) * ((R * (x[3][j] - x[i][j]) / Mathf.Sqrt(Sum(i, j))) - (x[3][j] - x[i][j])) + calculateMagnetics();
                omega_dot[i][j] = Mathf.Sqrt(5 * A[i][j] / (2 * m[i] * Ra * Ra));
            }
        }

        //spring center (x double dot c j)
        for (int j = 0; j < dimensions; j++)
        {
            x_dot_dot[3][j] = 0;
            for (int i = 0; i < n; i++) x_dot_dot[3][j] += (k / Mc) * ((2 * R * (x[3][j] - x[i][j]) / Mathf.Sqrt(Sum(i, j))) - (x[3][j] - x[i][j]));
        }

        //Randomness
        for (int i = 0; i < n + 1; i++) {
            x_dot_dot[i] += A[i] * chaosFactor;
        }


        for (int j = 0; j < dimensions; j++) { for (int i = 0; i < n; i++) { output[3 * i + j] = x_dot[i][j]; } output[9 + j] = x_dot[3][j]; }
        for (int j = 0; j < dimensions; j++) { for (int i = 0; i < n; i++) { output[12 + 3 * i + j] = x_dot_dot[i][j]; } output[12 + 9 + j] = x_dot_dot[3][j]; }
        for (int j = 0; j < dimensions; j++) { for (int i = 0; i < n; i++) { output[36 + 3 * i + j] = omega_dot[i][j]; } }

        return output;
    }

    private float calculateMagnetics()
    {
        //TODO: re-impliment magnetic force

        return 0;
    }

    //This is the sumnation that every equation seems to use
    private float Sum(int i, int J) {
        float output = 0;
        for (int j = J; j < dimensions; j++) {
            output += Mathf.Pow(x[3][j] - x[i][j], 2);
        }
        return (output == 0) ? 0.00001f : output; //If the sum would return zero, return not zero (otherwise everything crashes due to a division by zero)
    }

#endregion

    public void ResetState() {
        x = _x; x_dot = _x_dot; x_dot_dot = _x_dot_dot;
        omega = _omega; omega_dot = _omega_dot; A = _A;

        transform.parent.rotation = Quaternion.identity;
        rotateProton = new Vector3();

        FindObjectOfType<SeaQuarkSpawner>().ResetAllParticles();
    }

    #region Applying Random Force

    private void ChangeRandomForce() {
        int counterForceIndex = Random.Range(0, dimensions);
        float mult = Random.Range(-1f, 1f);
        int a = (counterForceIndex + 1) % dimensions,
            b = (counterForceIndex + 2) % dimensions;
        A[a] = RandomForce() / x_dot[a].magnitude;
        A[b] = RandomForce() / x_dot[b].magnitude;
        A[counterForceIndex] = -(A[a] * m[a] + A[b] * m[b]) / m[counterForceIndex];

        A[0] *= mult;
        A[1] *= mult;
        A[2] *= mult;
    }

    private Vector3 RandomForce() {
        return new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
    }

    #endregion

    #region Setting New Initial Values In-Game

    private Vector3[] ProcessValues(float?[] newVals, params Vector3[] oldVals) {
        for (int i = 0; i < newVals.Length; i++) {
            if (newVals[i] != null) oldVals[i/3][i%3] = newVals[i].Value;
        }
        return oldVals;
    }


    public void SetNewInitialAndRestart(float?[] newVals, ValueType valToModify)
    {
        switch (valToModify)
        {
            case ValueType.POSITION:
                _x = ProcessValues(newVals, _x);
                break;
            case ValueType.VELOCITY:
                _x_dot = ProcessValues(newVals, _x_dot);
                break;
            case ValueType.MISC:
                Vector3[] newerVals = ProcessValues(newVals, new Vector3(m[0], m[1], m[2]), new Vector3(R, k, Mc), new Vector3(n, dimensions, randomRotation));
                m = new float[] { newerVals[0][0], newerVals[0][1], newerVals[0][2] };
                R = newerVals[1][0]; k = newerVals[1][1]; Mc = newerVals[1][2];
                n = Mathf.FloorToInt(newerVals[2][0]);
                dimensions = Mathf.FloorToInt(newerVals[2][1]);
                randomRotation = newerVals[2][2];
                break;
        }

        ResetState();
    }
    public Vector3[] ResetToDefaultAndRestart(ValueType valToReset)
    {
        Vector3[] ret = null;

        switch (valToReset)
        {
            case ValueType.POSITION:
                _x = (Vector3[])__x.Clone();
                ret = _x;
                break;
            case ValueType.VELOCITY:
                _x_dot = (Vector3[])__x_dot.Clone();
                ret = _x_dot;
                break;
            case ValueType.MISC:
                m = (float[])__m.Clone();
                R = __R; k = __k; Mc = __Mc;
                n = __n; dimensions = __dimensions; randomRotation = __randomRotation;
                ret = new Vector3[] { new Vector3(m[0], m[1], m[2]), new Vector3(R, k, Mc), new Vector3(n, dimensions, randomRotation) };
                break;
        }

        ResetState();

        return (Vector3[])ret.Clone();
    }


    public enum ValueType
    {
        POSITION, VELOCITY, MISC
    }

    #endregion

    #region Outputting calculations to file for inspection

    private string GetVectorAsStringFromIndex(Vector3[] array, int index) {
        Vector3 value = array[index];
        return $"{value.x}, {value.y}, {value.z}";
    }

    private void HandleFileWriting(float time) {
        if (fileOutput == null) return;
        if (Mathf.Floor(time) > lastSecond) {
            string text = 
                $"{Mathf.FloorToInt(time)}, {GetVectorAsStringFromIndex(x, 0)}, {GetVectorAsStringFromIndex(x, 1)}, {GetVectorAsStringFromIndex(x, 2)}, {GetVectorAsStringFromIndex(x, 3)}"
                + $", {GetVectorAsStringFromIndex(x_dot, 0)}, {GetVectorAsStringFromIndex(x_dot, 1)}, {GetVectorAsStringFromIndex(x_dot, 2)}, {GetVectorAsStringFromIndex(x_dot, 3)}";
            fileOutput.WriteLine(text);
            lastSecond = Mathf.FloorToInt(time);
        }
        if (time >= 10)
        {
            fileOutput.Close();
            Application.Quit();
        }
    }

    #endregion
}

#region Custom Editor (Not used by client, devs only)

#if (UNITY_EDITOR && !UNITY_ANDROID)
[CustomEditor(typeof(CartesianModel))]
public class SomeScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CartesianModel cartesianModel = (CartesianModel)target;
        if (GUILayout.Button("Reset Proton"))
        {
            cartesianModel.ResetState();
        }
        EditorGUILayout.HelpBox("This is an implementation of the latest cartesion model in the overleaf document (plus some added randomness).\n" +
                                "N stands for the number of quarks you want active (1-3).\n" +
                                "Dimensions is the number of spatial dimensions you want active (1D, 2D, or 3D).\n" +
                                "M and Mc are the masses of each quark and the flux tube's center.\n" +
                                "K is the spring factor (how strong the strong force is).\n" +
                                "X and X_dot are used for the positions and velocities of each quark and the flux tube.\n" +
                                "A is the random force direction, and the chaos factor is how strong A is applied.\n" +
                                "Omega, omega_dot, and Ra are all currently unused.\n" +
                                "Dt is the level of precision each calculation is done by, and playback speed is how fast the simulation is run.", MessageType.Info);
        DrawDefaultInspector();
    }
}
#endif

#endregion