using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding : MonoBehaviour
{
    Color path_color = new Color(1f, 1f, 1f, 0.1f);
    int MAX_DEPTH = 4096;

    // returns an optimal path to the target within some constraints
    public Path path(Vector3 start, Vector3 goal)
    {
        Vector3Int start_cell = World.snapToGrid(start);
        Vector3Int goal_cell = World.snapToGrid(goal);
        Debug.Log(goal_cell);
        Graph graph = new Grid2DGraph(start_cell, goal_cell);
        Frontier frontier = new AFrontier(goal_cell);
        List<Path> solutions = GenericSearch.genericSearch(graph, frontier, MAX_DEPTH);
        Path solution;

        if (solutions.Count > 0)
        {
            solution = solutions[0];
        }
        else // no solution returned
        {
            solution = new Path();
            solution.insert(new Arc(new Vector3Int(0,0,0), start_cell, -1, 0));
            solution.insert(new Arc(start_cell, goal_cell, -1, 0));// couldn't find path (depth of search too high), so just assume path from start->target
        }
        //Debug.Log(solution.cost);
        return solution;
    }

    public void drawPath(Path path, int starting_index)
    {    
        
        for(int i = Mathf.Max(1, starting_index); i < path.size(); i++)
        {
            Arc arc = path.get(i);
            Debug.DrawLine(arc.tail, arc.head, path_color);    
        }
    }

    
}


public class Heap
{
    List<object> items = new List<object>();
    List<int> costs = new List<int>();

    public Heap()
    {
        items.Add(null);// heap starts at index 1
        costs.Add(0);
    }

    public bool isEmpty()
    {
        return items.Count <= 1;
    }

    public void insert(int cost, object path)
    {       
        costs.Add(cost);
        items.Add(path);
        sift_up(items.Count - 1);// raise added path to correct level
        //Debug.Log(validate());
    }

    void sift_up(int i)
    {
        int parent = i / 2;     
        if (i > 1 && cost(i) < cost(parent))
        {
            swap(i, parent);
            sift_up(parent);
        }
    }

    void sift_down(int i)
    {
        if (i >= items.Count)
            return;
        int smallest = i;
        int left = 2 * i;
        int right = left + 1;
        if (left < items.Count && cost(left) < cost(smallest))
            smallest = left;
        if (right < items.Count && cost(right) < cost(smallest))
            smallest = right;
        if(smallest != i)
        {
            swap(i, smallest);
            sift_down(smallest);    
        }
        
    }

    void swap(int x, int y)
    {
        object item = items[x];       
        items[x] = items[y];      
        items[y] = item;
        int cost = costs[x];
        costs[x] = costs[y];
        costs[y] = cost;
    }

    int cost(int i)
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

        object min = peek_min();
        
        swap(1, items.Count - 1);
        items.RemoveAt(items.Count - 1);
        costs.RemoveAt(costs.Count - 1);

        sift_down(1);
        return min;
    }

    public bool validate()
    {
        for (int i = 1; i < items.Count; i++) {
            if (2 * i < items.Count && cost(2 * i) < cost(i))
                return false;
            if (2 * i + 1 < items.Count && cost(2 * i + 1) < cost(i))
                return false;
        }
        return true;
    }

    public List<object>  getItems()
    {
        return items;
    }
}

public class Path
{
    public List<Arc> path;
    public int cost;

    public Path()
    {
        this.path = new List<Arc>();
        this.cost = 0;
    }
    public Path(Path _path)
    {
        this.path = new List<Arc>(_path.path);
        this.cost = _path.cost;
    }

    public void insert(Arc arc)
    {
        this.path.Add(arc);
        this.cost += arc.cost;
    }
    public Arc get(int i)
    {
        return path[i];
    }
    public void remove(int i)
    {
        this.cost -= get(i).cost;
        path.RemoveAt(i);
    }
    public int size()
    {
        return path.Count;
    }
    public Arc end()
    {
        return path[size()-1];
    }
}

/**
 * linked list like path used in the search algorithm and used to build a Path
 */
public class PathNode
{
    public Arc arc;
    public PathNode parent;
    public int cost=0;

    public PathNode(Arc arc)
    {
        this.parent = null;
        this.arc = arc;
        this.cost = arc.cost;
    }

    public PathNode(PathNode parent, Arc arc)
    {
        this.parent = parent;
        this.arc = arc;
        this.cost = parent.cost + arc.cost;
    }

    public static Path buildPath(PathNode tail)
    {
        Path path = new Path();
        PathNode current = tail;
        while (current != null)
        {
            path.insert(current.arc);
            current = current.parent;
        }
        path.path.Reverse();
        return path;
    }
}

public struct Arc
{
    public Vector3Int tail, head;// tail=prev, head=current
    public int action, cost;
    public Arc(Vector3Int tail, Vector3Int head, int action, int cost)
    {
        this.tail = tail;
        this.head = head;
        this.action = action;// 0 == x+, 1 == x-, 2 == y+, 3 == y-, -1 == other_action, diagonals: 4,5,6,7
        this.cost = cost;
    }
}


/**
 * defines the starting node(s), expands a current node to its neighbours, defines if a node is the goal
 * */
