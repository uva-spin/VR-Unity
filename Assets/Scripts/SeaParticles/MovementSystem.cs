using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


public partial class MovementSystem : SystemBase {

    private static float3 calculateValenceQuarkForce(float3 vectorToQuark) {
        return vectorToQuark / math.max(0.1f, math.lengthsq(vectorToQuark));
    }

    protected override void OnUpdate() {
        NativeArray<float3> valenceQuarkPositions = new NativeArray<float3>(
            GameObject.FindGameObjectsWithTag("Quark").Select(g => (float3)g.transform.position).ToArray(), Allocator.TempJob);
        
        EntityQuery particleQuery = GetEntityQuery(ComponentType.ReadOnly<MovementComponent>(), ComponentType.ReadOnly<Translation>());
        NativeArray<Entity> entityArray = particleQuery.ToEntityArray(Allocator.TempJob);
        NativeArray<float3> newParticlePositions = new NativeArray<float3>(entityArray.Length, Allocator.TempJob);
        NativeArray<Translation> oldParticleTranslations = particleQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
        entityArray.Dispose();
        
        float deltaTime = Time.DeltaTime;
        Unity.Mathematics.Random r = new Unity.Mathematics.Random(999);

        int maxGluonsNearbyToCheck = 40;
        int maxGluonsToCheck = math.min(oldParticleTranslations.Length - 1, (int)(15000000f/(oldParticleTranslations.Length+10000))); // 

        Entities
            .WithName("MovementSystem_calculate")
            .WithReadOnly(oldParticleTranslations)
            .WithReadOnly(valenceQuarkPositions)
            .WithBurst()
            .ForEach((int entityInQueryIndex, ref Translation translation, ref MovementComponent movementComponent) => {
                float3 gluonPosition = translation.Value;
                float3 attractRepelSum = float3.zero;

                int gluonsNearby = 0;
                for(int gluonsChecked = 0; gluonsChecked < maxGluonsToCheck; gluonsChecked++) {
                    if(gluonsNearby > maxGluonsNearbyToCheck) {
                        // If this gluon happens to consider a lot of gluons that are close to it (i.e. within its perception radius), it doesn't have to 
                        // consider many gluons total because gluons that are close have the most influence and make up most of the force
                        break;
                    }

                    float3 otherPosition = oldParticleTranslations[r.NextInt(0, oldParticleTranslations.Length)].Value;
                    if(gluonPosition.Equals(otherPosition)) {
                        continue;
                    }
                    float3 vectorToOtherGluon = otherPosition - gluonPosition;
                    float distToOtherGluon = math.length(vectorToOtherGluon);

                    if (distToOtherGluon < movementComponent.perceptionRadius) { 
                        // gluon is close enough to consider for force calculations
                        gluonsNearby++;
                        attractRepelSum += -1 * movementComponent.repelWeight * math.normalize(vectorToOtherGluon) / math.pow(math.max(distToOtherGluon, 0.2f), 2);
                    }
                }

                float3 force = float3.zero; // holds the total force on the gluon, starts as zero and gets added to for each force

                if (gluonsNearby > 0) {
                    float3 attractRepelForce = (attractRepelSum / gluonsNearby);
                    force += attractRepelForce;
                }

                float3 valenceQuarkForce = float3.zero;
                valenceQuarkForce += calculateValenceQuarkForce(valenceQuarkPositions[0] - gluonPosition);
                valenceQuarkForce += calculateValenceQuarkForce(valenceQuarkPositions[1] - gluonPosition);
                valenceQuarkForce += calculateValenceQuarkForce(valenceQuarkPositions[2] - gluonPosition);
                force += valenceQuarkForce * movementComponent.valenceQuarkWeight;

                if(math.length(gluonPosition) > movementComponent.cageRadius) {
                    // if the gluon is outside the proton (or the "cage") then apply a force to push the gluon back inside the proton
                    float3 avoidCageForce = -math.normalize(gluonPosition) * movementComponent.avoidCageWeight;
                    force += avoidCageForce;
                }

                float3 velocity = force * movementComponent.gluonSpeed;
                velocity = (velocity + movementComponent.oldVelocity*movementComponent.momentum)/(movementComponent.momentum + 1); // apply momentum - just averaging the new and old velocities
                float3 newPosition = gluonPosition + velocity * deltaTime;
                float3 oldVelocity = gluonPosition - newPosition;

                movementComponent.oldVelocity = velocity; // used in momentum calculation
                movementComponent.gluonsNearby = gluonsNearby; // used for density-based coloring

                newParticlePositions[entityInQueryIndex] = newPosition;

            })
            .WithDisposeOnCompletion(valenceQuarkPositions)
            .WithDisposeOnCompletion(oldParticleTranslations)
            .ScheduleParallel();

        Entities
            .WithName("MovementSystem_translate")
            .WithAll<MovementComponent>()
            .WithReadOnly(newParticlePositions)
            .WithBurst()
            .ForEach((int entityInQueryIndex, ref Translation position) => {
                position.Value = newParticlePositions[entityInQueryIndex];
                // Debug.Log(string.Format("updated position of i={0} to {1}", entityInQueryIndex, position.Value));
            })
            .WithDisposeOnCompletion(newParticlePositions)
            .ScheduleParallel();

    }
}
