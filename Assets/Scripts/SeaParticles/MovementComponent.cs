using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct MovementComponent : IComponentData {
    public float gluonSpeed; // final multiplier on velocity
    public float perceptionRadius; // how close gluons have to be to get repelled
    public float repelWeight; // how strongly gluons in the perception radius are repelled
    public int momentum; // smoothing on final velocity to make gluons appear less jumpy
    public float cageRadius; // radius of the proton
    public float avoidCageWeight; // how strongly gluons outside of the proton are pushed back in
    public float valenceQuarkWeight; // how strongly gluons are attracted to the valence quarks
    // public float attractWeight; // attraction behavior didn't seem very impactful and was confusing to implement
    // public float repelRadius; // having a repel radius in addition to the perception radius only makes sense when there is also attraction behavior

    public float3 oldVelocity;
    public int gluonsNearby;
}
