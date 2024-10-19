using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeaComputeShaderTest : MonoBehaviour
{
    public Material material;
    public Mesh mesh;
    public ComputeShader computeShader;
    public RenderTexture renderTexture;
    private Vector3[] positions;
    private Matrix4x4[][] matrices; //Limit on the amount of instanced renders we can do is 1023 (not 1024 strangely) in one draw call, so we gotta use [][]
    public int count = 200;

    private void CreateGluon(int x, int y) {
        int val = x * count + y;
        Debug.Log(val >> 10);

        //val % 1024 == val & 1023 since 1023 is just a bunch of 1s in binary, and can be used as a bitmask
        if ((val & 1023) == 0)
            matrices[val >> 10] = new Matrix4x4[1023];
        else
        {
            Matrix4x4 matrix = new Matrix4x4();
            Vector3 position = new Vector3(x / 10f, y / 10f, Random.Range(-0.1f, 0.1f));
            matrix.SetTRS(position, Quaternion.identity, Vector3.one);
            matrices[val >> 10][(val - 1) & 1023] = matrix;
            positions[val] = position;
        }
    }

    private void CreateGluons() {
        int size = count * count;
        positions = new Vector3[size];
        matrices = new Matrix4x4[(size >> 10) + 1][];
        for (int i = 0; i < count; i++)
            for (int j = 0; j < count; j++)
                CreateGluon(i, j);
    }

    private void RandomizeGluonsGPU() {
        int vectorSize = sizeof(float) * 3;
        int THREADS = 10;

        ComputeBuffer gluonBuffer = new ComputeBuffer(positions.Length, vectorSize);
        gluonBuffer.SetData(positions);

        computeShader.SetBuffer(0, "gluons", gluonBuffer);
        computeShader.SetFloat("resolution", positions.Length);
        computeShader.Dispatch(0, positions.Length / THREADS, 1, 1);

        gluonBuffer.GetData(positions);

        for (int i = 0; i < positions.Length; i++) {
            if ((i & 1023) == 0) continue;
            matrices[i >> 10][(i - 1) & 1023].SetColumn(3, positions[i]);
        }

        for (int i = 0; i <= positions.Length >> 10; i++)
        {
            Debug.Log(matrices[i].Length);
            Graphics.DrawMeshInstanced(mesh, 0, material, matrices[i]);
        }

        gluonBuffer.Dispose();
    }

    // Start is called before the first frame update
    void Start()
    {
        CreateGluons();
    }

    // Update is called once per frame
    void Update()
    {
        RandomizeGluonsGPU();
    }
}
