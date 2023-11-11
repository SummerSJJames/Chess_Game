using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEditor;
using UnityEngine;

public enum PieceType
{
    None,
    WPawn,
    WRook,
    WKnight,
    WBishop,
    WQueen,
    WKing,
    BPawn,
    BRook,
    BKnight,
    BBishop,
    BQueen,
    BKing,
}

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

    public List<Transform> Tiles = new List<Transform>();

    public ulong Board => WhitePawns | WhiteRooks | WhiteKnights | WhiteBishops | WhiteQueens | WhiteKing |
                          BlackPawns | BlackRooks | BlackKnights | BlackBishops | BlackQueens | BlackKing;

    public Dictionary<PieceType, (ulong, int)> BitBoards = new Dictionary<PieceType, (ulong, int)>();
    public Dictionary<int, Transform> TileInfo = new Dictionary<int, Transform>();

    private PieceType[] pieceOccupancy = new PieceType[64];

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

        BitBoards.Add(PieceType.WPawn, (WhitePawns, 0));
        BitBoards.Add(PieceType.WRook, (WhiteRooks, 0));
        BitBoards.Add(PieceType.WKnight, (WhiteKnights, 0));
        BitBoards.Add(PieceType.WBishop, (WhiteBishops, 0));
        BitBoards.Add(PieceType.WQueen, (WhiteQueens, 0));
        BitBoards.Add(PieceType.WKing, (WhiteKing, 0));

        BitBoards.Add(PieceType.BPawn, (BlackPawns, 1));
        BitBoards.Add(PieceType.BRook, (BlackRooks, 1));
        BitBoards.Add(PieceType.BKnight, (BlackKnights, 1));
        BitBoards.Add(PieceType.BBishop, (BlackBishops, 1));
        BitBoards.Add(PieceType.BQueen, (BlackQueens, 1));
        BitBoards.Add(PieceType.BKing, (BlackKing, 1));

        BitBoards.Add(PieceType.None, (0, -1));

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

        InitializePieceOccupancy(WhitePawns, PieceType.WPawn);
        InitializePieceOccupancy(WhiteRooks, PieceType.WRook);
        InitializePieceOccupancy(WhiteKnights, PieceType.WKnight);
        InitializePieceOccupancy(WhiteBishops, PieceType.WBishop);
        InitializePieceOccupancy(WhiteQueens, PieceType.WQueen);
        InitializePieceOccupancy(WhiteKing, PieceType.WKing);

        InitializePieceOccupancy(BlackPawns, PieceType.BPawn);
        InitializePieceOccupancy(BlackRooks, PieceType.BRook);
        InitializePieceOccupancy(BlackKnights, PieceType.BKnight);
        InitializePieceOccupancy(BlackBishops, PieceType.BBishop);
        InitializePieceOccupancy(BlackQueens, PieceType.BQueen);
        InitializePieceOccupancy(BlackKing, PieceType.BKing);

        SpawnPieces();
    }

    public List<int> ShowPossibleMoves(int index, PieceType pt)
    {
        List<int> validMoves = new List<int>();
        if (pt is PieceType.WKnight or PieceType.BKnight)
        {
            int[] knightOffsets = { -17, -15, -10, -6, 6, 10, 15, 17 };

            foreach (int offset in knightOffsets)
            {
                int destIndex = index + offset;

                if (destIndex is >= 0 and < 64)
                {
                    if (BitBoards[ReturnOccupant(destIndex)].Item2 == BitBoards[pt].Item2) continue;
                    int columnDiff = Math.Abs(index % 8 - destIndex % 8);
                    if (columnDiff is 1 or 2)
                        validMoves.Add(destIndex);
                }
            }
        }
        else if (pt is PieceType.WPawn or PieceType.BPawn)
        {
            int direction; // White pawns move "up" the board, black pawns move "down"
            int initialMoveOffset;

            if (pt == PieceType.WPawn)
            {
                direction = 1;
                initialMoveOffset = 8;
            }
            else
            {
                direction = -1;
                initialMoveOffset = -8;
            }

            // Initial move
            int destIndex = index + initialMoveOffset;
            if (ReturnOccupant(destIndex) == PieceType.None)
            {
                validMoves.Add(destIndex);

                // Double move if it's the pawn's first move
                destIndex += initialMoveOffset;
                if ((index / 8 == 1 && direction == 1) || (index / 8 == 6 && direction == -1))
                {
                    if (ReturnOccupant(destIndex) == PieceType.None)
                        validMoves.Add(destIndex);
                }
            }

            // Capture moves
            int[] captureOffsets = { 7, 9 }; // Diagonal capture offsets
            foreach (int offset in captureOffsets)
            {
                int o = direction == 1? offset : -offset;
                destIndex = index + o * direction;
                
                if(ReturnOccupant(destIndex) == PieceType.None) continue;
                if (destIndex >= 0 && destIndex < 64 && (index % 8 + o % 8 <= 7))
                {
                    if (BitBoards[ReturnOccupant(destIndex)].Item2 != BitBoards[pt].Item2)
                        validMoves.Add(destIndex);
                }
            }
        }
        else if (pt is PieceType.WBishop or PieceType.BBishop)
        {
            int[] bishopOffsets = { 9, 7, -9, -7 };

            foreach (int offset in bishopOffsets)
                validMoves.AddRange(DiagonalMoves(index, offset, pt));
        }
        else if (pt is PieceType.WRook or PieceType.BRook)
        {
            int[] rookOffsets = { 1, -1, 8, -8 };

            foreach (int offset in rookOffsets)
                validMoves.AddRange(StraightMoves(index, offset, pt));
        }
        else if (pt is PieceType.WQueen or PieceType.BQueen)
        {
            int[] straightOffsets = { 1, -1, 8, -8 };
            int[] diagonalOffsets = { 9, 7, -9, -7 };

            for (int i = 0; i < straightOffsets.Length; i++)
            {
                validMoves.AddRange(StraightMoves(index, straightOffsets[i], pt));
                validMoves.AddRange(DiagonalMoves(index, diagonalOffsets[i], pt));
            }
        }

        return validMoves;
    }

    List<int> DiagonalMoves(int index, int offset, PieceType pt)
    {
        int destIndex = index + offset;
        List<int> validMoves = new List<int>();

        if (destIndex is >= 0 and < 64)
        {
            int columnDiff = Math.Abs(index % 8 - destIndex % 8);
            if (columnDiff is not 1 or 2 || BitBoards[ReturnOccupant(destIndex)].Item2 == BitBoards[pt].Item2)
                return validMoves;
            validMoves.Add(destIndex);
            validMoves.AddRange(DiagonalMoves(destIndex, offset, pt));
        }

        return validMoves;
    }

    List<int> StraightMoves(int index, int offset, PieceType pt)
    {
        int destIndex = index + offset;
        List<int> validMoves = new List<int>();

        if (destIndex is >= 0 and < 64)
        {
            if ((offset == 1 && ((index + 1) % 8 == 0)) || (offset == -1 && (index % 8 == 0))) return validMoves;
            if (BitBoards[ReturnOccupant(destIndex)].Item2 == BitBoards[pt].Item2)
                return validMoves;
            validMoves.Add(destIndex);
            validMoves.AddRange(StraightMoves(destIndex, offset, pt));
        }

        return validMoves;
    }

    public bool IsOccupied(int index)
    {
        return ((Board >> index) & 1) == 1;
    }

    public void InitializePieceOccupancy(ulong bitboard, PieceType pieceType)
    {
        for (int i = 0; i < 64; i++)
        {
            if (((bitboard >> i) & 1) == 1)
                pieceOccupancy[i] = pieceType;
        }
    }

    public void SetOccupancy(PieceType pt, int index)
    {
        pieceOccupancy[index] = pt;
    }

    public PieceType ReturnOccupant(int index)
    {
        return pieceOccupancy[index];
    }

    // public int ReturnOccupant(int index)
    // {
    //     int occupant = 0;
    //     foreach (var b in AllPieces)
    //     {
    //         if (((b >> index) & 1) == 1)
    //             return occupant;
    //
    //         occupant++;
    //     }
    //
    //     return -1;
    // }

    void SpawnPieces()
    {
        int spawnIndex = 0;
        int row = 0;
        _currentSpawnPos = StartPos;
        foreach (var b in AllPieces)
        {
            var n = b;
            row = 0;
            bool isRight = true;
            _currentSpawnPos = StartPos;
            Transform piece;

            for (int i = 0; i < 64; i++)
            {
                if ((n & 1) == 1)
                {
                    _currentSpawnPos = Tiles[i].position;
                    piece = Instantiate(ObjectsToSpawn[spawnIndex], _currentSpawnPos, Quaternion.identity).transform;
                    if (!TileInfo.ContainsKey(i)) TileInfo.Add(i, piece);
                    else
                    {
                        TileInfo[i] = piece;
                    }
                }
                else
                {
                    if (!TileInfo.ContainsKey(i))
                        TileInfo.Add(i, null);
                }

                // if ((n & 1) == 1)
                // {
                //     piece = Instantiate(ObjectsToSpawn[spawnIndex], _currentSpawnPos, Quaternion.identity).transform;
                //     TileInfo.Add(i, piece);
                // }
                // else
                // {
                //     if (!TileInfo.ContainsKey(i))
                //         TileInfo.Add(i, null);
                // }
                //
                // if ((i + 1) % 8 == 0)
                // {
                //     row++;
                //     isRight = row % 2 == 0;
                //     _currentSpawnPos += Vector3.forward;
                // }
                // else
                //     _currentSpawnPos += isRight ? Vector3.right : Vector3.left;
                //
                n >>= 1;
            }

            spawnIndex++;
        }
    }
}