public abstract class Graph
{
    // confirms if the given node is a goal node and therefore the end of a path
    public abstract bool isGoal(Vector3Int node);

    // returns a list of possible starting_nodes (would be a single node for a single AI searching)
    public abstract List<Vector3Int> startingNodes();

    // returns a list of neighbouring arcs from the given node to its neighbours
    public abstract List<Arc> outgoingArcs(Vector3Int node);
}


public class Grid2DGraph : Graph
{
    Vector3Int startingNode, goalNode;
    public Grid2DGraph(Vector3Int startingNode, Vector3Int goalNode)
    {
        this.startingNode = startingNode;
        this.goalNode = goalNode;
    }

    public override bool isGoal(Vector3Int node)
    {
        return goalNode == node;
    }

    public override List<Arc> outgoingArcs(Vector3Int node)
    {
        List<Arc> arcs = new List<Arc>();

        int diagonal_cost = 14;
        int cost = 10;
        if (false)
        {
            // diagonals
            arcs.Add(new Arc(node, new Vector3Int(node.x + 1, node.y + 1, 0), 4, diagonal_cost));
            arcs.Add(new Arc(node, new Vector3Int(node.x - 1, node.y + 1, 0), 5, diagonal_cost));
            arcs.Add(new Arc(node, new Vector3Int(node.x + 1, node.y - 1, 0), 6, diagonal_cost));
            arcs.Add(new Arc(node, new Vector3Int(node.x - 1, node.y - 1, 0), 7, diagonal_cost));
        }
        

        arcs.Add(new Arc(node, new Vector3Int(node.x + 1, node.y, 0), 0, cost));
        arcs.Add(new Arc(node, new Vector3Int(node.x - 1, node.y, 0), 1, cost));
        arcs.Add(new Arc(node, new Vector3Int(node.x, node.y + 1, 0), 2, cost));
        arcs.Add(new Arc(node, new Vector3Int(node.x, node.y - 1, 0), 3, cost));

        return arcs;
    }

    public override List<Vector3Int> startingNodes()
    {        
        return new List<Vector3Int> { startingNode };
    }
}


/**
 * defines methods to add (new) and retrieve (depending on implementation) paths
 * */
public abstract class Frontier
{
    public abstract bool isEmpty();

    // adds a path to the frontier
    public abstract void add(PathNode path);

    // removes and returns the next path from the frontier
    public abstract PathNode next();
}

public class AFrontier : Frontier
{
    HashSet<Vector3Int> nodesExpanded = new HashSet<Vector3Int>();
    Heap container = new Heap();
    Vector3Int goal;
    public AFrontier(Vector3Int goal)
    {
        this.goal = goal;
    }

    public override bool isEmpty()
    {
        return container.isEmpty();
    }

    int manhattan_distance(Vector3Int start)
    {
        int cost = Mathf.Abs(start.x - goal.x);
        cost += Mathf.Abs(start.y - goal.y);
        return cost*20;
    }

    public override void add(PathNode path)
    {
        Vector3Int finalNode = path.arc.head;
        if (!nodesExpanded.Contains(finalNode))// only add paths whos last node has not been expanded
        {
            container.insert(path.cost + manhattan_distance(finalNode), path);
        }
    }

    public override PathNode next()
    {
        if (!isEmpty())
        {
            PathNode nextPath = (PathNode)container.pop_min();
            Vector3Int finalNode = nextPath.arc.head;
            while (nodesExpanded.Contains(finalNode) && !isEmpty())
            {
                nextPath = (PathNode)container.pop_min();
                finalNode = nextPath.arc.head;
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

// static class implementing the search using a Graph and Frontier
public static class GenericSearch
{
    public static List<Path> genericSearch(Graph graph, Frontier frontier, int MAX_DEPTH = 1024)
    {
        // list of paths (from starting_node -> goal_node) to return 
        List<Path> calculatedPaths = new List<Path>();

        // initializes the frontier to contain all starting nodes
        foreach(Vector3Int startingNode in graph.startingNodes())
        {
            Vector3Int nullNode = new Vector3Int(0,0,0);
            Arc arc = new Arc(nullNode, startingNode, -1, 0);
            PathNode path = new PathNode(arc);
            frontier.add(path);
        }

        int depth = 0;
        
        while (!frontier.isEmpty() && depth++ < MAX_DEPTH)
        {
            // O(log frontier.size()) pop heap
            PathNode path = frontier.next();
            Vector3Int nodeToExpand = path.arc.head;// get last node from path 

            if(graph.isGoal(nodeToExpand)) {
                calculatedPaths.Add(PathNode.buildPath(path));
                break;// path found so stop searching
            }

            // O(neighbours.Count)
            List<Arc> neighbours = graph.outgoingArcs(nodeToExpand);
            for(int i = 0; i < neighbours.Count; i++)
            {
                Arc neighbour = neighbours[i];
                neighbour.cost += World.getCost(neighbour.head);
                PathNode newPath = new PathNode(path, neighbour);

                // O(log frontier.size()) heap insert
                frontier.add(newPath);
            }
           
        }
        if(calculatedPaths.Count == 0 && !frontier.isEmpty())// no path in search so just add last path
        {
            calculatedPaths.Add(PathNode.buildPath(frontier.next()));
        }
        return calculatedPaths;
    }
}