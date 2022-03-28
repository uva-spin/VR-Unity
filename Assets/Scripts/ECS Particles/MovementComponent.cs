using System;
using Unity.Entities;

[Serializable]
public struct MovementComponent : IComponentData {
    public float boidSpeed;
    public float perceptionRadius;
    public float attractWeight;
    public float repelWeight;
    public float repelRadius;
    public float cageRadius;
    public float avoidCageWeight;
    public float valenceQuarkWeight;
}
