using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Music : MonoBehaviour
{
    AudioSource source;
    public AudioClip[] intros;
    public AudioClip[] loops;
    public AudioClip[] fulls;
    public bool autoPlay = false;

    private void Awake()
    {
        source = gameObject.AddComponent<AudioSource>();
        if (autoPlay && (loops.Length > 0 && intros.Length > 0)) {
            source.clip = loops[0];
            source.PlayOneShot(intros[0]);
            source.loop = true;
            source.PlayScheduled(intros[0].length + AudioSettings.dspTime); 
        }
    }
}
