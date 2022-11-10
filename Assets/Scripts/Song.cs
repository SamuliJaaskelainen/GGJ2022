using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Song : MonoBehaviour
{
    // Song setup
    public int index; // for track identification

    // Intro loop, plays only until first boost
    public AudioSource base_loop;

    // These play depending on how many boosts are collected
    public AudioSource level_one; // starting energy / energy state after ending first+ warp
    public AudioSource level_two; //max energy before first warp 
    public AudioSource level_three; //max energy before second+ warp 

    // These play after three boosts/while warping. alternates between the two
    public AudioSource warp_1; // first warp loop
    public AudioSource warp_2; // second warp loop

    public Song(int index, AudioSource base_loop, AudioSource level_one,
        AudioSource level_two, AudioSource level_three, AudioSource warp_1,
        AudioSource warp_2) {

        this.index = index;
        this.base_loop = base_loop;
        this.level_one = level_one;
        this.level_two = level_two;
        this.level_three = level_three;
        this.warp_1 = warp_1;
        this.warp_2= warp_2;
    }
    public void Properties() {

        Debug.Log("Song: " + index);
    }

}