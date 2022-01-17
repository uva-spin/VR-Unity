using PathCreation;
using UnityEngine;

public class FluxTube : MonoBehaviour
{
    [SerializeField]
    private Transform bone1;
    private Transform bone1Target;

    [SerializeField]
    private Transform bone2;
    private Transform bone2Target;

    [SerializeField]
    private Transform bone3;
    private Transform bone3Target;

    [SerializeField]
    public Transform centerBone;

    [SerializeField]
    private Transform quark1;
    private float quark1InitialDistance;
    private float quark1InitialOffset;


    [SerializeField]
    private Transform quark2;
    private float quark2InitialDistance;
    private float quark2InitialOffset;

    [SerializeField]
    private Transform quark3;
    private float quark3InitialDistance;
    private float quark3InitialOffset;

    private Transform gluonPositionTransform;
    private Material fluxTubeMaterial;

    /// <summary>
    /// Get the position of the flux tube cenetroid in World Space.
    /// </summary>
    /// <returns></returns>
    public Vector3 GetCenterPosition()
    {
        return centerBone.position;
    }

    // Start is called before the first frame update
    void Awake()
    {
        bone1Target = bone1.GetChild(0);
        quark1InitialOffset = Vector3.Distance(quark1.position, bone1Target.position);
        quark1InitialDistance = Vector3.Distance(bone1Target.position, centerBone.position);

        bone2Target = bone2.GetChild(0);
        quark2InitialOffset = Vector3.Distance(quark2.position, bone2Target.position);
        quark2InitialDistance = Vector3.Distance(bone2Target.position, centerBone.position);

        bone3Target = bone3.GetChild(0);
        quark3InitialOffset = Vector3.Distance(quark3.position, bone3Target.position);
        quark3InitialDistance = Vector3.Distance(bone3Target.position, centerBone.position);
        fluxTubeMaterial = GetComponentInChildren<Renderer>().material;
    }

    private void LateUpdate()
    {
        UpdateBonesTransforms();
        UpdateMaterial();
    }

    private void UpdateMaterial()
    {
        SetGluonPositionProperty("_Q1_Position", quark1);
        SetGluonPositionProperty("_Q2_Position", quark2);
        SetGluonPositionProperty("_Q3_Position", quark3);

        SetGluonPositionProperty("_GluonAnticolorPosition", gluonPositionTransform);
    }

    private void SetGluonPositionProperty(string gluonPositionName, Transform gluonTransform)
    {
        if (gluonTransform != null)
        {
            fluxTubeMaterial.SetVector(gluonPositionName, gluonTransform.position);
        }
        else
        {
            fluxTubeMaterial.SetVector(gluonPositionName, Vector3.positiveInfinity);
        }
    }

    private void UpdateBonesTransforms()
    {
        Vector3 centroid = (quark1.position + quark2.position + quark3.position) / 3.0f;
        Vector3 centroidNormal = Vector3.Cross(quark3.position - quark1.position, quark2.position - quark1.position);

        centerBone.position = centroid;
        centerBone.rotation = Quaternion.LookRotation(centroidNormal);


        var q1TargetPos = quark1.position - (quark1.position - centerBone.position).normalized * quark1InitialOffset;
        bone1.LookAt(q1TargetPos, centroidNormal);
        bone1.Rotate(-90, 90, 0);
        var scale1 = Vector3.Distance(centroid, q1TargetPos) / quark1InitialDistance;
        bone1.localScale = new Vector3(scale1, 1, 1);

        var q2TargetPos = quark2.position - (quark2.position - centerBone.position).normalized * quark2InitialOffset;
        bone2.LookAt(q2TargetPos, centroidNormal);
        bone2.Rotate(-90, 90, 0);
        var scale2 = Vector3.Distance(centroid, q2TargetPos) / quark2InitialDistance;
        bone2.localScale = new Vector3(scale2, 1, 1);

        var q3TargetPos = quark3.position - (quark3.position - centerBone.position).normalized * quark3InitialOffset;
        bone3.LookAt(q3TargetPos, centroidNormal);
        bone3.Rotate(-90, 90, 0);
        var scale3 = Vector3.Distance(centroid, q3TargetPos) / quark3InitialDistance;
        bone3.localScale = new Vector3(scale3, 1, 1);
    }

    public void SetGluon(Gluon gluon)
    {
        if (gluon != null)
        {
            fluxTubeMaterial.SetColor("_GluonAnticolor", QuarkSettings.GetColorRGB(gluon.AntiColor));
            gluonPositionTransform = gluon.transform;
            UpdateMaterial();
        }
        else
        {
            fluxTubeMaterial.SetColor("_GluonAnticolor", Color.clear);
            gluonPositionTransform = null;
            UpdateMaterial();
        }
    }
}
