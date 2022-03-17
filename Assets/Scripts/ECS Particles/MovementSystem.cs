using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

// This system updates all entities in the scene with both a RotationSpeed_SpawnAndRemove and Rotation component.

// ReSharper disable once InconsistentNaming
public partial class MovementSystem : SystemBase {
    // private struct EntityWithLocalToWorld {
    //     public Entity entity;
    //     public LocalToWorld localToWorld;
    // }

    // OnUpdate runs on the main thread.
    protected override void OnUpdate() {
        var deltaTime = Time.DeltaTime;

        float boidPerceptionRadius = 8f;
        float separationWeight = 20f;
        float cohesionWeight = 3f;
        // float alignmentWeight = 10f;
        float cageRadius = 50f;
        float avoidCageWeight = 2f;
        float boidSpeed = 10f;
        
        // EntityQuery particleQuery = GetEntityQuery(ComponentType.ReadOnly<MovementComponent>(), ComponentType.ReadOnly<LocalToWorld>());
        EntityQuery particleQuery = GetEntityQuery(ComponentType.ReadOnly<MovementComponent>(), ComponentType.ReadOnly<Translation>());

        NativeArray<Entity> entityArray = particleQuery.ToEntityArray(Allocator.TempJob);
        if(entityArray.Length <= 0) {
            Debug.Log("entityArray is empty, returning");
            return;
        } else {
            Debug.Log(string.Format("found {0} entities in query", entityArray.Length));
        }
        // NativeArray<LocalToWorld> localToWorldArray = particleQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob);
        NativeArray<Translation> localToWorldArray = particleQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
        // NativeArray<float4x4> newParticleTransforms = new NativeArray<float4x4>(entityArray.Length, Allocator.TempJob);
        NativeArray<float3> newParticlePositions = new NativeArray<float3>(entityArray.Length, Allocator.TempJob);

        // for (int i = 0; i < entityArray.Length; i++) {
        //     boidArray[i] = new EntityWithLocalToWorld {
        //         entity = entityArray[i],
        //         localToWorld = localToWorldArray[i]
        //     };
        // }

        // entityArray.Dispose();
        // localToWorldArray.Dispose();

        /*
        Entities
            .WithName("MovementSystem_calculate")
            .WithAll<MovementComponent>()
            .ForEach((int entityInQueryIndex, ref Rotation rotation, ref Translation position, in MovementComponent rotSpeedSpawnAndRemove) => {
                // var _a = boidArray[0];
                // var _b = localToWorldArray[0];
                // var _c = entityArray[0];

                float3 boidPosition = position.Value;
                
                float3 seperationSum = float3.zero;
                float3 positionSum = float3.zero;
                float3 headingSum = float3.zero;

                int boidsNearby = 0;

                for (int otherBoidIndex = 0; otherBoidIndex < entityArray.Length; otherBoidIndex++) {
                    // if (particle != boidArray[otherBoidIndex]) {
                    if (otherBoidIndex != entityInQueryIndex) {
                        
                        // float3 otherPosition = localToWorldArray[otherBoidIndex].Position;
                        float3 otherPosition = localToWorldArray[otherBoidIndex].Value;
                        float distToOtherBoid = math.length(boidPosition - otherPosition); // TODO avoid square root here?
                        // Debug.Log(string.Format("for i={0}, otherpos={1}, dist={2}", entityInQueryIndex, otherPosition, distToOtherBoid));

                        if (distToOtherBoid < boidPerceptionRadius) {

                            seperationSum += -(otherPosition - boidPosition) * (1f / math.max(distToOtherBoid, .0001f));
                            positionSum += otherPosition;
                            // headingSum += localToWorldArray[otherBoidIndex].Forward;
                            // headingSum += math.forward(localToWorldArray[otherBoidIndex].Value);

                            boidsNearby++;
                        }
                    }
                }
                // Debug.Log(string.Format("nearby={0}, positionsum={1}", boidsNearby, positionSum));

                float3 force = float3.zero;

                if (boidsNearby > 0) {
                    float3 separationForce = (seperationSum / boidsNearby)                * separationWeight;
                    float3 cohesionForce   = ((positionSum / boidsNearby) - boidPosition) * cohesionWeight;
                    // float3 alignForce      = (headingSum / boidsNearby)                   * alignmentWeight;
                    float3 alignForce = float3.zero;
                    Debug.Log(string.Format("sf={0}, cf={1}, af={2}", separationForce, cohesionForce, alignForce));
                    force += (separationForce + cohesionForce + alignForce);
                } else {
                    // Debug.Log(string.Format("lonely boid, i={0}; bpr={1}", entityInQueryIndex, boidPerceptionRadius));
                }

                if(math.length(boidPosition) > cageRadius) {
                    force += -math.normalize(boidPosition) * avoidCageWeight;
                    // Debug.Log(string.Format("boid outside cage, length={0}, i={1}", math.length(boidPosition), entityInQueryIndex));
                }

                // float3 velocity = localToWorld.Forward * boidSpeed;
                // float3 velocity = math.forward(rotation.Value) * boidSpeed;
                // velocity += force * deltaTime;
                // velocity = math.normalize(velocity) * boidSpeed;
                float3 velocity = math.forward(rotation.Value) * force * boidSpeed;

                newParticlePositions[entityInQueryIndex] = position.Value + velocity * deltaTime;
                // newParticlePositions[entityInQueryIndex] = position.Value + (new float3(0, 0.01f, 0));
                    // localToWorld.Position + (velocity + new float3(0, 0.05f, 0)) * deltaTime,
                    // localToWorld.Position + (new float3(0, 1f, 0)) * deltaTime,
                    // position.Value + (new float3(0, 1f, 0))
                    // quaternion.LookRotationSafe(velocity, localToWorld.Up),
                    // localToWorld.Rotation,
                    // rotation.Value,
                    // new float3(1f)
                // );
                // Debug.Log("calculating position of i=" + entityInQueryIndex + ", force=" + force);
                // Debug.Log("calculating position of i=" + entityInQueryIndex + ", position=" + boidPosition);

            })
            // .WithDisposeOnCompletion(boidArray)
            .WithDisposeOnCompletion(localToWorldArray)
            .WithDisposeOnCompletion(entityArray)
            .ScheduleParallel();
        */
        // entityArray.Dispose();

        /*
        Entities
            .WithName("MovementSystem_update")
            .WithAll<MovementComponent>()
            // .ForEach((ref LocalToWorld localToWorld, in int entityInQueryIndex) => {
            .ForEach((ref Translation position, in int entityInQueryIndex) => {
            // .ForEach((ref LocalToWorld localToWorld, in int entityInQueryIndex, in RotationSpeed_SpawnAndRemove rotSpeedSpawnAndRemove) => {
                // var _a = newParticlePositions[0];

                // localToWorld.Value = newParticleTransforms[entityInQueryIndex];
                // localToWorld.Value = float4x4.TRS(
                //     new float3(0, 1f, 0), quaternion.EulerXYZ(1, 2, 3), new float3(1));
                // localToWorld.Value += float4x4.Translate(new float3(0, 0.1f, 0));

                // position.Value += new float3(0, 0.1f, 0);
                position.Value = newParticlePositions[entityInQueryIndex];

                // Debug.Log("updating position of i=" + entityInQueryIndex + ", new value=" + localToWorld.Value);
            })
            .WithDisposeOnCompletion(newParticlePositions)
            .ScheduleParallel();
        */

        // /*
        Entities
            .WithName("MovementSystem_debug_translate")
            .WithAll<MovementComponent>()
            .ForEach((ref Rotation rotation, ref Translation position, in MovementComponent rotSpeedSpawnAndRemove) => {
                // Rotate something about its up vector at the speed given by RotationSpeed_SpawnAndRemove.
                // rotation.Value = math.mul(math.normalize(rotation.Value), quaternion.AxisAngle(math.up(), rotSpeedSpawnAndRemove.RadiansPerSecond * deltaTime));
                position.Value += new float3(0, 0.01f, 0);
                // Debug.Log(string.Format("updated position to {0}", position.Value));
            }).ScheduleParallel();
        // */

        entityArray.Dispose();
        localToWorldArray.Dispose();
        newParticlePositions.Dispose();
    }

}
