using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

// ReSharper disable once InconsistentNaming
// [AddComponentMenu("DOTS Samples/SpawnAndRemove/Rotation Speed")]
public class MovementAuthoring : MonoBehaviour, IConvertGameObjectToEntity {
    public float DegreesPerSecond = 360;

    // The MonoBehaviour data is converted to ComponentData on the entity.
    // We are specifically transforming from a good editor representation of the data (Represented in degrees)
    // To a good runtime representation (Represented in radians)
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        // dstManager.AddComponentData(entity, new MovementComponent { RadiansPerSecond = math.radians(DegreesPerSecond) });
        dstManager.AddComponentData(entity, new MovementComponent { RadiansPerSecond = 0 });
        // dstManager.AddComponentData(entity, new LifeTime { Value = 0.0F });
        // dstManager.AddComponentData(entity, new ParticleColor { Hue = 0.0F });
    }
}
