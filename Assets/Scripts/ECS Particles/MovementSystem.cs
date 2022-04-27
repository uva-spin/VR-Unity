using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


public struct CellData {
    public float3 position;
}

public partial class MovementSystem : SystemBase {
    // Random random;
    // private struct EntityWithLocalToWorld {
    //     public Entity entity;
    //     public LocalToWorld localToWorld;
    // }

    // static GameObject valenceQuarkDown;
    // static GameObject valenceQuarkUpRed;
    // static GameObject valenceQuarkUpBlue;

    private static int GetPositionHashMapKey(float3 p) {
        float cellSize = 0.5f;
        // int yMultiplier = (int) cellSize*3;
        // int zMultiplier = (int) yMultiplier*3;
        int yMultiplier = 6;
        int zMultiplier = 36;
        return (int) (math.floor(p.x/cellSize) + yMultiplier*math.floor(p.y/cellSize) + zMultiplier*math.floor(p.z/cellSize));;
    }

    // protected override void OnCreate() {
    //     base.OnCreate();
    //     valenceQuarkDown = GameObject.Find("Quark3_Down");
    //     valenceQuarkUpRed = GameObject.Find("Quark1_Up_Red");
    //     valenceQuarkUpBlue = GameObject.Find("Quark2_Up_Blue");
    //     Debug.Log(string.Format("found valence quarks, down position={0}", valenceQuarkDown.transform.position));
    // }

