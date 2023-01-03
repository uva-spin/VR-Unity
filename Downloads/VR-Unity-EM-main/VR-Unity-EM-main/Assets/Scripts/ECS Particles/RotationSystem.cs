using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;


public partial class RotationSystem : SystemBase {

    protected override void OnUpdate() {
        // var deltaTime = Time.DeltaTime; // TODO incorporate somehow?
      
        Entities
            .WithName("RotationSystem")
            .WithAll<MovementComponent, RotationComponent>()
            .WithBurst()
            .ForEach((ref Rotation rotation, in MovementComponent movementComponent) => {
                // The second parameter is the "up" direction - setting it to the same as the velocity doesn't make much sense but it prevents the quarks
                // from all having the same rotation on one axis
                // rotation.Value = Quaternion.LookRotation(movementComponent.oldVelocity, movementComponent.oldVelocity);

                rotation.Value = Quaternion.LookRotation(movementComponent.oldVelocity, Vector3.right);
            })
            .ScheduleParallel();

    }

}
