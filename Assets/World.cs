using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class World : MonoBehaviour
{
    public static World Instance { get; private set; }
    public static Grid grid;
    public static Tilemap tilemap;
    static Color highlightColor = new Color(1f,1f,0f,1f);

    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There is more than one instance!");
            return;
        }
        Instance = this;
        grid = gameObject.GetComponent<Grid>();
        tilemap = gameObject.GetComponentInChildren<Tilemap>();
    }

    public static void remove_tile(Vector3 pos)
    {
        Vector3Int cellPosition = grid.WorldToCell(pos);
        TileBase tile = tilemap.GetTile(cellPosition);
        if (tile != null)
        {
            tilemap.SetTile(cellPosition, null);
        }
    }

    // TODO
    public static void highlight_tile(Vector3 pos)
    {
        Vector3Int cellPosition = grid.WorldToCell(pos);
        if (tilemap.HasTile(cellPosition))
        {
            Debug.Log(highlightColor);
            tilemap.SetColor(cellPosition, highlightColor);
        }
    }

    public static Vector3Int snapToGrid(Vector3 pos)
    {
        return grid.WorldToCell(pos);
    }
          

    public static int getCost(Vector3 pos)
    {
        Vector3Int cellPosition = grid.WorldToCell(pos);
        Sprite sprite = tilemap.GetSprite(cellPosition);
        if (sprite != null && sprite.name == "wall")
        {
            return 9999;
        }
        return 0;
    }

    public static bool cell_walkable(Vector3 pos)
    {
        Vector3Int cellPosition = grid.WorldToCell(pos);
        Sprite sprite = tilemap.GetSprite(cellPosition);
        if (sprite != null && sprite.name == "wall")
        {
            return false;
        }
        return true;
    }
}
