using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class Tile : MonoBehaviour
{
    public string tileType;
    [SerializeField] private SpriteRenderer spr;

    [SerializeField] private GameObject highlight;
    // [SerializeField] private GameObject selection;

    public TileBase ruleTile;


    private void OnMouseEnter() {
        highlight.SetActive(true);
    }

    private void OnMouseExit() {
        highlight.SetActive(false);
    }
}
