using UnityEngine;

public class MoveRandomInsideSphere : MonoBehaviour
{
    [SerializeField]
    public float radius = 0.5f;

    [SerializeField]
    public float speed = 5f;

    [SerializeField]
    private float changeRotationSpeed = 100;

    private Vector3 target;

    [SerializeField]
    private float distanceThreshold = 0.1f;

    [SerializeField]
    private Vector2 changeIntervalMinMax = new Vector2(1, 3);

    [SerializeField]
    private bool gausian;

    private float curChangeInterval = 1;

    private float lastChangeTargetTime;

    private Vector3 force;

    private void Start()
    {
        target = GetRandomTarget();
        force = target.normalized;
    }

    // Update is called once per frame
    void Update()
    {
        force = Vector3.Slerp(force, target - transform.localPosition, Time.deltaTime * changeRotationSpeed);
        force.Normalize();

        transform.localPosition += force * speed * Time.deltaTime;

        var curDistance = Vector3.Distance(transform.localPosition, target);

        if (curDistance <= distanceThreshold || Time.time - lastChangeTargetTime > curChangeInterval)
        {
            target = GetRandomTarget();
        }
    }

    public Vector3 GetRandomTarget()
    {
        lastChangeTargetTime = Time.time;
        if (changeIntervalMinMax.x != changeIntervalMinMax.y)
        {
            curChangeInterval = Random.Range(changeIntervalMinMax.x, changeIntervalMinMax.y);
        }
        else
        {
            curChangeInterval = changeIntervalMinMax.x;
        }

        Vector3 result = Vector3.zero;

        if (gausian)
        {
            var x = RandomGaussian.Range(-1f, 1);
            var y = RandomGaussian.Range(-1f, 1f);
            var z = RandomGaussian.Range(-1f, 1f);
            result = new Vector3(x, y, z);
        }
        else
        {
            result = Random.insideUnitSphere;
        }
        return result * radius;
    }
}
