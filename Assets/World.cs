using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class World : MonoBehaviour
{
    public static World Instance { get; private set; }
    public static Grid grid;
    public static Tilemap tilemap;
    static int score = 0;
    static Text scoreText;
    static Color highlightColor = new Color(1f,1f,0f,1f);

    public TileBase silver_tile;


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
        scoreText = GameObject.Find("ScoreText").GetComponent<Text>();
    }

    void Start()
    {
        place_silver_tiles();
    }

    void place_silver_tiles()
    {
        int RADIUS = 45;
        for(int i = -RADIUS; i <= RADIUS; i++)
        {
            for (int j = -RADIUS; j <= RADIUS; j++)
            {
                TileBase tile = tilemap.GetTile(new Vector3Int(j, i, 0));
                if (tile == null)
                {
                    tilemap.SetTile(new Vector3Int(j, i, 0), silver_tile);
                }
            }
        }
    }

    public static void remove_tile(Vector3 pos)
    {
        Vector3Int cellPosition = grid.WorldToCell(pos);
        TileBase tile = tilemap.GetTile(cellPosition);
        if (tile != null)
        {
            score -= getCost(pos);
            update_score_text();
            tilemap.SetTile(cellPosition, null);
        }
    }

    static void update_score_text()
    {
       scoreText.text = "Score: " + score.ToString();
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
        if(sprite != null)
        {
            if (sprite.name == "wall")
            {
                return 9999;
            }
            else if (sprite.name == "silver")
            {
                return -1;
            }
            else if (sprite.name == "gold")
            {
                return -1;
            }
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
