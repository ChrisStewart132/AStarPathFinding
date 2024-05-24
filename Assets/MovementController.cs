using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MovementController : MonoBehaviour
{
    public GameObject mouse_cell;// gameobject sprite to render the current cell highlighted

    void Update()
    {
        Vector2 mouseOnScreen = (Vector2)Camera.main.ScreenToViewportPoint(Input.mousePosition);
        Vector2 mouseWorldPosition = (Vector2)Camera.main.ViewportToWorldPoint(mouseOnScreen);
        Vector3Int cell_pos = World.snapToGrid(mouseWorldPosition);

        if(mouse_cell != null )
            mouse_cell.transform.position = cell_pos;    
        
        // TODO
        //World.highlight_tile(mouseWorldPosition);


        if (Input.GetMouseButtonUp(0))
        {
            //Vector2 mouseOnScreen = (Vector2)Camera.main.ScreenToViewportPoint(Input.mousePosition);
            //Vector2 mouseWorldPosition = (Vector2)Camera.main.ViewportToWorldPoint(mouseOnScreen);
            World.remove_tile(mouseWorldPosition);

        }
        else if (Input.GetMouseButtonUp(1))
        {
            //Vector2 mouseOnScreen = (Vector2)Camera.main.ScreenToViewportPoint(Input.mousePosition);
            //Vector2 mouseWorldPosition = (Vector2)Camera.main.ViewportToWorldPoint(mouseOnScreen);
            gameObject.GetComponent<MovementCommand>().move_to_target(mouseWorldPosition);
        }


    }
}
