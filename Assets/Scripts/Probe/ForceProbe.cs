using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceProbe : TypeQuark
{

    private void Update()
    {

        Keyframe key = new Keyframe();
        key.value = .1f;
        if (!FindObjectOfType<AllQuarkScript>().forceLineVis) {
            key.value = 0;
        }

        GetComponent<LineRenderer>().widthCurve = new AnimationCurve(new Keyframe[] { key, key }); //Sets the width curve to 0 (you probably could just remove it entirely, but it might cause problems)
    }

}
