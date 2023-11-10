using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public LayerMask BoardLayer;
    Ray ray;

    public Transform tempRep;

    List<Transform> _allMoveRepresenters = new List<Transform>();

    Transform _selectedTile;
    PieceType _occupant;

    BoardManager _boardManager;

    public List<int> PossibleMoves = new List<int>();

    void Start()
    {
        _boardManager = FindObjectOfType<BoardManager>();
    }

    void Update()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Input.GetMouseButtonDown(0)) return;
        
        //PLEASE FIX THIS SOON IT IS SO BAD AND MESSY 
        if (Physics.Raycast(ray.origin, ray.direction, out var tile, 10000, BoardLayer))
        {
            var previousTile = _selectedTile;
            var previousOccupant = _occupant;

            _selectedTile = tile.transform;
            
            var index = _boardManager.Tiles.FindIndex(t => t.transform == _selectedTile.transform);
            _occupant = _boardManager.ReturnOccupant(
                _boardManager.Tiles.FindIndex(t => t.transform == _selectedTile.transform));
            
            var newMoves = _boardManager.ShowPossibleMoves(
                index,
                _boardManager.ReturnOccupant(
                    _boardManager.Tiles.FindIndex(t => t.transform == _selectedTile.transform)));
            Debug.Log($"Occupied by: {_occupant}");
            
            if (previousOccupant != PieceType.None)
            {
                if (_occupant == PieceType.None || _boardManager.BitBoards[previousOccupant].Item2 !=
                    _boardManager.BitBoards[_occupant].Item2)
                {
                    if (PossibleMoves.Count > 0)
                    {
                        if (PossibleMoves.Contains(index))
                        {
                            var prevIndex = _boardManager.Tiles.FindIndex(t => t.transform == previousTile.transform);

                            _boardManager.TileInfo[index] = _boardManager.TileInfo[prevIndex];
                            _boardManager.TileInfo[prevIndex] = null;

                            if(_boardManager.TileInfo[index]) _boardManager.TileInfo[index].position = _selectedTile.position;

                            var boardInfo = _boardManager.BitBoards[previousOccupant];

                            boardInfo.Item1 |= 1UL << index;
                            boardInfo.Item1 &= ~(1UL << prevIndex);
                            
                            _boardManager.BitBoards[previousOccupant] = boardInfo;
                            
                            _boardManager.SetOccupancy(previousOccupant, index);
                            _boardManager.SetOccupancy(PieceType.None, prevIndex);
                            
                            newMoves.Clear();
                        }
                    }
                }
                //first we want to check if we are clicking on a tile with the same color as our previously selected tile DONE
                //if there is we want to deselect the previous tile and select that one

                //if not:
                //if we have a tile that we have already selected we want to check if there were legal moves
                //if there are legal moves we want to check that the tile we are currently hitting is the index of one of the legal moves
                //if it is we want to place the piece to that tile and update the corresponding bitboard by placing the new index bit to 1 and the-
                //previous index bit to 0
                //if there was another piece on the new index we want to remove it and update the corresponding bitboard
            }

            PossibleMoves = newMoves;
            
            foreach (var VARIABLE in _allMoveRepresenters)
                Destroy(VARIABLE.gameObject);

            _allMoveRepresenters.Clear();

            foreach (var VARIABLE in PossibleMoves)
            {
                Debug.Log(VARIABLE);
                _allMoveRepresenters.Add(Instantiate(tempRep, _boardManager.Tiles[VARIABLE].position,
                    Quaternion.identity));
            }
        }
        else _selectedTile = null;
    }

    //Debug.Log($"Tile index: {_boardManager.Tiles.FindIndex(t => t.transform == _selectedTile.transform)}");
    //Debug.Log($"Is occupied: {_boardManager.IsOccupied(_boardManager.Tiles.FindIndex(t => t.transform == _selectedTile.transform))}");
}