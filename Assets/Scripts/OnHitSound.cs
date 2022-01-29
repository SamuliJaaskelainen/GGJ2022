using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnHitSound : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        AudioManager.Instance.PlaySound("Hit", transform.position);
    }
}
