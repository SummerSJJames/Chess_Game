using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public Vector3 StartPos;
    Vector3 _currentSpawnPos;
    public GameObject[] ObjectsToSpawn;
    ulong WhitePawns;
    ulong WhiteRooks;
    ulong WhiteKnights;
    ulong WhiteBishops;
    ulong WhiteQueens;
    ulong WhiteKing;

    ulong BlackPawns;
    ulong BlackRooks;
    ulong BlackKnights;
    ulong BlackBishops;
    ulong BlackQueens;
    ulong BlackKing;

    List<ulong> AllPieces = new List<ulong>();

    ulong board;

    public ulong Board => WhitePawns | WhiteRooks | WhiteKnights | WhiteBishops | WhiteQueens | WhiteKing |
                          BlackPawns | BlackRooks | BlackKnights | BlackBishops | BlackQueens | BlackKing;

    void Awake()
    {
        WhitePawns = 0x000000000000FF00;
        WhiteRooks = 0x0000000000000081;
        WhiteKnights = 0x0000000000000042;
        WhiteBishops = 0x0000000000000024;
        WhiteQueens = 0x0000000000000008;
        WhiteKing = 0x0000000000000010;

        BlackPawns = 0x00FF000000000000;
        BlackRooks = 0x8100000000000000;
        BlackKnights = 0x4200000000000000;
        BlackBishops = 0x2400000000000000;
        BlackQueens = 0x0800000000000000;
        BlackKing = 0x1000000000000000;

        AllPieces.Add(WhitePawns);
        AllPieces.Add(WhiteRooks);
        AllPieces.Add(WhiteKnights);
        AllPieces.Add(WhiteBishops);
        AllPieces.Add(WhiteQueens);
        AllPieces.Add(WhiteKing);
        AllPieces.Add(BlackPawns);
        AllPieces.Add(BlackRooks);
        AllPieces.Add(BlackKnights);
        AllPieces.Add(BlackBishops);
        AllPieces.Add(BlackQueens);
        AllPieces.Add(BlackKing);

        // foreach (var b in AllPieces)
        // {
        //     string num = "";
        //     ulong n = b;
        //     for (int i = 0; i < 64; i++)
        //     {
        //         if ((n & 1) == 1) num += '1';
        //         else num += '0';
        //         n = n >> 1;
        //     }
        //
        //     Debug.Log(num);
        // }

        int spawnIndex = 0;
        int row = 0;
        _currentSpawnPos = StartPos;
        foreach (var b in AllPieces)
        {
            var n = b;
            row = 0;
            _currentSpawnPos = StartPos;
            for (int i = 0; i < 64; i++)
            {
                // Debug.Log(_currentSpawnPos);
                if ((i + 1) % 8 == 0)
                {
                    row++;
                    Debug.Log("ROW HIGHER");
                    _currentSpawnPos += Vector3.forward;
                };
                if ((n & 1) == 1)
                    Instantiate(ObjectsToSpawn[spawnIndex], _currentSpawnPos, Quaternion.identity);
                n >>= 1;
                _currentSpawnPos += row % 2 == 0 ? Vector3.right : Vector3.left;
            }

            spawnIndex++;
        }
    }
}