using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct MovementComponent : IComponentData {
    public float boidSpeed;
    public float perceptionRadius;
    public float attractWeight;
    public float repelWeight;
    public float repelRadius;
    public int momentum;
    public float cageRadius;
    public float avoidCageWeight;
    public float valenceQuarkWeight;

    public float3 oldVelocity;
}
