using UnityEngine;
using Unity.Mathematics;

public class SeaQuark : MonoBehaviour {
    private float HP;
    public float virtuality;
    public float x;

    private GameObject valenceQuarkDown;
    private GameObject valenceQuarkUpRed;
    private GameObject valenceQuarkUpBlue;

    private Vector3 dirAvg;

    private Vector3 RandomUnitVector() {
        return new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
    }

    private static float3 calculateValenceQuarkForce(float3 vectorToQuark) {
        return vectorToQuark / math.max(0.1f, math.lengthsq(vectorToQuark));
    }

    public void Start() {
        HP = 1f;
        valenceQuarkDown = GameObject.Find("Quark3_Down");
        valenceQuarkUpRed = GameObject.Find("Quark1_Up_Red");
        valenceQuarkUpBlue = GameObject.Find("Quark2_Up_Blue");

        dirAvg = RandomUnitVector() * 5;
        transform.rotation = Quaternion.LookRotation(dirAvg);
    }

    public void Update() {
        float3 valenceQuarkForce = calculateValenceQuarkForce(valenceQuarkDown.transform.position - transform.position) +
            calculateValenceQuarkForce(valenceQuarkUpRed.transform.position - transform.position) +
            calculateValenceQuarkForce(valenceQuarkUpBlue.transform.position - transform.position);

        Vector3 dir = valenceQuarkForce * 2.3f;
        dirAvg = dirAvg * 0.9f + dir * 0.1f;

        if(virtuality > 0.1f) {
            Vector3 shake = RandomUnitVector() * virtuality * 10f;
            dir += shake;

            if(UnityEngine.Random.Range(0, 900 - (int)(virtuality * 800)) == 1) { // range(0,~100) good for maximum jumpiness
                Vector3 jump = RandomUnitVector() * 150f;
                dir += jump;
                dirAvg = dir; // reset position history - also makes rotation look random
            }
        }

        transform.position += dir * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(dirAvg, Vector3.up);

        if(HP < 0) {
            Destroy(this.gameObject);
        } else {
            HP -= virtuality * 1.5f * Time.deltaTime; // TODO determine lifetime scaling with virtuality
        }
    }

}