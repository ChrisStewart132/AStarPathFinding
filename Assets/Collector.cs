using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collector : MonoBehaviour
{
    void FixedUpdate()
    {
        if (World.getCost(transform.position) < 0)
        {
            World.remove_tile(transform.position);
        }
    }
}
