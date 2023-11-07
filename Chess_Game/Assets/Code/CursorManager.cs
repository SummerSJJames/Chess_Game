using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public Int64 HoveredSquare;
    public Int64 SelectedPiece;

    public LayerMask BoardLayer;
    Ray ray;

    Transform _selectedTile;

    BoardManager _boardManager;

    void Start()
    {
        _boardManager = FindObjectOfType<BoardManager>();
    }

    void Update()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // if (_selectedTile)
        // {
        //     if (Input.GetMouseButtonDown(0))
        //     {
        //         //_selectedTile.gameObject.SetActive(false);
        //         _selectedTile = null;
        //     }
        //     return;
        // }

        if (Physics.Raycast(ray.origin, ray.direction, out var piece, 10000, BoardLayer))
        {
            if (Input.GetMouseButtonDown(0))
            {
                _selectedTile = piece.transform;
                
                //Debug.Log($"Tile index: {_boardManager.Tiles.FindIndex(t => t.transform == _selectedTile.transform)}");
                Debug.Log($"Is occupied: {_boardManager.IsOccupied(_boardManager.Tiles.FindIndex(t => t.transform == _selectedTile.transform))}");
            }
        }
        
    }
}
