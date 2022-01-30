using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warp : MonoBehaviour
{
    SphereCollider sphereCollider;
    float respawnTime = 5.0f;
    float respawnTimer;
    Vector3 startScale;

    void Start()
    {
        startScale = transform.localScale;
        sphereCollider = GetComponent<SphereCollider>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && Time.time > respawnTimer)
        {
            respawnTimer = Time.time + respawnTime;
            other.transform.parent.GetComponent<Ship>().Warp();
            sphereCollider.enabled = false;
        }
    }

    void Update()
    {
        if (Time.time < respawnTimer)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, Time.deltaTime * 10.0f);
        }
        else if (!sphereCollider.enabled)
        {
            transform.localScale = startScale;
            sphereCollider.enabled = true;
        }
    }
}
