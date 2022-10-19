///Sets spin values for each quark every update frame
///Note: values can only be spin up or spin down at current moment and only works along z-axis

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartonSpinAdjuster : MonoBehaviour
{
    Vector3 position = Vector3.zero;
    Vector3 polarization = Vector3.forward;

    [Tooltip("Threshold for total parton spin error allowed (Larger values are less realistic but usually slower to calculate)")] public float Threshold = 0.1f;

    float mostSpin = 0;

    float average = 0;
    int totalFrames = 0;

    private void Start()
    {
        position = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        float totalSpin = 0;

        Vector3 test = Vector3.zero;

        TypeQuark[] quarks = FindObjectsOfType<TypeQuark>();
        Vector3[] angMom = new Vector3[quarks.Length];
        bool[] spinUp = new bool[quarks.Length];

        int ups = 0;

        int most = 0;

        for (int i = 0; i < quarks.Length; i++) {
            if (quarks[i].GetComponent<Rigidbody>()) {
                Vector3 vel = quarks[i].GetComponent<Rigidbody>().velocity;
                angMom[i] = Mult(Vector3.Cross(vel, quarks[i].transform.position - position), polarization);

                spinUp[i] = totalSpin <= 0;
                spinUp[i] = (angMom[i][2] <= 0) ? !spinUp[i] : spinUp[i];
                totalSpin += angMom[i][2] * ((spinUp[i]) ? 0.5f : -0.5f);

                if (Mathf.Abs(angMom[i][2]) >= Mathf.Abs(angMom[most][2])) most = i;
            }
        }

        if (Mathf.Abs(totalSpin) > Threshold) { //If the sum of each spin is not close enough to desired value, go back and swap some spins
            for (int i = quarks.Length - 2; i >= 0; i--) { //We don't need to check the very last one since we just did it
                if (Mathf.Abs(totalSpin - (2 * (angMom[i][2] * ((spinUp[i]) ? 0.5f : -0.5f)))) < Mathf.Abs(totalSpin)) { //If negating spin values gets closer to value, do it
                    totalSpin -= 2 * (angMom[i][2] * ((spinUp[i]) ? 0.5f : -0.5f));
                    spinUp[i] = !spinUp[i];
                }
            }
        }

        for (int i = 0; i < quarks.Length; i++) {
            if (spinUp[i]) ups++;
        }

        average = (totalFrames * average + Mathf.Abs(totalSpin)) / (totalFrames + 1);
        totalFrames++;

        Debug.Log(totalSpin + " spin | " + ups + " ^ " + (quarks.Length - ups) + " v  | " + angMom[most][2] + " " + quarks[most].GetType() + " | " + average + " | " + mostSpin);

        if (Mathf.Abs(totalSpin) > Mathf.Abs(mostSpin)) mostSpin = totalSpin;
    }

    Vector3 Mult(Vector3 first, Vector3 second)
    {
        return new Vector3(first[0] * second[0], first[1] * second[1], first[2] * second[2]);
    }
}
