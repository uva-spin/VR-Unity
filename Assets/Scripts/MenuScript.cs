using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuScript : MonoBehaviour
{
    public GameObject menu;
    public GameObject[] menuTabs = new GameObject[2];
    private int currentTab = 0;
    private int currentSubTab = 0;
    public GameObject speedSlider;
    public GameObject trailWidthSlider;
    public Slider sliderComp, trailWidths, xSlider, q2Slider;
    public GameObject openMenuButton;
    public GameObject closeMenuButton;

    public TMP_Text seaCounter;
    public TMP_Text comTracker;

    public TMP_Text[] texts;
    public TMP_InputField[] valueInputs;

    public GameObject quark1;
    public GameObject quark2;
    public GameObject quark3;

    Quark quark1comp;
    Quark quark2comp;
    Quark quark3comp;

    public bool menuActive = false;
    public bool openMenuButtonActive = true;
    public bool closeMenuButtonActive = false;

    public CSVPhysics csvPhysics;

    FluxTube fluxTube;

    // Boolean tracker to check if the old polarized model is turned on
    private bool polarized = false;

    private string[][][] valueFieldText = {
        new string[][]{ // Position
            new string[] {"Quark 1 Position", "X<sub>0</sub>", "Y<sub>0</sub>", "Z<sub>0</sub>" },
            new string[] {"Quark 2 Position", "X<sub>0</sub>", "Y<sub>0</sub>", "Z<sub>0</sub>" },
            new string[] {"Quark 3 Position", "X<sub>0</sub>", "Y<sub>0</sub>", "Z<sub>0</sub>" }
        },
        new string[][]{ // Velocity
            new string[] {"Quark 1 Velocity", "V<sub>x</sub>", "V<sub>y</sub>", "V<sub>z</sub>" },
            new string[] {"Quark 2 Velocity", "V<sub>x</sub>", "V<sub>y</sub>", "V<sub>z</sub>" },
            new string[] {"Quark 3 Velocity", "V<sub>x</sub>", "V<sub>y</sub>", "V<sub>z</sub>" }
        },
        new string[][]{ // Misc
            new string[] {"Quark Masses", "m<sub>1</sub>", "m<sub>2</sub>", "m<sub>3</sub>" },
            new string[] {"Model Values", "R", "K", "m<sub>c</sub>" },
            new string[] {"Extras", "#", "dim", "rot" }
        }
    };

    // Using nullables (float?) so we can detect unset values
    private float?[][] vals = {
        new float?[] {null, null, null, null, null, null, null, null, null},
        new float?[] {null, null, null, null, null, null, null, null, null},
        new float?[] {null, null, null, null, null, null, null, null, null}
    };

    void Start()
    {
        sliderComp = speedSlider.GetComponent<Slider>();
        trailWidths = trailWidthSlider.GetComponent<Slider>();
        quark1comp = quark1.GetComponent<Quark>();
        quark2comp = quark2.GetComponent<Quark>();
        quark3comp = quark3.GetComponent<Quark>();

        foreach (GameObject t in menuTabs)
            t.SetActive(false);

        SwitchTab(currentTab);
        fluxTube = FindObjectOfType<FluxTube>();
    }

    void Update()
    {
        // Control global time scale with speed slider
        Time.timeScale = sliderComp.value;

        // Update trail widths
        quark1comp.trailWidth = trailWidths.value;
        quark2comp.trailWidth = trailWidths.value;
        quark3comp.trailWidth = trailWidths.value;

        // Toggle menu visibility
        menu.SetActive(menuActive);

        // Toggle open/close menu button visibility
        if (openMenuButtonActive)
        {
            openMenuButton.SetActive(true);
            closeMenuButton.SetActive(false);
        }
        else
        {
            openMenuButton.SetActive(false);
            closeMenuButton.SetActive(true);
        }

        // Update seaquark parameters
        float x = -xSlider.value;
        float q2 = q2Slider.value;

        SeaQuarkSpawner sqs = FindObjectOfType<SeaQuarkSpawner>();
        sqs.xDegree = x;
        sqs.q2 = q2;

        // Adjust flux tube width
        if (fluxTube != null)
            fluxTube.fluxTubeWidth = 0.5f + Mathf.Min(5f / q2, 3.0f);

        // ---------------------------------------------------------------
        // ------------ NEW PART: Revolve Around X, Y, and Z -------------
        // ---------------------------------------------------------------
        if (protonTransform != null && pivotTransform != null)
        {
            // 1) Convert each slider's 0–1 value into a revolve speed (degrees/sec)
            float revolveSpeedX = revolveSliderX.value * maxRevolveSpeed;
            float revolveSpeedY = revolveSliderY.value * maxRevolveSpeed;
            float revolveSpeedZ = revolveSliderZ.value * maxRevolveSpeed;

            // 2) RotateAround for each axis
            // X-axis revolve (using pivotTransform.right)
            protonTransform.RotateAround(
                pivotTransform.position,
                pivotTransform.right,
                revolveSpeedX * Time.deltaTime
            );

            // Y-axis revolve (using pivotTransform.up)
            protonTransform.RotateAround(
                pivotTransform.position,
                pivotTransform.up,
                revolveSpeedY * Time.deltaTime
            );

            // Z-axis revolve (using pivotTransform.forward)
            protonTransform.RotateAround(
                pivotTransform.position,
                pivotTransform.forward,
                revolveSpeedZ * Time.deltaTime
            );
        }
        // ---------------------------------------------------------------
        // ------------------ END OF NEW PART ----------------------------
        // ---------------------------------------------------------------
    }

    private void LateUpdate()
    {
        // Update slider label for xSlider
        if (xSlider.isActiveAndEnabled)
        {
            xSlider.GetComponentInChildren<SliderNum>()
                   .GetComponent<TextMeshProUGUI>()
                   .text = "10<sup>" + -xSlider.value + "</sup>";

            seaCounter.text = "Seaquark Pairs: " + FindObjectOfType<SeaQuarkSpawner>().getPairCount();
        }

        // Display COM
        comTracker.text = "COM: [" + FindObjectOfType<CartesianModel>().centerOfMass.position + "]";
    }

    public void openVirtualKeyboard(int valIndex)
    {
        TouchScreenKeyboard.Open(vals[currentSubTab][valIndex] + "");
    }

    public void MenuShow()
    {
        menuActive = true;
        closeMenuButtonActive = true;
        openMenuButtonActive = false;
    }
    public void MenuHide()
    {
        menuActive = false;
        closeMenuButtonActive = false;
        openMenuButtonActive = true;
    }

    // New Method to Call OpenFile from CSVPhysics
    public void OpenCSVFileFromButton()
    {
        if (csvPhysics != null)
        {
            csvPhysics.OpenFile();
        }
        else
        {
            Debug.LogError("CSVPhysics script is not assigned in the MenuScript.");
        }
    }

    public void SwitchTab(int tab)
    {
        if (menuTabs.Length <= tab)
        {
            Debug.LogError("Tab " + tab + " is outside the range of tabs.");
            return;
        }
        if (!menuTabs[tab])
        {
            Debug.LogError("Tab " + tab + " does not exist or is null.");
            return;
        }
        menuTabs[currentTab].SetActive(false);
        menuTabs[tab].SetActive(true);

        currentTab = tab;
    }

    public void SwitchSubTab(int subtab)
    {
        if (valueFieldText.Length <= subtab)
        {
            Debug.LogError("Subtab " + subtab + " is outside the range of tabs.");
            return;
        }
        for (int i = 0; i < texts.Length; i++)
        {
            texts[i].text = valueFieldText[subtab][i][0];
            for (int j = 0; j < valueFieldText[subtab][i].Length - 1; j++)
            {
                valueInputs[i * texts.Length + j].placeholder.GetComponent<TMP_Text>().text
                    = valueFieldText[subtab][i][j + 1];
                float? v = vals[subtab][i * texts.Length + j];
                valueInputs[i * texts.Length + j].text = v.HasValue ? v.Value + "" : "";
            }
        }

        currentSubTab = subtab;
    }

    public int GetTab() { return currentTab; }

    public void SetValue(int index)
    {
        if (index > vals[0].Length)
        {
            Debug.LogError("Input textbox " + index + " is outside the range of known textfields");
            return;
        }
        string newText = valueInputs[index].text;

        bool b = float.TryParse(newText, out float r);
        float? _null = null;
        vals[currentSubTab][index] = (b ? r : _null);

        Debug.Log(vals[currentSubTab][index]);
    }

    public void Restart()
    {
        FindObjectOfType<CartesianModel>().ResetState();
    }

    public void ApplyAndRestart()
    {
        FindObjectOfType<CartesianModel>().SetNewInitialAndRestart(vals[currentSubTab], GetTabAsType());
    }

    private void ResetValence()
    {
        Vector3[] returned = FindObjectOfType<CartesianModel>().ResetToDefaultAndRestart(GetTabAsType());
        for (int i = 0; i < 3; i++)
        {
            Debug.Log(returned[i]);
            for (int j = 0; j < 3; j++)
                vals[currentSubTab][i * 3 + j] = returned[i][j];
        }
        SwitchSubTab(currentSubTab);
    }

    private void ResetSea()
    {
        xSlider.value = 0;
        q2Slider.value = 1;
    }

    private void NotImplemented()
    {
        Debug.LogError("Resetting to default not implemented for tab " + currentTab + ". Please add the method to run under the method ResetDefault");
    }

    public void ResetDefault()
    {
        System.Action functionToUse = currentTab switch
        {
            1 => ResetValence,
            2 => ResetSea,
            _ => NotImplemented
        };
        functionToUse();
    }

    public void StabilizeValues()
    {
        if (currentSubTab == 2) return;
        float?[] values = vals[currentSubTab];
        float[] masses = FindObjectOfType<CartesianModel>().m;

        for (int i = 0; i < 3; i++)
        {
            int indexToSolve = 2;
            float value = 0;
            for (int j = 0; j < 3; j++)
            {
                if (values[i + j * 3] == null)
                {
                    if (indexToSolve != 2)
                    {
                        values[i + j * 3] = 0;
                    }
                    else
                    {
                        indexToSolve = j;
                    }
                }
                else
                {
                    if (j == 2) continue;
                    value -= values[i + j * 3].Value * masses[j];
                }
            }
            values[i + indexToSolve * 3] = value;
        }

        SwitchSubTab(currentSubTab);
    }

    private CartesianModel.ValueType GetTabAsType()
    {
        return currentSubTab switch
        {
            0 => CartesianModel.ValueType.POSITION,
            1 => CartesianModel.ValueType.VELOCITY,
            2 => CartesianModel.ValueType.MISC,
            _ => throw new System.IndexOutOfRangeException()
        };
    }

    public void OldModelButton(bool item)
    {
        quark1.GetComponent<Quark>().enabled = item;
        quark2.GetComponent<Quark>().enabled = item;
        quark3.GetComponent<Quark>().enabled = item;

        quark1.GetComponent<ChangeOrbit>().enabled = item;
        quark2.GetComponent<ChangeOrbit>().enabled = item;
        quark3.GetComponent<ChangeOrbit>().enabled = item;
        polarized = item;
    }

    public bool getPolarized()
    {
        return polarized;
    }

    // ---------------------------------------------------------------
    // -------------- NEW PART: Revolve around X, Y, Z ---------------
    // ---------------------------------------------------------------
    [Header("Proton Revolution Sliders")]
    public Slider revolveSliderX;  // Assign in Inspector for X-axis revolve
    public Slider revolveSliderY;  // Assign in Inspector for Y-axis revolve
    public Slider revolveSliderZ;  // Assign in Inspector for Z-axis revolve

    [Header("Proton to Revolve")]
    public Transform protonTransform; // The object that will revolve

    [Header("Pivot for Revolution")]
    public Transform pivotTransform;  // The center (pivot) around which protonTransform will revolve

    [Header("Revolution Speed")]
    [Tooltip("Maximum degrees per second when slider is at 1.")]
    public float maxRevolveSpeed = 180f;
    // ---------------------------------------------------------------
    // ------------------ END OF NEW PART ----------------------------
    // ---------------------------------------------------------------
}