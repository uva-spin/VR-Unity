using Unity.Entities;
using UnityEngine;
using Unity.Transforms;

public class GluonSpawner : MonoBehaviour {

    private EntityManager entityManager;

    public MovementComponent gluonMovementComponent;
    public int gluonsToSpawn;
    public GameObject gluonPrefab;
    private EntityArchetype gluonArchetype;
    private Entity gluonEntity;

    private void Start() {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
        gluonEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(gluonPrefab, settings);

        for(int i=0; i<gluonsToSpawn; i++) {
            Entity e = spawnRandomParticle(gluonEntity, gluonMovementComponent.cageRadius);
            entityManager.AddComponentData(e, gluonMovementComponent);
        }
    }

    // For spawning prefabs:
    private Entity spawnRandomParticle(Entity target, float radius) {
        Entity e = entityManager.Instantiate(target);
        
        entityManager.SetComponentData(e, new Translation {
            Value = UnityEngine.Random.insideUnitSphere * radius
        });
        return e;
    }


    /*
    // For spawning meshes:
    // https://answers.unity.com/questions/1787871/script-created-ecs-entity-renders-black.html
    private void spawnRandomParticle() {
        Entity e = entityManager.CreateEntity(entityArchetype);

        entityManager.AddComponentData(e, new Translation {
            Value = UnityEngine.Random.insideUnitSphere * cageRadius
        });
        entityManager.AddComponentData(e, gluonMovementComponent);
        entityManager.AddSharedComponentData(e, new RenderMesh {
            mesh = mesh,
            material = material,
            // castShadows = UnityEngine.Rendering.ShadowCastingMode.On,
            // receiveShadows = true,
        });
    }
    */
}
