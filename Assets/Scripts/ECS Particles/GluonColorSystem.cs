using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using Unity.Rendering;
using Unity.Mathematics;

public partial class GluonColorSystem : SystemBase {

    static float4 colorRamp(int n) {
        float nScaled = Mathf.Clamp((float)n + 15f, 0, 80f)/80f;
        var c = Color.HSVToRGB(nScaled, 1f, 1f);
        // return new float4(n, 0, 255-n, 1)
        return new float4(c.r, c.g, c.b, 0.3f);
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
