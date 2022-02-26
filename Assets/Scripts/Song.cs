using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Song : MonoBehaviour
{

    // Song setup
    public int index; // for current book-keeping/future merchandising
    public int bpm; // when we add gfx strobing to the beat and make game unplayable


    // Base audio stems. These always play
    public AudioSource base_perc_loop;
    public AudioSource base_melody_loop;

    // This plays depending on how many boost are collected
    public AudioSource energy_one; // starting energy
    public AudioSource energy_two;
    public AudioSource energy_three; //final energy 

    // This only plays while warping

    public AudioSource warp_main;
    public AudioSource warp_melody;

    public Song(int index, int bpm, AudioSource base_perc_loop,
        AudioSource base_melody_loop, AudioSource energy_one,
       AudioSource energy_two, AudioSource energy_three, AudioSource warp_main,
       AudioSource warp_melody) {

        this.index = index;
        this.bpm = bpm;
        this.base_perc_loop = base_perc_loop;
        this.base_melody_loop = base_melody_loop;
        this.energy_one = energy_one;
        this.energy_two = energy_two;
        this.energy_three = energy_three;
        this.warp_main = warp_main;
        this.warp_melody = warp_melody;
    }
    public void Properties() { 

        Debug.Log("Song: " + index);
        Debug.Log("BPM: " + bpm);   

    }

}
