using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using Unity.Rendering;
using Unity.Mathematics;

public partial class GluonColorSystem : SystemBase {

    static float4 colorRamp(int n) {
        float brightness = Mathf.Clamp((float)n/9f, 0.1f, 1f);
        return new float4(255*brightness, 255*brightness, 255*brightness, 0.3f);
    }

    protected override void OnUpdate() {
        Entities
            .WithName("GluonColorSystem")
            .WithBurst()
            .ForEach((ref URPMaterialPropertyBaseColor baseColor, in MovementComponent movementComponent) => {
                baseColor.Value = colorRamp(movementComponent.gluonsNearby);
            })
            .ScheduleParallel();
    }

}
