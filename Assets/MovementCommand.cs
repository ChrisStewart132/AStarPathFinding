using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * implements the logic to move a gameObject to a target 
 * the gameObject MUST contain a Movement class implementing a move_toward function to call every fixed frame
 */
public class MovementCommand : MonoBehaviour
{
    EntityState state;
    Movement movement;
    PathFinding pathFinding;
    public Path path;
    public int pathIndex = 0;
    Vector3 target;// used to continue loading_path to the current target
    bool searching = false;// continue loading_path
    int MAX_SEARCHING_FRAMES = 50*60;// how many frames a target path is allowed to
    int SEARCHING_FRAMES = 0;
    
    void Awake()
    {
        state = gameObject.GetComponentInChildren<EntityState>();
        if (state == null)
        {
            Debug.LogError("EntityState component not found in children of " + gameObject.name);
        }
        state.set("idle");

        pathFinding = gameObject.GetComponent<PathFinding>();
        movement = gameObject.GetComponent<Movement>();
    }

    public void move_to_target(Vector3 target)
    {
        this.target = target;
        load_path();
    }

    void load_path()
    {
        path = pathFinding.path(transform.position, this.target);
        pathIndex = 0;
        if(path == null)
        {
            clear_path();
            searching = true;
        }
    }

    public void clear_path()
    {
        searching = false;
        path = null;
        pathIndex = 0;
        movement.stop_moving();
    }

    void FixedUpdate()
    {
        if (SEARCHING_FRAMES > MAX_SEARCHING_FRAMES)
        {
            searching = false;
        }

        if (path != null)// follow path
        {
            pathFinding.drawPath(path, pathIndex);
            Vector3Int path_waypoint = path.get(pathIndex).head;// current path arc to move to
            if(World.snapToGrid(transform.position) == path_waypoint)// waypoint reached
            {
                pathIndex++;
                if (pathIndex >= path.size())//path complete
                    clear_path();
            }
            else
            {
                state.set("moving");
                movement.move_toward(path_waypoint);
            }
        }
        else if(searching)// continue loading path
        {   
            load_path();
            state.set("searching");
            SEARCHING_FRAMES++;
        }
        else// idle
        {
            state.set("idle");
        }

        if (!searching)
        {
            SEARCHING_FRAMES = 0;
        }
    }
}
