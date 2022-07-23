using Unity.Entities;
using UnityEngine;
using Unity.Transforms;
using Unity.Rendering;

public class GluonSpawner : MonoBehaviour {

    private EntityManager entityManager;

    public MovementComponent gluonMovementComponent;
    public int gluonsToSpawn;
    public GameObject gluonPrefab;
    private EntityArchetype gluonArchetype;
    private Entity gluonEntity;

    // public Mesh mesh;
    // public Material material;

    private void Start() {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        // gluonArchetype = entityManager.CreateArchetype(
        //     typeof(Translation),
        //     typeof(Rotation),
        //     typeof(LocalToWorld),
        //     typeof(RenderMesh),
        //     typeof(RenderBounds),
        //     typeof(MovementComponent));

        // Try scaling the mesh vertices directly?
        // meshModified = mesh;
        // meshModified.vertices = scaleMeshVerts(meshModified);
        // meshModified.RecalculateBounds();

        // mesh.vertices = scaleMeshVerts(mesh);
        // mesh.RecalculateBounds();

        var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
        gluonEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(gluonPrefab, settings);

        for(int i=0; i<gluonsToSpawn; i++) {
            Entity e = spawnRandomParticle(gluonEntity, gluonMovementComponent.cageRadius);
            entityManager.AddComponentData(e, gluonMovementComponent);
        }
    }

    // For prefabs:
    private Entity spawnRandomParticle(Entity target, float radius) {
        Entity e = entityManager.Instantiate(target);
        
        entityManager.SetComponentData(e, new Translation {
            Value = UnityEngine.Random.insideUnitSphere * radius
        });
        return e;
    }

    /*
    static Vector3[] scaleMeshVerts(Mesh m) {
        Vector3[] verts = m.vertices;
        for(int i=0; i<verts.Length; i++) {
            // verts[i] *= 0.05f;
        }
        return verts;
    }
    */

    /*
    // For meshes:
    private void spawnRandomParticle() {
        Entity e = entityManager.CreateEntity(entityArchetype);

        entityManager.AddComponentData(e, new Translation {
            Value = UnityEngine.Random.insideUnitSphere * cageRadius
        });
        entityManager.AddComponentData(e, new Rotation {
            Value = quaternion.EulerXYZ(new Vector3(0, 0, 0))
        });
        entityManager.AddComponentData(e, new MovementComponent {
            boidSpeed = boidSpeed,
            perceptionRadius = perceptionRadius,
            separationWeight = separationWeight,
            cohesionWeight = cohesionWeight,
            alignmentWeight = alignmentWeight,
            cageRadius = cageRadius,
            avoidCageWeight = avoidCageWeight,
        });
        entityManager.AddSharedComponentData(e, new RenderMesh {
            mesh = mesh,
            // mesh = meshModified,
            material = material,
            // castShadows = UnityEngine.Rendering.ShadowCastingMode.On,
            // receiveShadows = true,
        });

        // https://answers.unity.com/questions/1787871/script-created-ecs-entity-renders-black.html
    }
    */
}
