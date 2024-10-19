using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeShaderUser : MonoBehaviour
{
    public Material material;
    public Mesh mesh;
    public ComputeShader computeShader;
    private Matrix4x4[] transforms;
    private Vector3[] velocities;

    private readonly int TRANSFORM_SIZE = sizeof(float) * 16;
    private readonly int THREADS = 10;
    private readonly int ALLOCATED_CHUNKS = 4;
    private readonly int MAXIMUM_INSTANCED_TRANSFORMS = 1023; //Must be less than 1024. Here just for clarity

    private void Start()
    {
        transforms = new Matrix4x4[ALLOCATED_CHUNKS * MAXIMUM_INSTANCED_TRANSFORMS];
        velocities = new Vector3[ALLOCATED_CHUNKS * MAXIMUM_INSTANCED_TRANSFORMS];
    }

    protected void Update()
    {
        UpdatePositions();
        UpdateRendering();
    }

    private void UpdatePositions() {
        SetComputeShaderInformation();
    }

    private void SetComputeShaderInformation() {
        ComputeBuffer transformBuffer = new ComputeBuffer(transforms.Length * MAXIMUM_INSTANCED_TRANSFORMS, TRANSFORM_SIZE);
        ComputeBuffer velocityBuffer = new ComputeBuffer(transforms.Length * MAXIMUM_INSTANCED_TRANSFORMS, TRANSFORM_SIZE);
        transformBuffer.SetData(transforms);
        velocityBuffer.SetData(velocities);
        computeShader.SetBuffer(0, "transforms", transformBuffer);
        computeShader.SetBuffer(0, "velocities", velocityBuffer);
        computeShader.SetFloat("delta", Time.deltaTime);
        computeShader.Dispatch(0, transforms.Length / THREADS, 1, 1);
        transformBuffer.GetData(transforms);
        velocityBuffer.GetData(velocities);
    }

    private void UpdateRendering() {
        for (int i = 0; i < ALLOCATED_CHUNKS; i++) {
            System.ArraySegment<Matrix4x4> segment = new System.ArraySegment<Matrix4x4>(transforms, i * MAXIMUM_INSTANCED_TRANSFORMS, (i + 1) * MAXIMUM_INSTANCED_TRANSFORMS);
            Graphics.DrawMeshInstanced(mesh, 0, material, segment.Array);
        }
    }
}
