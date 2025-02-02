using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using SimpleFileBrowser;
using UnityEngine.UI;
using System;

public class CSVPhysics : MonoBehaviour
{
    public GameObject[] obj = new GameObject[4];
    List<Vector3[]> positions = new List<Vector3[]>();
    int LOOP_TIMEOUT = 1000000;
    float REMOVE_FROM_VIEW = 999999;
    string path = null;
    float dt = 0.001f;
    float elapsed_time = 0;
    bool follow_center = false;
    public UISkin darkmode;
    public Slider playbackSlider;
    private Vector3 camPosition = new Vector3(0, 0, -10);
    public Text info, stats;
    [SerializeField] public float playbackSpeed = 1f;

    private bool unstable = false;

    //scaling values
    public float scalingFactor = 1f;

    // Start is called before the first frame update
    void Start()
    {
        FileBrowser.Skin = darkmode;
    }

    // Update is called once per frame
    void Update()
    {
        if (info != null) info.text = "Data Loaded: " + (path == "" || path == null ? "None" : path) + "\n\nElapsed Time: " + elapsed_time.ToString("0.000") + " (dt: " + dt + ")";
        if (positions.Count == 0) return;
        elapsed_time += Time.deltaTime * playbackSpeed;
        if (elapsed_time < 0) { elapsed_time += positions.Count * dt; }
        int index = Mathf.FloorToInt(elapsed_time / dt) % positions.Count;
        if (index < 0) { index = positions.Count - 1; }
        for (int i = 0; i < 4; i++)
            obj[i].transform.localPosition = Vector3.Lerp(
                positions[index][i] * scalingFactor,
                positions[(index + 1) % positions.Count][i] * scalingFactor,
                (elapsed_time - Mathf.Floor(elapsed_time)) / dt
            );
        if (playbackSlider != null) playbackSlider.value = index;
    }

    Vector3 GetCenterAtTime(int t)
    {
        return 1 / 3f * (positions[t][0] + positions[t][1] + positions[t][2]);
    }

    Vector3 getCenter()
    {
        return 1 / 3f * (obj[0].transform.position + obj[1].transform.position + obj[2].transform.position);
    }

    public void FollowCenter()
    {
        follow_center = !follow_center;
    }

    //
    //TODO: We need a button to run this function. This should open a popup file browser you can use to select a csv file
    //There should be a folder called CSV_file outside of the asset folder with a couple example files you can use
    //
    public void OpenFile()
    {
        SimpleFileBrowser.FileBrowser.ShowLoadDialog(OnSuccess, OnCancel, FileBrowser.PickMode.Files, title: "Select CSV file");
    }

    //
    //TODO: We also need a button to disable the CSV model, and instead run the CartesianModel.cs script
    //Done!

    public void OnSuccess(string[] paths)
    {
        if (paths != null && paths.Length > 0 && paths[0] != "")
        {
            path = paths[0];
            ReadData();
        }
    }

    public void OnCancel() { } //Unused

    public void ChangeTime()
    {
        if (playbackSlider == null) return;
        elapsed_time = dt * playbackSlider.value;
    }


    //
    //TODO: You can use this function to disable the Cartesian Model as this will run only after we read in a csv file
    //didn't need to use this, so I think it's done
    public void ResetPlayback()
    {
        elapsed_time = 0;
    }

    void ReadData()
    {
        if (path == null) return;
        unstable = false;
        StreamReader reader = new StreamReader(path);
        reader.ReadLine();
        positions = new List<Vector3[]>();

        float[] times = { 0, 0 };
        string lastData = "";

        for (int timeout = 0; timeout < LOOP_TIMEOUT; timeout++)
        {
            string data = reader.ReadLine();
            if (data == null) break;
            Vector3[] vec = GetVector(data);
            if (vec != null) positions.Add(vec);

            if (timeout == 0) times[0] = GetTime(data);
            lastData = data;
        }
        reader.Close();
        times[1] = GetTime(lastData);

        dt = (times[1] - times[0]) / positions.Count;

        for (int i = 0; i < 4; i++)
            obj[0].transform.position = positions[0][i];
        if (playbackSlider != null) playbackSlider.maxValue = positions.Count;
        if (stats != null) stats.text = "Drift: " + (GetCenterAtTime(positions.Count - 1) - GetCenterAtTime(0)) / (positions.Count * dt) + "\n\n" + (unstable ? "Unstable" : "Stable");
        ResetPlayback();
    }

    float GetTime(string data)
    {
        return float.Parse(data.Split(',')[0]);
    }

    float parseFloat(string value)
    {
        if (value.Equals("nan"))
        {
            unstable = true;
            return float.NaN;
        }
        else
        {
            try
            {
                return float.Parse(value);
            }
            catch
            {
                unstable = true;
                return float.NaN;
            }
        }
    }

    string[] separateString(string data)
    {
        string[] vals = data.Split(',');
        if (vals.Length < 13)
        {
            string[] v = new string[13];
            for (int i = 0; i < v.Length; i++)
            {
                v[i] = (i < vals.Length) ? vals[i] : REMOVE_FROM_VIEW + "";
            }
            return v;
        }
        return vals;
    }

    Vector3[] GetVector(string data)
    {
        string[] vals = separateString(data);

        return new Vector3[] {
        new Vector3(parseFloat(vals[1]) * scalingFactor, parseFloat(vals[2]) * scalingFactor, parseFloat(vals[3]) * scalingFactor),
        new Vector3(parseFloat(vals[4]) * scalingFactor, parseFloat(vals[5]) * scalingFactor, parseFloat(vals[6]) * scalingFactor),
        new Vector3(parseFloat(vals[7]) * scalingFactor, parseFloat(vals[8]) * scalingFactor, parseFloat(vals[9]) * scalingFactor),
        new Vector3(parseFloat(vals[10]) * scalingFactor, parseFloat(vals[11]) * scalingFactor, parseFloat(vals[12]) * scalingFactor)
        };
    }

    public void SetScalingFactor(float scale)
    {
        scalingFactor = scale;
    }
}
