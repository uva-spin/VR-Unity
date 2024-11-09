using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RungeKutta
{
    public static float[][] rk4(System.Func<float, float[], float[]> f, float t_start, float t_end, float dt, params float[] variables)
    {
        if (variables == null || variables.Length == 0) {
            Debug.LogError("Function variables cannot be empty or null!");
            return null;
        }

        #region Calculate stepsize and starting point
        int n_steps = Mathf.FloorToInt((t_end - t_start) / dt);
        int sign = +1;
        float t = t_start;

        #region If the starting point is after the ending point, set the stepsize sign flag to negative
        if (n_steps < 0) {
            sign = -1;
            n_steps *= -1;
        }
        #endregion
        #endregion

        float[][] output = new float[n_steps + 1][];
        output[0] = variables;

        #region Then, add up individual slices with widths of the stepsize to estimate the solution
        for (int i = 0; i < n_steps; i++)
        {
            float[][] k = generateKValues(f, t, sign * dt, output[i]);
            output[i + 1] = addArrays(output[i], multiplyArray(addArrays(addArrays(k[0], multiplyArray(k[1], 2.0f)), addArrays(multiplyArray(k[2], 2.0f), k[3])), sign * dt / 6.0f));
            t += sign * dt;
        }
        #endregion
        
        return output;
    }

    private static float[][] generateKValues(System.Func<float, float[], float[]> f, float t, float dt, float[] y) {
        float[] k1 = f(t, y);
        float[] k2 = f(t + 0.5f * dt, addArrays(y, multiplyArray(k1, 0.5f * dt)));
        float[] k3 = f(t + 0.5f * dt, addArrays(y, multiplyArray(k2, 0.5f * dt)));
        float[] k4 = f(t + dt, addArrays(y, multiplyArray(k3, dt)));
        return new float[][] { k1, k2, k3, k4 };
    }
    private static float[] addArrays(params float[][] arrays) {
        float[] sum = new float[arrays[0].Length];
        foreach (float[] array in arrays) {
            for (int i = 0; i < sum.Length; i++) {
                sum[i] += array[i];
            }
        }
        return sum;
    }
    private static float[] multiplyArray(float[] array, float constant) {
        float[] product = new float[array.Length];
        for (int i = 0; i < product.Length; i++) {
            product[i] = array[i] * constant;
        }
        return product;
    }
}
