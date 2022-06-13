using Unity.Entities;
using UnityEngine;
using Unity.Transforms;
using Unity.Rendering;


public class Spawner : MonoBehaviour {

    private EntityManager entityManager;

    public MovementComponent seaGluonMovementComponent;
    public int gluonsToSpawn;
    public GameObject gluonPrefab;
    private EntityArchetype gluonArchetype;
    private Entity gluonEntity;

    public MovementComponent seaQuarkMovementComponent;
    public int quarksToSpawn;
    public GameObject quarkPrefab;
    private EntityArchetype quarkArchetype;
    private Entity quarkEntity;

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
        quarkEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(quarkPrefab, settings);

        for(int i=0; i<gluonsToSpawn; i++) {
            Entity e = spawnRandomParticle(gluonEntity, seaGluonMovementComponent.cageRadius);
            entityManager.AddComponentData(e, seaGluonMovementComponent);
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
        }
        for(int i=0; i<quarksToSpawn; i++) {
            Entity e = spawnRandomParticle(quarkEntity, seaQuarkMovementComponent.cageRadius);
            entityManager.AddComponentData(e, seaQuarkMovementComponent);
            entityManager.AddComponentData(e, new RotationComponent());
            // entityManager.AddComponentData(e, new SQColorComponent {
            //     color = QuarkColor.Red
            // });
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


    // static Vector3[] scaleMeshVerts(Mesh m) {
    //     Vector3[] verts = m.vertices;
    //     for(int i=0; i<verts.Length; i++) {
    //         // verts[i] *= 0.05f;
    //     }
    //     return verts;
    // }

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
