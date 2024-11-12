using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct MovementComponent : IComponentData {
    public float boidSpeed;
    public float perceptionRadius;
    // public float attractWeight; // attraction behavior didn't seem very impactful and was confusing to implement
    public float repelWeight;
    // public float repelRadius; // this separation is only needed when considering attraction plus repelling
    public int momentum;
    public float cageRadius;
    public float avoidCageWeight;
    public float valenceQuarkWeight;

    public float3 oldVelocity;
    public int gluonsNearby;
}
