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

    public void Start() {
        lastSpawnTime = -spawnIntervalSeconds;
        spawnPosition = float3.zero;
    }

    public void Update() {
        float t = Time.time;
        // Debug.Log(string.Format("t={0}, lst+i={1}", t, lastSpawnTime + spawnIntervalSeconds));
        if(t > lastSpawnTime + spawnIntervalSeconds) {
            // Debug.Log("Spawning sea quark");
            lastSpawnTime = t;
            SpawnQuark();
        }
    }

    private void SpawnQuark() {
        GameObject q = Instantiate(prefab);

        q.transform.position = this.spawnPosition;

        var script = q.GetComponent<SeaQuark>();
        script.cageRadius = this.cageRadius;
        script.virtuality = this.virtuality;
        script.x = this.x;
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