    // OnUpdate runs on the main thread.
    protected override void OnUpdate() {
        var deltaTime = Time.DeltaTime;

        float3 valenceQuarkDown = new float3(GameObject.Find("Quark3_Down").transform.position);
        float3 valenceQuarkUpRed = new float3(GameObject.Find("Quark1_Up_Red").transform.position);
        float3 valenceQuarkUpBlue = new float3(GameObject.Find("Quark2_Up_Blue").transform.position);
        
        EntityQuery particleQuery = GetEntityQuery(ComponentType.ReadOnly<MovementComponent>(), ComponentType.ReadOnly<LocalToWorld>());

        NativeArray<Entity> entityArray = particleQuery.ToEntityArray(Allocator.TempJob);
        // NativeArray<LocalToWorld> localToWorldArray = particleQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob);
        // NativeArray<Translation> localToWorldArray = particleQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
        // NativeArray<Translation> localToWorldArray = particleQuery.ToComponentDataArray< ComponentType.ReadOnly<Translation>() >(Allocator.TempJob);
        NativeArray<float3> newParticlePositions = new NativeArray<float3>(entityArray.Length, Allocator.TempJob);

        NativeMultiHashMap<int, CellData> cellVsEntityPositions = new NativeMultiHashMap<int, CellData>(entityArray.Length, Allocator.TempJob);

        float3 fuzzMask = new float3(UnityEngine.Random.Range(-1, 2), UnityEngine.Random.Range(-1, 2), UnityEngine.Random.Range(-1, 2));

        Entities
            .WithAll<MovementComponent>()
            .WithBurst()
            .ForEach((Entity entity, ref Translation translation) => {
                int key = GetPositionHashMapKey(translation.Value);
                cellVsEntityPositions.Add(key, new CellData {
                    position = translation.Value,
                });
            }).Schedule();
        

        Entities
            .WithName("MovementSystem_quadrant_calculate")
            .WithReadOnly(cellVsEntityPositions)
            .WithBurst()
            .ForEach((int entityInQueryIndex, ref Translation translation, ref MovementComponent movementComponent) => {

                float3 boidPosition = translation.Value;
                float3 boidPositionFuzzed = boidPosition + fuzzMask * ((entityInQueryIndex % 100) - 50)/500f;
                float3 attractRepelSum = float3.zero;

                int boidsNearby = 0;
                int hashMapKey = GetPositionHashMapKey(boidPositionFuzzed);
                CellData otherEntityData;
                NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
                if(cellVsEntityPositions.TryGetFirstValue(hashMapKey, out otherEntityData, out nativeMultiHashMapIterator)) {
                    do {
                        float3 otherPosition = otherEntityData.position;
                        if(otherPosition.Equals(boidPosition)) {
                            continue;
                        }

                        float3 diff = otherPosition - boidPosition;
                        float distToOtherBoid = math.length(diff); // TODO avoid square root here?

                        if (distToOtherBoid < movementComponent.perceptionRadius) { // particle is too far away to consider
                            boidsNearby++;

                            if(distToOtherBoid < movementComponent.repelRadius) { // repelled to this particle
                                attractRepelSum += -1 * movementComponent.repelWeight * math.normalize(diff) / math.pow(math.max(distToOtherBoid, 0.2f), 2);
                            } else { // attracted to this particle
                                attractRepelSum += movementComponent.attractWeight * math.normalize(diff);
                            }
                        }

                    } while (cellVsEntityPositions.TryGetNextValue(out otherEntityData, ref nativeMultiHashMapIterator));
                }

                float3 force = float3.zero;

                if (boidsNearby > 0) {
                    float3 attractRepelForce = (attractRepelSum / boidsNearby);
                    force += attractRepelForce;
                }

                float3 valenceQuarkForce = float3.zero;
                float3 q1Dir = valenceQuarkDown - boidPosition;
                float3 q2Dir = valenceQuarkUpRed - boidPosition;
                float3 q3Dir = valenceQuarkUpBlue - boidPosition;
                valenceQuarkForce += (q1Dir / math.max(0.2f, math.lengthsq(q1Dir)));
                valenceQuarkForce += (q2Dir / math.max(0.2f, math.lengthsq(q2Dir)));
                valenceQuarkForce += (q3Dir / math.max(0.2f, math.lengthsq(q3Dir)));
                force += valenceQuarkForce * movementComponent.valenceQuarkWeight;

                if(math.length(boidPosition) > movementComponent.cageRadius) {
                    // Maybe if the particle is outside the cage, it skips all other calculations?
                    float3 avoidCageForce = -math.normalize(boidPosition) * movementComponent.avoidCageWeight; // TODO this should be smoother, maybe exponential?
                    force += avoidCageForce;
                }

                // float3 velocity = localToWorld.Forward * boidSpeed;
                // float3 velocity = math.forward(rotation.Value) * boidSpeed;
                // velocity += force * deltaTime;
                // velocity = math.normalize(velocity) * boidSpeed;
                // float3 velocity = math.forward(rotation.Value) * force * boidSpeed;
                float3 velocity = force * movementComponent.boidSpeed;
                velocity = (velocity + movementComponent.oldVelocity*movementComponent.momentum)/(movementComponent.momentum + 1);
                float3 newPosition = boidPosition + velocity  * deltaTime;
                float3 oldVelocity = boidPosition - newPosition;

                movementComponent.oldVelocity = velocity;

                // Debug.Log("calculating position of i=" + entityInQueryIndex + ", force=" + force);
                // Debug.Log(string.Format("finished boid, i={0}, oldp={1}, f={2}", entityInQueryIndex, boidPosition, force));
                newParticlePositions[entityInQueryIndex] = newPosition;

            })
            // .WithDisposeOnCompletion(localToWorldArray)
            // .WithDisposeOnCompletion(entityArray)
            .WithDisposeOnCompletion(cellVsEntityPositions)
            .ScheduleParallel();

        // /*
        Entities
            .WithName("MovementSystem_translate")
            .WithAll<MovementComponent>()
            .WithReadOnly(newParticlePositions)
            .WithBurst()
            .ForEach((int entityInQueryIndex, ref Rotation rotation, ref Translation position) => {
                // position.Value += new float3(0, 0.01f, 0);
                position.Value = newParticlePositions[entityInQueryIndex];
                // Debug.Log(string.Format("updated position of i={0} to {1}", entityInQueryIndex, position.Value));
            })
            .WithDisposeOnCompletion(newParticlePositions)
            .ScheduleParallel();
        // */

        entityArray.Dispose();
        // localToWorldArray.Dispose();
        // newParticlePositions.Dispose();
    }

}
