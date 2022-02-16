using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager2 : MonoBehaviour
{
    [SerializeField] private int width, height;
    [SerializeField] private float tileSize;
    [SerializeField] private Tile blankTile;
    [SerializeField] private Tile[] tilePrefabs;
    [SerializeField] private Tilemap groundTileMap;

    [SerializeField] Transform cam;

    private Dictionary<Vector2, Tile> tilesDict;

    private void Start() => GenerateGrid();

    public Tile PickRandomTile() {
        return tilePrefabs[Random.Range(0, tilePrefabs.Length)]; // if it's an array
    }

    void GenerateGrid()
    {
        tilesDict = new Dictionary<Vector2, Tile>();
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                Tile spawnedTile = null;
                if (x == 0 || x == width-1 || y <= 1 || y >= height-2) {
                    spawnedTile = Instantiate(blankTile, new Vector3(x+.5f, y+.5f), Quaternion.identity);
                } else {
                    spawnedTile = Instantiate(PickRandomTile(), new Vector3(x+.5f, y+.5f), Quaternion.identity);
                }
                spawnedTile.name = $"{spawnedTile.tileType} {x},{y}";
                groundTileMap.SetTile(new Vector3Int(x, y, 0), spawnedTile.ruleTile);
                tilesDict[new Vector2(x, y)] = spawnedTile;
            }
        }

        cam.transform.position = new Vector3((float)width * 0.5f, (float)height * 0.5f, -10);
    }

    void GenerateRoads() {
        // Take in a grid /2d array and populate road tile locations
        // Return the updated grid
    }
    void RenderMap() {
        // Place down tile objects and set the tile on the tile map for each
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
