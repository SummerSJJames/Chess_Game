using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardCreator : MonoBehaviour
{
    Transform[,] boardTiles = new Transform[8, 8];
    GameObject quad;

    Material white;
    Material black;

    public enum pieceType { FUCKINGEMPTYMATE, King, Queen, Pawn, Bishop, Knight, Rook };

    private void Start()
    {
        quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        white = new Material(Shader.Find("Standard"));
        black = new Material(Shader.Find("Standard"));
        white.color = Color.white;
        black.color = Color.black;
        quad.GetComponent<Renderer>().material = new Material(Shader.Find("Standard"));
        quad.transform.rotation = Quaternion.Euler(90, 0, 0);

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                quad.GetComponent<Renderer>().material = (i + j) % 2 == 0 ? white : black;

                quad.transform.position = new Vector3(i, 0, j);
                boardTiles[i, j] = quad.transform;
                var tile = Instantiate(quad, quad.transform.position, quad.transform.rotation);
                tile.layer = 7;
                tile.AddComponent<BoxCollider>();
            }
        }
    }
}

