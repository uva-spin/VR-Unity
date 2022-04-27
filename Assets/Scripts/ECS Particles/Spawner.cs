using Unity.Entities;
using UnityEngine;
using Unity.Transforms;
using Unity.Rendering;



public class Spawner : MonoBehaviour {

    public int boidsToSpawn;

    private EntityManager entityManager;
    private EntityArchetype entityArchetype;

    // public Mesh mesh;
    // public Material material;
    // private Mesh meshModified;

    public GameObject prefab;
    private Entity prefabEntity;

    // public MovementComponent seaQuarkMovementComponent;
    public MovementComponent seaGluonMovementComponent;

    // public float boidSpeed;
    // public float perceptionRadius;
    // public float attractWeight;
    // public float repelWeight;
    // public float repelRadius;
    // public float cageRadius;
    // public float avoidCageWeight;
    // public float valenceQuarkWeight;

    private void Start() {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        entityArchetype = entityManager.CreateArchetype(
            typeof(Translation),
            typeof(Rotation),
            typeof(LocalToWorld),
            typeof(RenderMesh),
            typeof(RenderBounds),
            typeof(MovementComponent));

        // Try scaling the mesh vertices directly?
        // meshModified = mesh;
        // meshModified.vertices = scaleMeshVerts(meshModified);
        // meshModified.RecalculateBounds();

        // mesh.vertices = scaleMeshVerts(mesh);
        // mesh.RecalculateBounds();

        var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
        prefabEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab, settings);

        for(int i=0; i<boidsToSpawn; i++) {
            spawnRandomParticle();
        }
    }

    // static Vector3[] scaleMeshVerts(Mesh m) {
    //     Vector3[] verts = m.vertices;
    //     for(int i=0; i<verts.Length; i++) {
    //         // verts[i] *= 0.05f;
    //     }
    //     return verts;
    // }

    // For prefabs:
    private void spawnRandomParticle() {
        Entity e = entityManager.Instantiate(prefabEntity);
        
        entityManager.SetComponentData(e, new Translation {
            Value = UnityEngine.Random.insideUnitSphere * seaGluonMovementComponent.cageRadius
        });
        // entityManager.AddComponentData(e, new Rotation {
        //     Value = quaternion.EulerXYZ(new Vector3(0, 0, 0))
        // });
        // entityManager.AddComponentData(e, new MovementComponent {
        //     boidSpeed = boidSpeed,
        //     perceptionRadius = perceptionRadius,
        //     attractWeight = attractWeight,
        //     repelWeight = repelWeight,
        //     repelRadius = repelRadius,
        //     cageRadius = cageRadius,
        //     avoidCageWeight = avoidCageWeight,
        //     valenceQuarkWeight = valenceQuarkWeight,
        // });
        entityManager.AddComponentData(e, seaGluonMovementComponent);
    }

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
