using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding : MonoBehaviour
{
    // MainController: script containing a dictionary<Vector2, Tile> to check 2D Tiles for constructs
    private MainController controller;

    private void Awake()
    {       
        controller = GameObject.Find("Controller").GetComponent<MainController>();
        GenericSearch.controller = controller;
    }

    // returns an optimal path to the target within some constraints
    public List<Arc> path(Vector3 start, Vector3 goal)
    {
        Graph graph = new Grid2DGraph(controller.snapToGrid(start), controller.snapToGrid(goal));
        Frontier frontier = new AFrontier(goal);
        List<List<Arc>> solutions = GenericSearch.genericSearch(graph, frontier);
        List<Arc> solution;

        if (solutions.Count > 0)
        {
            solution = solutions[0];
        }
        else // no solution returned (algorithm halted prematurely)
        {
            solution = new List<Arc>();
            solution.Add(new Arc(new Vector3(0,0,0), start, -1, 0));
            solution.Add(new Arc(start, goal, -1, 0));// couldn't find path (depth of search too high), so just assume path from start->target
        }
        
        return solution;
    }

    public void drawPath(List<Arc> path, int index)
    {    
        for(int i = Mathf.Max(1,index); i < path.Count; i ++)
        {
            Arc arc = path[i];
            Vector3 tail = new Vector3(arc.tail.x, arc.tail.y, 0);
            Vector3 head = new Vector3(arc.head.x, arc.head.y, 0);
            Debug.DrawLine(tail, head);         
        }
    }
}


public class Heap
{
    List<object> items = new List<object>();
    List<float> costs = new List<float>();

    public Heap()
    {
        items.Add(new List<Arc>());// heap starts at index 1
        costs.Add(999999999);
    }

    public bool isEmpty()
    {
        return items.Count <= 1;
    }

    public void insert(float cost, object path)
    {       
        costs.Add(cost);
        items.Add(path);
        sift_up(items.Count - 1);// raise added path to correct level
    }

    void sift_up(int i)
    {
        int parent = i / 2;
        // While we haven't reached the top of the heap, and its parent is
        // smaller than the item       
        if (i > 1 && cost(i) < cost(parent))
        {
            // Swap the item and its parent
            swap(i, parent);
            // Carry on sifting up from the parent index
            sift_up(parent);
        }
        // else no more sifting needed as didn't swap.
    }

    void sift_down(int i)
    {
        // While the item at 'index' has at least one child...
        if (i * 2 + 1 < items.Count) {
            int left = 2 * i;
            int right = left + 1;
            int smallest = left;
            float smallestCost = cost(left);
            if (cost(right) < cost(left))
            {
                smallest = right;
                smallestCost = cost(right);
            }

            if (cost(i) > smallestCost) {
                swap(i, smallest);
                sift_down(smallest);
            }
        }
    }

    void swap(int x, int y)
    {
        object item = items[x];       
        items[x] = items[y];      
        items[y] = item;
        float cost = costs[x];
        costs[x] = costs[y];
        costs[y] = cost;
    }

    float cost(int i)
    {
        return costs[i];
    }

    public object peek_min()
    {
        return items[1];
    }

    public object pop_min()
    {
        if (isEmpty())
            return null;

        // pop min
        object min = peek_min();
        
        swap(1, items.Count - 1);
        items.RemoveAt(items.Count - 1);
        costs.RemoveAt(costs.Count - 1);

        // siftUp the new root
        sift_down(1);
        sift_down(1);

        return min;
    }

    public bool validate()
    {
        float min_value = cost(1);
        for (int i = 1; i < items.Count; i++) {
            if(cost(i) < min_value)
                return false;
        }
        return true;
    }

    public List<object>  getItems()
    {
        return items;
    }
}

// Arc represents a path between two nodes (tail->head with an action(direction code) and cost)
public struct Arc
{
    public Vector3 tail, head;
    public int action, cost;
    public Arc(Vector3 tail, Vector3 head, int action, int cost)
    {
        this.tail = tail;
        this.head = head;
        this.action = action;// 0 == x+, 1 == x-, 2 == y+, 3 == y-, -1 == other_action
        this.cost = cost;
    }
}

public abstract class Graph
{
    // confirms if the given node is a goal node and therefore the end of a path
    public abstract bool isGoal(Vector3 node);

    // returns a list of possible starting_nodes (would be a single node for a single AI searching)
    public abstract List<Vector3> startingNodes();

    // returns a list of neighbouring arcs from the given node to its neighbours
    public abstract List<Arc> outgoingArcs(Vector3 node);
}


// nodes are x,y coordinates, neighbouring nodes are above and to the side (no diagonal movement)
public class Grid2DGraph : Graph
{
    Vector3 startingNode, goalNode;
    public Grid2DGraph(Vector3 startingNode, Vector3 goalNode)
    {
        this.startingNode = startingNode;
        this.goalNode = goalNode;
    }

