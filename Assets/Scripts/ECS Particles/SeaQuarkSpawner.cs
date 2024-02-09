using UnityEngine;
using Unity.Mathematics;
using System;

public class SeaQuarkSpawner : MonoBehaviour
{

    public GameObject prefab;

    [Tooltip("The resolution of the proton. Larger values decrease the size of the quarks. Not yet implemented.")] [Range(0.1f, 100f)] public float q2;
    [Tooltip("Determines the order of magnitude of x (10^{degree}). X determines how much energy the proton has by ln(1/x). Smaller degrees contains more energy, larger less.")] [Range(-8, 0)] public float xDegree = 0;
    public float spawnIntervalSeconds = 1f;
    public float cageRadius;

    private float lastSpawnTime;
    private float3 spawnPosition;

    [Tooltip("Restart the simulation or the proton to update.")] public float seaQuarkEnergyCost = 1;
    private float _seaQuarkEnergyCost = 1;

    [Tooltip("Total energy used by system (1 seaquark raises energy by 1). Maximum energy is calculated by ln(1/x).")] [SerializeField] private float Energy = 0f;

    private int numPairs = 0;

    public int getPairCount() {
        return numPairs;
    }

    public void Start()
    {
        lastSpawnTime = -spawnIntervalSeconds;
        spawnPosition = float3.zero;

        _seaQuarkEnergyCost = seaQuarkEnergyCost;
    }

    public void Update()
    {
        float t = Time.time;

        // Debug.Log(string.Format("t={0}, lst+i={1}", t, lastSpawnTime + spawnIntervalSeconds));

        //Since we're dealing with exponents (ln(1/10^degree)), we can simplify the equation (-degree*ln(10)), and ln(10) is just 2.30258509299, or 2.3026ish

        if (Energy < xDegree * -2.3026f && t > lastSpawnTime + spawnIntervalSeconds)
        {
            // Debug.Log("Spawning sea quark");
            lastSpawnTime = t;
            SpawnQuark();
            numPairs++;
        }

        foreach (SeaQuark s in FindObjectsOfType<SeaQuark>()) {
            QuarkPair[] qs = s.GetComponentsInChildren<QuarkPair>();
            qs[0].transform.localScale = 2.5f / (q2+25) * Vector3.one;
            qs[1].transform.localScale = qs[0].transform.localScale;
        }
        foreach (Quark q in FindObjectsOfType<Quark>()) {
            q.transform.localScale = -2 * (float)Math.Log(q2 / 1000) * Vector3.one;//(5 + (2 / (q2))) * Vector3.one;
        }
    }

    private void SpawnQuark()
    {
        GameObject q = Instantiate(prefab);

        q.transform.position = UnityEngine.Random.insideUnitCircle;//this.spawnPosition;

        /*
        if (FindObjectOfType<EMField>() && !FindObjectOfType<EMField>().seaLines) { //If seaquark forcelines are disabled, then disable them
            LineRenderer[] lines = q.GetComponentsInChildren<LineRenderer>();
            Keyframe key = new Keyframe();
            key.value = 0;
            foreach (LineRenderer line in lines) {
                line.widthCurve = new AnimationCurve(new Keyframe[] { key, key }); //Sets the width curve to 0 (you probably could just remove it entirely, but it might cause problems)
            }
        }
        */
        var script = q.GetComponent<SeaQuark>();
        script.cageRadius = this.cageRadius;
        script.virtuality = this.q2;
        script.x = this.xDegree;

        /*  Currently Disabled until Non-physics based sea quarks implemented
        if (FindObjectOfType<EMField>() && !FindObjectOfType<EMField>().physQuarks) {
            foreach (Rigidbody r in q.GetComponentsInChildren<Rigidbody>()) {
                Destroy(r);
            }
        };
        */
        QuarkPair[] pair = q.GetComponentsInChildren<QuarkPair>();
        float random = UnityEngine.Random.Range(0f, 1f);
        script.color = (random <= 0.33f ? QuarkColor.Red : random >= 0.66f ? QuarkColor.Green : QuarkColor.Blue);

        Energy += _seaQuarkEnergyCost;
    }

    public void SetValue(float3 position)
    { // called by SeaQuarkBridgeSystem
        spawnPosition = position;
    }

    void OnDrawGizmos()
    {
        // Debug.Log(string.Format("gizmos {0}", Position));
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(spawnPosition, 0.03f);
    }

    public void DestroyParticle(SeaQuark s) {
        if (!s || !s.gameObject) return;
        Destroy(s.gameObject);
        Energy -= _seaQuarkEnergyCost;
        numPairs--;
    }

    public void ResetAllParticles() {
        foreach (SeaQuark s in FindObjectsOfType<SeaQuark>()) {
            Destroy(s.gameObject);
        }
        Energy = 0;
        numPairs = 0;
        _seaQuarkEnergyCost = seaQuarkEnergyCost;
    }
}