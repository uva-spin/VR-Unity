using UnityEngine;
using Unity.Mathematics;

public class SeaQuarkSpawner : MonoBehaviour {

    public GameObject prefab;

    public float virtuality;
    public float x;
    public float spawnIntervalSeconds = 1f;
    public float cageRadius;

    private float lastSpawnTime;
    private float3 spawnPosition;

    [Tooltip("Total energy available for the proton to use for creating particles (1 Energy = 1 Seaquark pair)")] public float Energy = 10f;

    public void Start() {
        lastSpawnTime = -spawnIntervalSeconds;
        spawnPosition = float3.zero;
    }

    public void Update() {
        float t = Time.time;
        // Debug.Log(string.Format("t={0}, lst+i={1}", t, lastSpawnTime + spawnIntervalSeconds));
        if(Energy > 0 && t > lastSpawnTime + spawnIntervalSeconds) {
            // Debug.Log("Spawning sea quark");
            lastSpawnTime = t;
            SpawnQuark();
        }
    }

    private void SpawnQuark() {
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
        script.virtuality = this.virtuality;
        script.x = this.x;

        /*  Currently Disabled until Non-physics based sea quarks implemented
        if (FindObjectOfType<EMField>() && !FindObjectOfType<EMField>().physQuarks) {
            foreach (Rigidbody r in q.GetComponentsInChildren<Rigidbody>()) {
                Destroy(r);
            }
        };
        */
        QuarkPair[] pair = q.GetComponentsInChildren<QuarkPair>();
        pair[0].SetColor(QuarkColor.Red);
        pair[1].SetColor(QuarkColor.AntiRed);

        Energy--;
    }

    public void SetValue(float3 position) { // called by SeaQuarkBridgeSystem
       spawnPosition = position;
    }

    void OnDrawGizmos() {
        // Debug.Log(string.Format("gizmos {0}", Position));
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(spawnPosition, 0.03f);
    }

}