using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MovementController : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonUp(1))
        {
            Vector2 mouseOnScreen = (Vector2)Camera.main.ScreenToViewportPoint(Input.mousePosition);
            Vector2 mouseWorldPosition = (Vector2)Camera.main.ViewportToWorldPoint(mouseOnScreen);
            gameObject.GetComponent<MovementCommand>().move_to_target(mouseWorldPosition);
        }
    }
}
