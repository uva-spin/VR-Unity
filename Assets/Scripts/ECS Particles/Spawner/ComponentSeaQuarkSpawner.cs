using Unity.Entities;
using Unity.Mathematics;

public struct ComponentSeaQuarkSpawner : IComponentData
{
    public Entity prefab;
    public float3 position;
    public float charge;
}
