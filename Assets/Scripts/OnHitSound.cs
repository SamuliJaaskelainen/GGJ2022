using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnHitSound : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        switch (Random.Range(0, 3))
        {
            case 0:
                AudioManager.Instance.PlaySound("Hit1", transform.position);
                break;
            case 1:
                AudioManager.Instance.PlaySound("Hit2", transform.position);
                break;
            case 2:
                AudioManager.Instance.PlaySound("Hit3", transform.position);
                break;
        }
    }
}
