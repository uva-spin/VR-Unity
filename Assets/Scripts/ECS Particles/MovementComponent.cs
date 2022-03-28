using System;
using Unity.Entities;

[Serializable]
public struct MovementComponent : IComponentData {
    public float boidSpeed;
    public float perceptionRadius;
    public float separationWeight;
    public float cohesionWeight;
    public float cageRadius;
    public float avoidCageWeight;
    public float valenceQuarkWeight;
}
