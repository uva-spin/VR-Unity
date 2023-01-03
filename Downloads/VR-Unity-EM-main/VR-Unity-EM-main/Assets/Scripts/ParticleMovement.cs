using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleMovement : MonoBehaviour
{
    float partSpeed = 1;
    public Rigidbody rb;
    public AllQuarkScript all;


    // Start is called before the first frame update
    void Start()
    {
        all = transform.parent.GetComponent<AllQuarkScript>();
        Physics.IgnoreLayerCollision(8,8);
        rb = GetComponent<Rigidbody>();

        Vector3 direction = Random.insideUnitCircle.normalized;
        rb.AddForce(direction * partSpeed, ForceMode.Impulse);
    }

    void FixedUpdate()
    {
        partSpeed = all.quarkSpeed;
        rb.velocity = partSpeed * (rb.velocity.normalized);
    }

}
