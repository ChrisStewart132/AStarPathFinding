using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * defines methods to control a gameObjects movement
 */
public class Movement : MonoBehaviour
{
    public float maxSpeed = 1f;
    private Rigidbody2D rb;

    public void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    public void move_toward(Vector3 target)
    {
        rb.angularVelocity = 0f;
        Vector3 direction = target - transform.position;
        float d = direction.magnitude;
        if(d > maxSpeed * Time.deltaTime)
        {            
            rb.velocity = direction.normalized * maxSpeed;// full speed
        }
        else
        {
            rb.velocity = direction / Time.deltaTime;// ease to target
        }
        
    }

    public void stop_moving()
    {
        rb.angularVelocity = 0f;
        rb.velocity = Vector3.zero;
    }
}
