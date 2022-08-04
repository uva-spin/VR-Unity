using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


public partial class MovementSystem : SystemBase {
    // Random random;

    // static GameObject valenceQuarkDown;
    // static GameObject valenceQuarkUpRed;
    // static GameObject valenceQuarkUpBlue;

    // protected override void OnCreate() {
    //     base.OnCreate();
    //     valenceQuarkDown = GameObject.Find("Quark3_Down");
    //     valenceQuarkUpRed = GameObject.Find("Quark1_Up_Red");
    //     valenceQuarkUpBlue = GameObject.Find("Quark2_Up_Blue");
    //     Debug.Log(string.Format("found valence quarks, down position={0}", valenceQuarkDown.transform.position));
    // }

    private static float3 calculateValenceQuarkForce(float3 vectorToQuark) {
        return vectorToQuark / math.max(0.1f, math.lengthsq(vectorToQuark));
        // float lsq = math.lengthsq(vectorToQuark);
        // if(lsq > 3) {
        //     return float3.zero;
        // } else {
        //     return vectorToQuark / math.max(0.2f, lsq);
        // }
    }

    protected override void OnUpdate() {
        // float3 valenceQuarkDown = (float3) (GameObject.Find("Quark3_Down").transform.position); // TODO replace Find() with FindWithTag() for performance
        // float3 valenceQuarkUpRed = (float3) (GameObject.Find("Quark1_Up_Red").transform.position);
        // float3 valenceQuarkUpBlue = (float3) (GameObject.Find("Quark2_Up_Blue").transform.position);
        NativeArray<float3> valenceQuarkPositions = new NativeArray<float3>(
            GameObject.FindGameObjectsWithTag("Quark").Select(g => (float3)g.transform.position).ToArray(), Allocator.TempJob);
        
        EntityQuery particleQuery = GetEntityQuery(ComponentType.ReadOnly<MovementComponent>(), ComponentType.ReadOnly<Translation>());
        NativeArray<Entity> entityArray = particleQuery.ToEntityArray(Allocator.TempJob);
        // NativeArray<LocalToWorld> localToWorldArray = particleQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob);
        // NativeArray<Translation> localToWorldArray = particleQuery.ToComponentDataArray< ComponentType.ReadOnly<Translation>() >(Allocator.TempJob);
        NativeArray<float3> newParticlePositions = new NativeArray<float3>(entityArray.Length, Allocator.TempJob);
        NativeArray<Translation> oldParticleTranslations = particleQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
        entityArray.Dispose();
        
        float deltaTime = Time.DeltaTime;
        Unity.Mathematics.Random r = new Unity.Mathematics.Random(999);

        int maxGluonsNearby = 40;
        int maxGluonsToCheck = math.min(oldParticleTranslations.Length - 1, (int)(15000000f/(oldParticleTranslations.Length+10000)));

        Entities
            .WithName("MovementSystem_calculate")
            .WithReadOnly(oldParticleTranslations)
            .WithReadOnly(valenceQuarkPositions)
            .WithBurst()
            .ForEach((int entityInQueryIndex, ref Translation translation, ref MovementComponent movementComponent) => {
                float3 boidPosition = translation.Value;
                float3 attractRepelSum = float3.zero;

                int gluonsNearby = 0;
                for(int gluonsChecked = 0; gluonsChecked < maxGluonsToCheck; gluonsChecked++) {
                    if(gluonsNearby > maxGluonsNearby) {
                        // each gluon only has to check a few nearby gluons, because gluons that are close have the most influence and make up most of the force
                        break;
                    }

                    // float3 otherPosition = oldParticleTranslations[gluonsChecked].Value;
                    // float3 otherPosition = GetComponentDataFromEntity<Translation>(entityArray[0]);
                    float3 otherPosition = oldParticleTranslations[r.NextInt(0, oldParticleTranslations.Length)].Value;
                    if(boidPosition.Equals(otherPosition)) {
                        continue;
                    }
                    float3 diff = otherPosition - boidPosition;
                    float distToOtherBoid = math.length(diff); // TODO avoid square root here?

                    if (distToOtherBoid < movementComponent.perceptionRadius) { // particle is close enough to consider
                        gluonsNearby++;
                        attractRepelSum += -1 * movementComponent.repelWeight * math.normalize(diff) / math.pow(math.max(distToOtherBoid, 0.2f), 2);
                    }
                }

                float3 force = float3.zero;

                if (gluonsNearby > 0) {
                    float3 attractRepelForce = (attractRepelSum / gluonsNearby);
                    force += attractRepelForce;
                }

                float3 valenceQuarkForce = float3.zero;
                valenceQuarkForce += calculateValenceQuarkForce(valenceQuarkPositions[0] - boidPosition);
                valenceQuarkForce += calculateValenceQuarkForce(valenceQuarkPositions[1] - boidPosition);
                valenceQuarkForce += calculateValenceQuarkForce(valenceQuarkPositions[2] - boidPosition);
                force += valenceQuarkForce * movementComponent.valenceQuarkWeight;

                if(math.length(boidPosition) > movementComponent.cageRadius) {
                    float3 avoidCageForce = -math.normalize(boidPosition) * movementComponent.avoidCageWeight; // TODO this should be smoother, maybe exponential?
                    force += avoidCageForce;
                }

                float3 velocity = force * movementComponent.boidSpeed;
                velocity = (velocity + movementComponent.oldVelocity*movementComponent.momentum)/(movementComponent.momentum + 1);
                float3 newPosition = boidPosition + velocity * deltaTime;
                float3 oldVelocity = boidPosition - newPosition;

                movementComponent.oldVelocity = velocity;
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
                // position.Value += new float3(0, 0.01f, 0);
                // Debug.Log(string.Format("updated position of i={0} to {1}", entityInQueryIndex, position.Value));
            })
            .WithDisposeOnCompletion(newParticlePositions)
            .ScheduleParallel();

        // entityArray.Dispose();
        // localToWorldArray.Dispose();
        // newParticlePositions.Dispose();
    }

}
