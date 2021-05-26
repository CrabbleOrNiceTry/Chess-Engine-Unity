using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sound : MonoBehaviour
{
    // Start is called before the first frame update

    public AudioClip moveSound;
    private AudioSource audioSource;

    void Start()
    {
        audioSource= GetComponent<AudioSource>();
    }

    // Update is called once per frame
    public void PlayMoveSound()
    {
        audioSource.PlayOneShot(moveSound, 0.9f);
    }
}
