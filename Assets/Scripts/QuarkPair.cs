using System;
using DG.Tweening;
using UnityEngine;

public class QuarkPair : TypeQuark
{
    Vector3 averageVelocity;
    Vector3 oldPos;

    private void Awake()
    {
        oldPos = transform.position;
        partSys1 = GetComponent<ParticleSystem>();
        UpdateColor();
    }

    [Obsolete]
    private void Start()
    {
        FlashToColor(quarkColor);

        //TODO: fix anti-color to have 157 opacity by default
        Color c = GetComponent<ParticleSystem>().startColor;
        c.a = 157f / 255f;
        GetComponent<ParticleSystem>().startColor = c;
    }

    // Update is called once per frame
    void Update()
    {
        averageVelocity = (transform.position - oldPos) / Time.deltaTime;
        oldPos = transform.position;

        Keyframe key = new Keyframe();
        key.value = 1f;
        if (!FindObjectOfType<AllQuarkScript>() || !FindObjectOfType<AllQuarkScript>().forceLineVis)
        {
            key.value = 0;
        }

        GetComponent<LineRenderer>().widthCurve = new AnimationCurve(new Keyframe[] { key, key }); //Sets the width curve to 0 (you probably could just remove it entirely, but it might cause problems)

        Vector3 pos = (!FindObjectOfType<Proton>()) ? Vector3.zero : transform.position - FindObjectOfType<Proton>().transform.position;

        if (pos.magnitude >= 0.01f) GetComponent<Rigidbody>().AddForce(-pos.normalized * FindObjectOfType<AllQuarkScript>().forceSlider.value * 20000 / pos.magnitude * Time.deltaTime, ForceMode.Force);

        if (pos.magnitude > 50) { transform.parent.GetComponent<SeaQuark>().HP = 0; } //If a certain distance away from proton, delete it
    }
    public Vector3 getVelocity() { return averageVelocity; }
}
