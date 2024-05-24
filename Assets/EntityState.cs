using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityState : MonoBehaviour
{
    SpriteRenderer[] states;
    void Awake()
    {
        states = gameObject.GetComponentsInChildren<SpriteRenderer>();
        reset();
    }

    void reset()
    {
        for (int i = 0; i < states.Length; i++)
        {
            states[i].enabled = false;
        }
    }

    public void set(string state)
    {
        reset();
        for (int i = 0; i < states.Length; i++)
        {
            if (states[i].gameObject.name == state)
                states[i].enabled = true;
        }
    }
    public string get()
    {
        for (int i = 0; i < states.Length; i++)
        {
            if (states[i].enabled)
                 return states[i].name;
        }
        return "";
    }
}
