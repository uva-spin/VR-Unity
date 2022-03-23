using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


public partial class MovementSystem : SystemBase {
    // private struct EntityWithLocalToWorld {
    //     public Entity entity;
    //     public LocalToWorld localToWorld;
    // }

    // OnUpdate runs on the main thread.
    protected override void OnUpdate() {
        var deltaTime = Time.DeltaTime;
        
        EntityQuery particleQuery = GetEntityQuery(ComponentType.ReadOnly<MovementComponent>(), ComponentType.ReadOnly<LocalToWorld>());

        NativeArray<Entity> entityArray = particleQuery.ToEntityArray(Allocator.TempJob);
        NativeArray<LocalToWorld> localToWorldArray = particleQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob);
        // NativeArray<Translation> localToWorldArray = particleQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
        // NativeArray<Translation> localToWorldArray = particleQuery.ToComponentDataArray< ComponentType.ReadOnly<Translation>() >(Allocator.TempJob);
        // NativeArray<float4x4> newParticleTransforms = new NativeArray<float4x4>(entityArray.Length, Allocator.TempJob);
        NativeArray<float3> newParticlePositions = new NativeArray<float3>(entityArray.Length, Allocator.TempJob);

        // for (int i = 0; i < entityArray.Length; i++) {
        //     boidArray[i] = new EntityWithLocalToWorld {
        //         entity = entityArray[i],
        //         localToWorld = localToWorldArray[i]
        //     };
        // }

        // /*
        Entities
            .WithName("MovementSystem_calculate")
            .WithAll<MovementComponent>()
            .WithReadOnly(localToWorldArray)
            // .WithBurst()
            .ForEach((int entityInQueryIndex, ref Rotation rotation, ref Translation position, in MovementComponent movementComponent) => {
                float3 boidPosition = position.Value;
                
                float3 seperationSum = float3.zero;
                float3 positionSum = float3.zero;
                // float3 headingSum = float3.zero;

                int boidsNearby = 0;
                for (int otherBoidIndex = 0; otherBoidIndex < entityArray.Length; otherBoidIndex++) {
                    // if (particle != boidArray[otherBoidIndex]) {
                    if (otherBoidIndex != entityInQueryIndex) {
                        
                        float3 otherPosition = localToWorldArray[otherBoidIndex].Position;
                        // float3 otherPosition = localToWorldArray[otherBoidIndex].Value;
                        float distToOtherBoid = math.length(boidPosition - otherPosition); // TODO avoid square root here?
                        // Debug.Log(string.Format("for i={0}, otherpos={1}, dist={2}", entityInQueryIndex, otherPosition, distToOtherBoid));

                        if (distToOtherBoid < movementComponent.perceptionRadius) {
                            boidsNearby++;
                            seperationSum += -(otherPosition - boidPosition) * (1f / math.max(distToOtherBoid, .0001f));
                            positionSum += otherPosition;
                            // headingSum += localToWorldArray[otherBoidIndex].Forward;
                            // headingSum += math.forward(localToWorldArray[otherBoidIndex].Value);
                        }
                    }
                }
                // Debug.Log(string.Format("nearby={0}, positionsum={1}", boidsNearby, positionSum));

                float3 force = float3.zero;

                if (boidsNearby > 0) {
                    float3 separationForce = (seperationSum / boidsNearby)                * movementComponent.separationWeight;
                    // float3 cohesionForce   = ((positionSum / boidsNearby) - boidPosition) * movementComponent.cohesionWeight;
                    // float3 alignForce      = (headingSum / boidsNearby)                   * movementComponent.alignmentWeight;
                    float3 cohesionForce = float3.zero;
                    float3 alignForce = float3.zero;
                    force += (separationForce + cohesionForce + alignForce);
                    // Debug.Log(string.Format("sf={0}, cf={1}, af={2}", separationForce, cohesionForce, alignForce));
                }

                if(math.length(boidPosition) > movementComponent.cageRadius) {
                    float3 avoidCageForce = -math.normalize(boidPosition) * movementComponent.avoidCageWeight;
                    force += avoidCageForce;
                    // Debug.Log(string.Format("boid outside cage, length={0}, i={1}", math.length(boidPosition), entityInQueryIndex));
                    // Debug.Log(string.Format("boid outside cage, force={0}", avoidCageForce));
                }

                // float3 velocity = localToWorld.Forward * boidSpeed;
                // float3 velocity = math.forward(rotation.Value) * boidSpeed;
                // velocity += force * deltaTime;
                // velocity = math.normalize(velocity) * boidSpeed;
                // float3 velocity = math.forward(rotation.Value) * force * boidSpeed;
                float3 velocity = force * movementComponent.boidSpeed;

                newParticlePositions[entityInQueryIndex] = position.Value + velocity * deltaTime;
                // newParticlePositions[entityInQueryIndex] = position.Value + (new float3(0, 0.01f, 0));
                    // localToWorld.Position + (velocity + new float3(0, 0.05f, 0)) * deltaTime,
                    // position.Value + (new float3(0, 1f, 0))
                    // localToWorld.Rotation,
                    // rotation.Value,
                    // new float3(1f)
                // );
                // Debug.Log("calculating position of i=" + entityInQueryIndex + ", force=" + force);

            })
            .WithDisposeOnCompletion(localToWorldArray)
            .WithDisposeOnCompletion(entityArray)
            .ScheduleParallel();
        // */
        // localToWorldArray.Dispose();
        // entityArray.Dispose();

        /*
        Entities
            .WithName("MovementSystem_calculate")
            .WithAll<MovementComponent>()
            .ForEach((int entityInQueryIndex, ref Rotation rotation, ref Translation position, in MovementComponent rotSpeedSpawnAndRemove) => {
                // var _a = boidArray[0];
                // var _b = localToWorldArray[0];
                // var _c = entityArray[0];

                float3 boidPosition = position.Value;
                newParticlePositions[entityInQueryIndex] = boidPosition + new float3(0, 0.01f, 0);
            }).ScheduleParallel();
        */

        // /*
        Entities
            .WithName("MovementSystem_debug_translate")
            .WithAll<MovementComponent>()
            // .WithBurst()
            .ForEach((int entityInQueryIndex, ref Rotation rotation, ref Translation position, in MovementComponent rotSpeedSpawnAndRemove) => {
                // rotation.Value = math.mul(math.normalize(rotation.Value), quaternion.AxisAngle(math.up(), rotSpeedSpawnAndRemove.RadiansPerSecond * deltaTime));
                // position.Value += new float3(0, 0.01f, 0);
                position.Value = newParticlePositions[entityInQueryIndex];
                // Debug.Log(string.Format("updated position of i={0} to {1}", entityInQueryIndex, position.Value));
            })
            .WithDisposeOnCompletion(newParticlePositions)
            .ScheduleParallel();
        // */

        // entityArray.Dispose();
        // localToWorldArray.Dispose();
        // newParticlePositions.Dispose();
    }

}
