#if (false)

using Unity.Entities;

public struct SpawnerComponent : IComponentData {
    public int Count;
    public Entity Prefab;
    public float CageRadius;

    // Other fields for initial state (eg number of gluons vs seaquarks)
}
#endif
