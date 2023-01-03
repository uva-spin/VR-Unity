using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

// Equivalent to LateUpdate():
// Necessary to prevent reading the translation array at the same time it's being updated by movementsystem
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))] 
public partial class SeaQuarkBridgeSystem : SystemBase {
    
    protected override void OnUpdate() {

        var deltaTime = Time.DeltaTime;

        GameObject spawner = GameObject.Find("SeaQuarkSpawner");
        if (spawner == null)
            return;
        SeaQuarkSpawner spawnerScript = spawner.GetComponent<SeaQuarkSpawner>();
        
        EntityQuery particleQuery = GetEntityQuery(ComponentType.ReadOnly<MovementComponent>(), ComponentType.ReadOnly<Translation>());
        NativeArray<Entity> entityArray = particleQuery.ToEntityArray(Allocator.TempJob);
        // NativeArray<Translation> oldParticleTranslations = particleQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
        // NativeArray<float3> newParticlePositions = new NativeArray<float3>(entityArray.Length, Allocator.TempJob);
        ComponentDataFromEntity<Translation> translationArray = GetComponentDataFromEntity<Translation>(true);
        
        // Unity.Mathematics.Random r = new Unity.Mathematics.Random(999);
        float3 sum = float3.zero;

        // Entities
        //     .WithName("SeaQuarkBridgeSystem")
        //     // .WithBurst()
        //     .ForEach((ref Translation translation, ref MovementComponent movementComponent) => {
        //         float3 gluonPosition = translation.Value;
        //         // sum += gluonPosition;
        //     })
        //     .Run();


        int count = 0;
        for(int i=0; i<entityArray.Length; i+=300) {
            // Debug.Log(string.Format("adding val={0}, sum={1}", translationArray[entityArray[i]].Value, sum));
            float3 p = translationArray[entityArray[i]].Value;
            bool3 n = math.isnan(p);
            if(!(n[0] || n[1] || n[2])) { // sum variable will get messed up if any NaN value is added to it
                sum += p;
            }
            count += 1;
        }
        // for(int i=0; i<allTranslations.; i+=100) {
        //     sum += allTranslations[i];
        // }

        // Debug.Log(string.Format("setting val={0}", sum));
        // Debug.Log(string.Format("setting val={0}", sum / (20000f/100f)));
        sum = sum / (float)count;
        spawnerScript.SetValue(sum);

        entityArray.Dispose();
        // particleQuery.Dispose();
    }
}
