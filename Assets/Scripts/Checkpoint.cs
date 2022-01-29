using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    LapSystem lapSystem;
    public int index;

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if (lapSystem == null)
            {
                lapSystem = GetComponentInParent<LapSystem>();
            }
            lapSystem.HitCheckpoint(index);
        }
    }
}
