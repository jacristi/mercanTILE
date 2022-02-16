using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int width, height;
    [SerializeField] private float tileSize;
    [SerializeField] private Tile blankTile;
    [SerializeField] private Tile[] tilePrefabs;
    [SerializeField] private Tilemap groundTileMap;

    [SerializeField] Transform cam;

    private Dictionary<Vector2, Tile> tilesDict;

    private Tile[,] tileArray;

    private void Start()
    {
        GenerateGrid();
        RenderMap();
    }

    public Tile PickRandomTile() {
        return tilePrefabs[Random.Range(0, tilePrefabs.Length)]; // if it's an array
    }

    void GenerateGrid()
    {   Debug.Log("Generating Grid");
        tileArray = new Tile[width, height];

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                Tile tileToSpawn = blankTile;
                if (!(x == 0 || x == width-1 || y <= 1 || y >= height-2)) {
                    tileToSpawn = PickRandomTile();
                }
                tileArray[x,y] = tileToSpawn;
            }
        }
    }

    void GenerateRoads() {
        // Take in a grid /2d array and populate road tile locations
        // Return the updated grid
    }
    void RenderMap()
    {   Debug.Log("Rendering Grid");
        for (int x=0; x<tileArray.GetLength(0); x++) {
            for (int y=0; y<tileArray.GetLength(1); y++) {
                Tile thisTile = tileArray[x, y];
                var tileObj = Instantiate(thisTile, new Vector3(x+.5f, y+.5f), Quaternion.identity);
                tileObj.transform.parent = this.transform;
                tileObj.name = $"{tileObj.tileType} {x},{y}";
                groundTileMap.SetTile(new Vector3Int(x, y, 0), thisTile.ruleTile);
            }
        }

        cam.transform.position = new Vector3((float)width * 0.5f, (float)height * 0.5f, -10);
    }

    void GeneratePlaces() {
        // Take in a grid/2d array and populate places (starting village, starting bandit camps, etc.)
        // Return new places grid
    }

    private Tile GetTileAtPosition(Vector2 pos)
    {
        if (tilesDict.TryGetValue(pos, out var tile)) {
            return tile;
        } else {
            return null;
        }
    }
}