    public override bool isGoal(Vector3 node)
    {
        return node.x == goalNode.x && node.y == goalNode.y;
    }

    public override List<Arc> outgoingArcs(Vector3 node)
    {
        List<Arc> arcs = new List<Arc>();
        arcs.Add(new Arc(node, new Vector3(node.x + 1f, node.y), 0,  1));
        arcs.Add(new Arc(node, new Vector3(node.x - 1f, node.y), 1, 1));
        arcs.Add(new Arc(node, new Vector3(node.x, node.y + 1f), 2, 1));
        arcs.Add(new Arc(node, new Vector3(node.x, node.y - 1f), 3, 1));
        return arcs;
    }

    public override List<Vector3> startingNodes()
    {        
        return new List<Vector3> { startingNode };
    }
}


// Frontier is an enumerable collection containing possible arcs on the frontier of the search,
// adds new arcs as the search goes on
public abstract class Frontier
{
    public abstract bool isEmpty();

    // adds a path to the frontier
    public abstract void add(List<Arc> path);

    // removes and returns the next path from the frontier
    public abstract List<Arc> next();
}

public class AFrontier : Frontier
{
    HashSet<Vector3> nodesExpanded = new HashSet<Vector3>();
    Heap container = new Heap();
    Vector3 goal;
    public AFrontier(Vector3 goal)
    {
        this.goal = goal;
    }

    public override bool isEmpty()
    {
        return container.isEmpty();
    }

    float estimatedCost(Vector3 start)
    {
        float cost = 0;
        cost += Mathf.Abs(start.x - goal.x);
        cost += Mathf.Abs(start.y - goal.y);
        return cost;
    }

    public override void add(List<Arc> path)
    {
        Vector3 finalNode = path[path.Count - 1].head;
        if (!nodesExpanded.Contains(finalNode))// only add paths whos last node has not been expanded
        {
            float totalPathCost = 0;
            foreach (Arc arc in path)
            {
                totalPathCost += arc.cost;
            }
            container.insert(totalPathCost + estimatedCost(finalNode), path);
        }
    }

    public override List<Arc> next()
    {
        if (!isEmpty())
        {
            List<Arc> nextPath = (List<Arc>)container.pop_min();
            Vector3 finalNode = nextPath[nextPath.Count - 1].head;
            while (nodesExpanded.Contains(finalNode) && !isEmpty())
            {
                nextPath = (List<Arc>)container.pop_min();
                finalNode = nextPath[nextPath.Count - 1].head;
            }
            nodesExpanded.Add(finalNode);
            return nextPath;
        }
        else
        {
            return null;
        }
    }
}

// searches the given graph(with start and goal nodes) and frontier(specifying the search method) and returns a path
public static class GenericSearch
{
    public static MainController controller;
    public static List<List<Arc>> genericSearch(Graph graph, Frontier frontier)
    {
        // list of paths (from starting_node -> goal_node) to return 
        List<List<Arc>> calculatedPath = new List<List<Arc>>();

        // initializes the frontier to contain all starting nodes
        foreach(Vector3 startingNode in graph.startingNodes())
        {
            Vector3 nullNode = new Vector3(0,0,0);
            Arc arc = new Arc(nullNode, startingNode, -1, 0);
            List<Arc> path = new List<Arc>();
            path.Add(arc);
            frontier.add(path);
        }

        int depth = 0;
        const int MAX_DEPTH = 1024;
        while(!frontier.isEmpty() && depth < MAX_DEPTH)
        {
            depth++;

            List<Arc> path = frontier.next();
            Vector3 nodeToExpand = path[path.Count - 1].head;// get last node from path 

            if(graph.isGoal(nodeToExpand)) {
                calculatedPath.Add(path);
                break;// path found so stop searching, else the algorithm won't halt until the frontier has no more neighbours
            }

            List<Arc> neighbours = graph.outgoingArcs(nodeToExpand);
            for(int i = 0; i < neighbours.Count; i++)
            {
                Arc arc = neighbours[i];
                List<Arc> newPath = new List<Arc>(path);// copy of current path to expand

                // check for obstructions on the tile
                if (controller.getTile(new Vector3(arc.head.x, arc.head.y, 0)).construct != null)
                {
                    const int constructPathingCost = 200;
                    arc.cost += constructPathingCost;// allow pathfinding through buildings, but with increased cost
                }
              
                // check if previous arc in path has same direction (and therefore adjust so tail1->head1, tail2->head2 = tail1->head2 with cost=2
                // Arc includes a direction/action integer    
                
                Arc prev = path[path.Count - 1];
                if (prev.action == arc.action)
                {
                    arc.tail = prev.tail;
                    arc.cost += prev.cost;

                    newPath.RemoveAt(newPath.Count - 1);// remove prev arc
                }

                newPath.Add(arc);
                frontier.add(newPath);
            }
           
        }
        return calculatedPath;
    }
}