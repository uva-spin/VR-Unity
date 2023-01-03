using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EMField : MonoBehaviour
{
    //Various scale multipliers that can be changed for more precise measurements

    [Tooltip("Scale multiplier for u0/4pi = 1E-7")] public float u0Pi = Mathf.Pow(10, -7);
    [Tooltip("Scale multiplier for electric constant k")] public float k = 8_987_551_792.3f;
    [Tooltip("Scale mulitplier for size (model (~3m):proton (~8.33E-16m) = 2.78E-16)")] public float scale = 2.7778f * Mathf.Pow(10, -16);
    [Tooltip("Scale multiplier for charge")] public float cScale = 1.602_176_634f * Mathf.Pow(10, -19);
    [Tooltip("Scale multiplier for velocity cross products (new B = s^2 * B)")] public float vScale = 1;
    [Tooltip("Scale multiplier for force strength (For demonstration)")] public float strength = 1f;
    //[Tooltip("Scale multiplier for Sea Quark Pair force strength")] public float seaStrength = 0;
    //[Tooltip("Skips magnetic field calculation")] public bool fastField = false;
    //[Tooltip("Enables physics based seaquarks")] public bool physQuarks = false;
    //[Tooltip("Enables forcelines for seaquarks")] public bool seaLines = false;
    [Tooltip("Threshold for how close two seaquarks must be to cancel out")] public float cancelThreshold = 0.0001f;
    [Tooltip("Threshold for maximum force a quark can experience")] public float forceThreshold = 1000f;

    private List<TypeQuark> destroyNext = new List<TypeQuark>();

    private void Update()
    {
        TypeQuark[] quarks = FindObjectsOfType<TypeQuark>(); //Gets all quarks in the proton
        Vector3[] magForces = new Vector3[quarks.Length];
        Vector3[] elecForces = new Vector3[quarks.Length];

        for (int i = 0; i < quarks.Length; i++) //Iterates through each quark
        {
            Vector3 vel1 = quarks[i].GetComponent<Rigidbody>() ? quarks[i].GetComponent<Rigidbody>().velocity : ((quarks[i] is QuarkPair) ? ((QuarkPair)quarks[i]).getVelocity() : Vector3.zero);
            float charge1 = quarks[i].getCharge() * ((quarks[i].isPositive) ? 1 : -1 );

            Charges cT1 = quarks[i].chargeType;

            for (int j = i + 1; j < quarks.Length; j++) //Iterates through all quarks after target quark
            {
                float charge2 = quarks[j].getCharge() * ((quarks[j].isPositive) ? 1 : -1);
                Charges cT2 = quarks[j].chargeType;

                Vector3 eForce = CalculateElecF(quarks[i], quarks[j]); //Newton's Third Law:
                if (cT2 != Charges.PROBE) elecForces[i] += eForce * charge2; //Every action...
                if (cT1 != Charges.PROBE) elecForces[j] -= eForce * charge1; //...has an opposite yet equal reaction
                Vector3 mForce = CalculateMagF(quarks[i], quarks[j]);
                if (cT2 != Charges.PROBE) magForces[i] += mForce * charge2;
                if (cT1 != Charges.PROBE) magForces[j] -= mForce * charge1;
            }

            //Multiply by the charge and constants afterwords instead of in the calculation
            elecForces[i] *= -k * Mathf.Pow(cScale, 2) * charge1 / Mathf.Pow(scale, 2);
            magForces[i] = Vector3.Cross(vel1, (charge1 * Mathf.Pow(cScale * vScale / scale, 2) * u0Pi * magForces[i]));




            //Prints EM forces to console
            //Debug.Log(quarks[i].gameObject.name + "has net EM force: " + (elecForces[i] + magForces[i]) + " with an electric force of: " + elecForces[i] + " and magnetic force of: " + magForces[i]);
            if (true /*|| quarks[i].isCharged*/)
            {
                Vector3 force = (elecForces[i] + magForces[i]) * strength;

                if (force.magnitude > forceThreshold) force = force.normalized * forceThreshold;

                if (quarks[i].GetComponent<Rigidbody>()) quarks[i].GetComponent<Rigidbody>().AddForce(force * Time.deltaTime, ForceMode.Force);
                else if (quarks[i] is QuarkPair)
                {
                    //quarks[i].transform.position += Vector3.ClampMagnitude(force * Time.deltaTime * seaStrength, 999999);
                    //force *= seaStrength;
                }
                else if (quarks[i].GetComponentInChildren<TMP_Text>()) { //Force probe
                    quarks[i].GetComponentInChildren<TMP_Text>().text = (force * Time.deltaTime) + "";
                }

                LineRenderer lr = quarks[i].getLineType(VisualLines.FORCELINE);

                if (lr)
                {
                    Vector3[] linePos = { quarks[i].transform.position, Vector3.Lerp(lr.GetPosition(1), quarks[i].transform.position + Vector3.ClampMagnitude(force / 50000f + force.normalized / 1.5f, 5f), Time.deltaTime * 20) };
                    quarks[i].setVisualLine(VisualLines.FORCELINE, linePos[0], linePos[1]);
                }

            }
        }

        for (int i = 0; i < destroyNext.Count; i = 0) {
            SeaQuark sq = destroyNext[0].transform.parent.GetComponent<SeaQuark>();
            sq.HP = 0;
            destroyNext.Remove(destroyNext[0]);
        }
    }

    //F = q1(E + v1 x B)
    //E = -kq2/r^2 * rhat
    //B = u0/4pi * (q2*v2 x rhat)/r^2
    //FE = -kq1q2/r^2 * rhat
    //FB = (u0/4pi) * q1q2/r^2 * v1 x (v2 x rhat)
    //
    //Note: Magnetic Force is pretty weak in comparison to electric force (in terms of current scaling), so skipping magnetic force calculation may be advised for better performance
    //Also, q values multiplied to end due to scaling issues
    //
    //Handles calculations for forces
    Vector3 CalculateElecF(TypeQuark quark, TypeQuark quark2) { //F = -(k * q1q2/r^2) * r hat
        if (quark2 is StaticField)
        {
            if ((quark2 as StaticField).type && ((quark2 as StaticField).scope || quark2.GetComponent<Collider>().bounds.Intersects(quark.GetComponent<Collider>().bounds)))
                return ((quark2 as StaticField).Value * quark2.transform.up);
            return Vector3.zero;
        }
        if (quark is StaticField)
        {
            if ((quark as StaticField).type && ((quark as StaticField).scope || quark.GetComponent<Collider>().bounds.Intersects(quark2.GetComponent<Collider>().bounds)))
                return ((quark as StaticField).Value * quark.transform.up);
            return Vector3.zero;
        }
        Vector3 dir = (quark2 is StaticField) ? quark2.transform.up : quark2.transform.position - quark.transform.position;

        if (dir.magnitude <= cancelThreshold) { //Divide by 0 protection & sea/antisea quark annihilation checker
            if (quark is QuarkPair && quark2 is QuarkPair) {
                if (quark.isPositive != quark2.isPositive) { //Are they opposite?
                    destroyNext.Add(quark);
                    destroyNext.Add(quark2);
                }
            }
            return Vector3.zero; //Skips first calculation
        }

        return dir.normalized / Mathf.Pow(dir.magnitude, 2);
    }
    Vector3 CalculateMagF(TypeQuark quark, TypeQuark quark2) { //F = (u0/4pi) * (q1q2/r^2)v1 x (v2 x r hat)
        if (quark2 is StaticField)
        {
            if (!(quark2 as StaticField).type && ((quark2 as StaticField).scope || quark2.GetComponent<Collider>().bounds.Intersects(quark.GetComponent<Collider>().bounds)))
                return ((quark2 as StaticField).Value * quark2.transform.up);
        }
        if (quark is StaticField)
        {
            if (!(quark as StaticField).type && ((quark as StaticField).scope || quark.GetComponent<Collider>().bounds.Intersects(quark2.GetComponent<Collider>().bounds)))
                return ((quark as StaticField).Value * quark.transform.up);
        }
        Vector3 dir = quark2.transform.position - quark.transform.position;

        Vector3 vel2 = quark2.GetComponent<Rigidbody>() ? quark2.GetComponent<Rigidbody>().velocity : ((quark2 is QuarkPair) ? ((QuarkPair)quark2).getVelocity() : Vector3.zero);

        if (dir.magnitude <= cancelThreshold) {
            if (quark is QuarkPair && quark2 is QuarkPair) { //Divide by 0 protection & sea/antisea quark annihilation checker
                if (quark.isPositive != quark2.isPositive) { //Are they opposite?
                    destroyNext.Add(quark);
                    destroyNext.Add(quark2);
                }
            }
            return Vector3.zero; //Skips first calculation
        }

        return (Vector3.Cross(vel2, dir.normalized)) / (Mathf.Pow(dir.magnitude, 2));
    }
}
