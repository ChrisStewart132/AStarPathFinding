using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * implements the logic to move a gameObject to a target 
 * the gameObject MUST contain a Movement class implementing a move_toward function to call every fixed frame
 */
public class MovementCommand : MonoBehaviour
{
    Movement movement;
    PathFinding pathFinding;
    public Path path;
    public int pathIndex = 0;

    void Awake()
    {
        pathFinding = gameObject.GetComponent<PathFinding>();
        movement = gameObject.GetComponent<Movement>();
    }

    public void move_to_target(Vector3 target)
    {
        path = pathFinding.path(transform.position, target);
        pathIndex = 0;
    }

    public void clear_path()
    {
        path = null;
        pathIndex = 0;
        movement.stop_moving();
    }

    void FixedUpdate()
    {
        if (path != null)
        {
            pathFinding.drawPath(path, pathIndex);
            Vector3Int path_waypoint = path.get(pathIndex).head;// current path arc to move to
            if(World.snapToGrid(transform.position) == path_waypoint) 
            {
                pathIndex++;
                if (pathIndex >= path.size())//path complete
                    clear_path();
            }
            else
            {
                movement.move_toward(path_waypoint);
            }
        }
    }
}
