using System;
using Unity.Entities;

// ReSharper disable once InconsistentNaming
[Serializable]
public struct MovementComponent : IComponentData {
    public float RadiansPerSecond;
}
