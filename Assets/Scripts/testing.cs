using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class testing : MonoBehaviour
{
    public int width = 25;
    public int height = 18;
    public float cellSize = 1f;

    private ZoneGrid<int> zoneGrid;

    // Start is called before the first frame update
    void Start()
    {
        zoneGrid = new ZoneGrid<int>(width, height, cellSize);
    }

    private void Update() {
        if (Input.GetMouseButtonDown(0)) {
            int value = zoneGrid.GetValue(MouseExtensions.GetMouseWorldPosition());
            zoneGrid.SetValue(MouseExtensions.GetMouseWorldPosition(), value+5);
        }
        if (Input.GetMouseButtonDown(1)) {
            Debug.Log(zoneGrid.GetValue(MouseExtensions.GetMouseWorldPosition()));
        }
    }
}
