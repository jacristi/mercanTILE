using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class Tile : MonoBehaviour
{
    public string tileTypeVerbose;
    [SerializeField] private SpriteRenderer spr;

    [SerializeField] private GameObject highlight;
    [SerializeField] private GameObject selection;

    public enum TileType {Void, Grassland, Forest, Mountain, Water, Road}
    public TileType tileType = TileType.Void;

    public TileBase ruleTile;

    public (int, int) gridPos;

    private void Start() {
        tileTypeVerbose = GetTileTypeVerbose();
    }

    public string GetTileTypeVerbose() {
        var typeString = "__";
        switch (tileType)
        {
            case TileType.Void:
                typeString = "Void";
                break;
            case TileType.Grassland:
                typeString = "Grassland";
                break;
            case TileType.Forest:
                typeString = "Forest";
                break;
            case TileType.Mountain:
                typeString = "Mountain";
                break;
            case TileType.Water:
                typeString = "Water";
                break;
            case TileType.Road:
                typeString = "Road";
                break;
            default:
                typeString = "__";
                break;
        }
        return typeString;
    }

    private void OnMouseEnter() {
        highlight.SetActive(true);
    }

    private void OnMouseExit() {
        highlight.SetActive(false);
    }

    public void Select() {
        selection.SetActive(true);
    }
    public void Deselect() {
        selection.SetActive(false);
    }
}
