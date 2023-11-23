using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CartesianModel : MonoBehaviour
{
    public int n = 2;
    public int dimensions = 2;
    public float R = 5;

    public float[] m = new float[3] { 1, 1, 2 };
    public float Mc = 0.1f,
    k = 5;

    public Vector3[] x, x_dot, x_dot_dot, omega, omega_dot, A;
    public float Ra;

    public float dt;

    public float playbackSpeed = 0.01f;


    public GameObject[] displays;
    public Transform centerOfMass; //This is for display purposes only, modifying this will NOT change the actual center of mass. Modify the quarks instead

    public GameObject Point;

    private void Start()
    {
        if (!centerOfMass) centerOfMass = Instantiate(new GameObject()).transform;
        //FindObjectOfType<Pather>().active = true;
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
                displays[i].transform.position = x[i];
                displays[i].transform.eulerAngles = omega[i];

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

        Debug.Log(ang.magnitude);
    }

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
                x_dot_dot[i][j] = -(k / m[i]) * ((R * (x[3][j] - x[i][j]) / Mathf.Sqrt(Sum(i, j))) - (x[3][j] - x[i][j]));
                omega_dot[i][j] = Mathf.Sqrt(5 * A[i][j] / (2 * m[i] * Ra * Ra));
            }
        }

        //spring center (x double dot c j)
        for (int j = 0; j < dimensions; j++)
        {
            x_dot_dot[3][j] = 0;
            for (int i = 0; i < n; i++) x_dot_dot[3][j] += (k / Mc) * ((2 * R * (x[3][j] - x[i][j]) / Mathf.Sqrt(Sum(i, j))) - (x[3][j] - x[i][j]));
        }

        for (int j = 0; j < dimensions; j++) { for (int i = 0; i < n; i++) { output[3 * i + j] = x_dot[i][j]; } output[9 + j] = x_dot[3][j]; }
        for (int j = 0; j < dimensions; j++) { for (int i = 0; i < n; i++) { output[12 + 3 * i + j] = x_dot_dot[i][j]; } output[12 + 9 + j] = x_dot_dot[3][j]; }
        for (int j = 0; j < dimensions; j++) { for (int i = 0; i < n; i++) { output[36 + 3 * i + j] = omega_dot[i][j]; } }

        return output;
    }

    //This is the sumnation that every equation seems to use
    private float Sum(int i, int J) {
        float output = 0;
        for (int j = J; j < dimensions; j++) {
            output += Mathf.Pow(x[3][j] - x[i][j], 2);
        }
        return (output == 0) ? 0.00001f : output; //If the sum would return zero, return not zero (otherwise everything crashes due to a division by zero)
    }
}
