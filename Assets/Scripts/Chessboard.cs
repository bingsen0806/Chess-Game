using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

public enum SpecialMove
{
    None = 0,
    EnPassant,
    Castling,
    Promotion
}
public class Chessboard : MonoBehaviour
{
    [Header("Art stuff")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 0.15f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;
    [SerializeField] private float deathScale = 0.55f;
    [SerializeField] private float deathSpacing = 0.5f;
    [SerializeField] private float heightDifferenceBetweenFrameAndBoard = 0.1f;
    [SerializeField] private float dragOffset = 0.8f;
    [SerializeField] private GameObject victoryScreen;

    [Header("Prefabs & Materials")]
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] teamMaterials;
    [SerializeField] private float[] materialYOffset;


    // LOGIC
    private ChessPiece[,] chessPieces;
    private ChessPiece currentlyDragging;
    private List<Vector2Int> availableMoves = new List<Vector2Int>();
    private List<ChessPiece> deadWhites = new List<ChessPiece>();
    private List<ChessPiece> deadBlacks = new List<ChessPiece>();
    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover;
    private Vector3 bounds;
    private bool isWhiteTurn;
    private SpecialMove specialMove;
    private List<Vector2Int[]> moveList = new List<Vector2Int[]>();
    //For multiplayer
    private int playerCount = -1;
    private int currentTeam = -1;
    private bool localGame = true;

    private void Start()
    {
        isWhiteTurn = true;
        GenerateAllTiles(tileSize, TILE_COUNT_X, TILE_COUNT_Y);
        SpawnAllPieces();
        PositionAllPieces();

        RegisterEvents();
    }

    private void Update()
    {
        if (!currentCamera)
        {
            currentCamera = Camera.main;
            return;
        }
        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover", "Highlight")))
        {
            //Get index of the tile hit by raycast
            Vector2Int hitPosition = LookupTileIndex(info.transform.gameObject);
            if (currentHover == -Vector2Int.one)
            {
                //Previously no tiles hovered upon
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }
            if (currentHover != hitPosition)
            {
                //Previously hovering a tile but not this one, reset previous one and change this one
                tiles[currentHover.x, currentHover.y].layer =
                    (ContainsValidMove(ref availableMoves, currentHover))
                    ? LayerMask.NameToLayer("Highlight")
                    : LayerMask.NameToLayer("Tile");
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }
            if (Input.GetMouseButtonDown(0))
            {
                if (chessPieces[hitPosition.x, hitPosition.y] != null)
                {
                    //Is it our turn?
                    if ((chessPieces[hitPosition.x, hitPosition.y].team == 0 && isWhiteTurn && currentTeam == 0)
                    || (chessPieces[hitPosition.x, hitPosition.y].team == 1 && !isWhiteTurn && currentTeam == 1))
                    {
                        currentlyDragging = chessPieces[hitPosition.x, hitPosition.y];

                        //Get a list of where I can go and highlight tiles
                        availableMoves = currentlyDragging.GetAvailableMoves(ref chessPieces,
                        TILE_COUNT_X, TILE_COUNT_Y);
                        //Get a list of special moves
                        specialMove = currentlyDragging.GetSpecialMoves(ref chessPieces, ref moveList, ref availableMoves);

                        PreventCheck();
                        HighlightTiles();
                    }
                }
            }

            if (currentlyDragging != null && Input.GetMouseButtonUp(0))
            {
                Vector2Int previousPosition = new Vector2Int(currentlyDragging.currentX, currentlyDragging.currentY);
                bool validMove = MoveTo(currentlyDragging, hitPosition.x, hitPosition.y);
                if (!validMove)
                {
                    //Move back to position since move not allowed
                    currentlyDragging.SetPosition(GetTileCenter(previousPosition.x, previousPosition.y)
                    + new Vector3(0, materialYOffset[(int)currentlyDragging.type - 1], 0));

                }
                currentlyDragging = null;
                RemoveHighlightTiles();
            }
        }
        else
        { //Raycast not hitting anything in the layer Tile
            if (currentHover != -Vector2Int.one)
            {
                tiles[currentHover.x, currentHover.y].layer =
                    (ContainsValidMove(ref availableMoves, currentHover))
                    ? LayerMask.NameToLayer("Highlight")
                    : LayerMask.NameToLayer("Tile");
                currentHover = -Vector2Int.one;
            }
            if (currentlyDragging != null && Input.GetMouseButtonUp(0))
            {
                Vector2Int previousPosition = new Vector2Int(currentlyDragging.currentX, currentlyDragging.currentY);
                currentlyDragging.SetPosition(GetTileCenter(previousPosition.x, previousPosition.y)
                    + new Vector3(0, materialYOffset[(int)currentlyDragging.type - 1], 0));
                currentlyDragging = null;
                RemoveHighlightTiles();
            }
        }

        //If we're dragging a piece
        if (currentlyDragging)
        {
            Plane horizontalPlane = new Plane(Vector3.up, Vector3.up * yOffset);
            float distance = 0.0f;
            if (horizontalPlane.Raycast(ray, out distance))
            {
                currentlyDragging.SetPosition(ray.GetPoint(distance)
                    + new Vector3(0, materialYOffset[(int)currentlyDragging.type - 1], 0)
                    + Vector3.up * dragOffset);
            }
        }
    }



    //Generate the board
    private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {
        yOffset += transform.position.y;
        bounds = new Vector3((tileCountX / 2) * tileSize, 0, (tileCountY / 2) * tileSize) + boardCenter;
        tiles = new GameObject[tileCountX, tileCountY];
        for (int x = 0; x < tileCountX; x++)
        {
            for (int y = 0; y < tileCountY; y++)
            {
                tiles[x, y] = GenerateSingleTile(tileSize, x, y);
            }
        }
    }

    //Spawning of the pieces
    private void SpawnAllPieces()
    {
        chessPieces = new ChessPiece[TILE_COUNT_X, TILE_COUNT_Y];
        int whiteTeam = 0, blackTeam = 1;

        //White team
        chessPieces[0, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
        chessPieces[1, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[2, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPieces[3, 0] = SpawnSinglePiece(ChessPieceType.Queen, whiteTeam);
        chessPieces[4, 0] = SpawnSinglePiece(ChessPieceType.King, whiteTeam);
        chessPieces[5, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPieces[6, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[7, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
        for (int i = 0; i < TILE_COUNT_X; i++)
        {
            chessPieces[i, 1] = SpawnSinglePiece(ChessPieceType.Pawn, whiteTeam);
        }

        //Black team
        chessPieces[0, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);
        chessPieces[1, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieces[2, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPieces[3, 7] = SpawnSinglePiece(ChessPieceType.Queen, blackTeam);
        chessPieces[4, 7] = SpawnSinglePiece(ChessPieceType.King, blackTeam);
        chessPieces[5, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPieces[6, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieces[7, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);
        for (int i = 0; i < TILE_COUNT_X; i++)
        {
            chessPieces[i, 6] = SpawnSinglePiece(ChessPieceType.Pawn, blackTeam);
        }
    }

    private ChessPiece SpawnSinglePiece(ChessPieceType type, int team)
    {
        ChessPiece cp = Instantiate(prefabs[(int)type - 1], transform).GetComponent<ChessPiece>();
        cp.type = type;
        cp.team = team;
        cp.GetComponent<MeshRenderer>().material = teamMaterials[team];

        return cp;
    }

    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        GameObject tileObject = new GameObject(string.Format($"X:{x}, Y: {y}"));
        tileObject.transform.parent = transform;

        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, yOffset, y * tileSize) - bounds;
        vertices[1] = new Vector3(x * tileSize, yOffset, (y + 1) * tileSize) - bounds;
        vertices[2] = new Vector3((x + 1) * tileSize, yOffset, y * tileSize) - bounds;
        vertices[3] = new Vector3((x + 1) * tileSize, yOffset, (y + 1) * tileSize) - bounds;

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };
        mesh.vertices = vertices;
        mesh.triangles = tris;

        mesh.RecalculateNormals();

        tileObject.layer = LayerMask.NameToLayer("Tile");
        tileObject.AddComponent<BoxCollider>();
        return tileObject;
    }

    //Positioning
    private void PositionAllPieces()
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (chessPieces[x, y] != null)
                {
                    PositionSinglePiece(x, y, true);
                }
            }
        }
    }

    private void PositionSinglePiece(int x, int y, bool force = false)
    {
        if (chessPieces[x, y] == null)
        {
            return;
        }
        ChessPieceType cpType = chessPieces[x, y].type;
        chessPieces[x, y].currentX = x;
        chessPieces[x, y].currentY = y;
        chessPieces[x, y].SetPosition(GetTileCenter(x, y)
        + new Vector3(0, materialYOffset[(int)cpType - 1], 0), force);
    }

    private Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3(x * tileSize, yOffset, y * tileSize) - bounds +
        new Vector3(tileSize / 2, 0, tileSize / 2);
    }

    //Hightlight Tiles
    private void HighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
        {
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Highlight");
        }
    }

    private void RemoveHighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
        {
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Tile");
        }
        availableMoves.Clear();
    }

    //Checkmate
    private void CheckMate(int winTeam)
    {
        DisplayVictory(winTeam);
    }

    private void DisplayVictory(int winningTeam)
    {
        victoryScreen.SetActive(true);
        victoryScreen.transform.GetChild(winningTeam).gameObject.SetActive(true);
    }
    private void Stalemate()
    {
        victoryScreen.SetActive(true);
        victoryScreen.transform.GetChild(2).gameObject.SetActive(true);
    }

    private void OnApplicationQuit()
    {
        UnregisterToEvents();
    }

    private void OnDestroy()
    {
        UnregisterToEvents();
    }

    public void OnResetButton()
    {
        //Disable UI
        victoryScreen.transform.GetChild(0).gameObject.SetActive(false);
        victoryScreen.transform.GetChild(1).gameObject.SetActive(false);
        victoryScreen.transform.GetChild(2).gameObject.SetActive(false);
        victoryScreen.SetActive(false);

        //Fields reset
        currentlyDragging = null;
        availableMoves.Clear();
        moveList.Clear();
        specialMove = SpecialMove.None;

        //Clean up
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (chessPieces[x, y] != null)
                {
                    Destroy(chessPieces[x, y].gameObject);
                }
                chessPieces[x, y] = null;
            }
        }

        for (int i = 0; i < deadWhites.Count; i++)
        {
            Destroy(deadWhites[i].gameObject);
        }
        for (int i = 0; i < deadBlacks.Count; i++)
        {
            Destroy(deadBlacks[i].gameObject);
        }
        deadWhites.Clear();
        deadBlacks.Clear();

        isWhiteTurn = true;
        playerCount = -1; //diff
        currentTeam = -1; //diff
        localGame = true; //diff
        SpawnAllPieces();
        PositionAllPieces();
    }

    public void OnExitButton()
    {
        Application.Quit();
    }

    //Special Moves
    private void ProcessSpecialMove()
    {

        //Possibility of EnPassant performed as it is valid
        if (specialMove == SpecialMove.EnPassant)
        {
            var newMove = moveList[moveList.Count - 1];
            ChessPiece myPawn = chessPieces[newMove[1].x, newMove[1].y];
            int direction = (myPawn.team == 0) ? 1 : -1;
            var targetPawnPosition = moveList[moveList.Count - 2];
            ChessPiece opponentPawn = chessPieces[targetPawnPosition[1].x, targetPawnPosition[1].y];

            //Did the user actually chose to perform EnPassant?
            if (myPawn.currentX == opponentPawn.currentX
            && myPawn.currentY == opponentPawn.currentY + direction)
            {
                if (opponentPawn.team == 0)
                {
                    deadWhites.Add(opponentPawn);
                    opponentPawn.SetScale(Vector3.one * deathScale);
                    opponentPawn.SetPosition(new Vector3(8 * tileSize, yOffset, -1 * tileSize) - bounds
                    + new Vector3(tileSize / 3, 0, tileSize / 2)
                    + (Vector3.forward * deathSpacing) * deadWhites.Count
                    + new Vector3(0, materialYOffset[(int)opponentPawn.type - 1] * deathScale
                    + heightDifferenceBetweenFrameAndBoard, 0));
                }
                else
                {
                    deadBlacks.Add(opponentPawn);
                    opponentPawn.SetScale(Vector3.one * deathScale);
                    opponentPawn.SetPosition(new Vector3(-1 * tileSize, yOffset, 8 * tileSize) - bounds
                    + new Vector3(2 * tileSize / 3, 0, tileSize / 2)
                    + (Vector3.back * deathSpacing) * deadBlacks.Count
                    + new Vector3(0, materialYOffset[(int)opponentPawn.type - 1] * deathScale
                    + heightDifferenceBetweenFrameAndBoard, 0));
                }
                chessPieces[opponentPawn.currentX, opponentPawn.currentY] = null;
            }
        }

        //Possibility of promotion
        if (specialMove == SpecialMove.Promotion)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];
            ChessPiece targetPawn = chessPieces[lastMove[1].x, lastMove[1].y];
            if (targetPawn.type == ChessPieceType.Pawn)
            {
                if (targetPawn.team == 0 && lastMove[1].y == 7)
                {
                    ChessPiece newQueen = SpawnSinglePiece(ChessPieceType.Queen, 0);
                    newQueen.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].transform.position;
                    Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
                    chessPieces[lastMove[1].x, lastMove[1].y] = newQueen;
                    PositionSinglePiece(lastMove[1].x, lastMove[1].y);
                }
                if (targetPawn.team == 1 && lastMove[1].y == 0)
                {
                    ChessPiece newQueen = SpawnSinglePiece(ChessPieceType.Queen, 1);
                    newQueen.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].transform.position;
                    Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
                    chessPieces[lastMove[1].x, lastMove[1].y] = newQueen;
                    PositionSinglePiece(lastMove[1].x, lastMove[1].y);
                }
            }
        }

        //Possibility of Castling performed as it is valid
        if (specialMove == SpecialMove.Castling)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];
            //Left Rook
            if (lastMove[1].x == 2)
            {
                if (lastMove[1].y == 0)
                { //White side
                    ChessPiece rook = chessPieces[0, 0];
                    chessPieces[3, 0] = rook;
                    PositionSinglePiece(3, 0);
                    chessPieces[0, 0] = null;
                }
                else if (lastMove[1].y == 7)
                { //Black side
                    ChessPiece rook = chessPieces[0, 7];
                    chessPieces[3, 7] = rook;
                    PositionSinglePiece(3, 7);
                    chessPieces[0, 7] = null;
                }
            }
            //Right Rook
            else if (lastMove[1].x == 6)
            {
                if (lastMove[1].y == 0)
                { //White side
                    ChessPiece rook = chessPieces[7, 0];
                    chessPieces[5, 0] = rook;
                    PositionSinglePiece(5, 0);
                    chessPieces[7, 0] = null;
                }
                else if (lastMove[1].y == 7)
                { //Black side
                    ChessPiece rook = chessPieces[7, 7];
                    chessPieces[5, 7] = rook;
                    PositionSinglePiece(5, 7);
                    chessPieces[7, 7] = null;
                }
            }
        }
    }
    private void PreventCheck()
    {
        ChessPiece targetKing = null;
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (chessPieces[x, y] != null
                && chessPieces[x, y].type == ChessPieceType.King
                && chessPieces[x, y].team == currentlyDragging.team)
                {
                    targetKing = chessPieces[x, y];
                }
            }
        }
        SimulateMoveForSinglePiece(currentlyDragging, ref availableMoves, targetKing);
    }

    private int SimulateMoveForSinglePiece(ChessPiece cp, ref List<Vector2Int> moves, ChessPiece targetKing)
    {
        //Save the current values, to reset after function call
        int actualX = cp.currentX;
        int actualY = cp.currentY;
        List<Vector2Int> movesToRemove = new List<Vector2Int>();

        //Go through all moves and check whether King will be checked if mvoe performed
        for (int i = 0; i < moves.Count; i++)
        {
            int simulationX = moves[i].x;
            int simulationY = moves[i].y;
            Vector2Int kingPositionThisSim = new Vector2Int(targetKing.currentX, targetKing.currentY);
            //If King is the chess piece moved
            if (cp.type == ChessPieceType.King)
            {
                kingPositionThisSim = new Vector2Int(simulationX, simulationY);
            }

            //Copy chessPieces into simulation
            ChessPiece[,] simulation = new ChessPiece[TILE_COUNT_X, TILE_COUNT_Y];
            List<ChessPiece> nextRoundAttackingPieces = new List<ChessPiece>();
            for (int x = 0; x < TILE_COUNT_X; x++)
            {
                for (int y = 0; y < TILE_COUNT_Y; y++)
                {
                    if (chessPieces[x, y] != null)
                    {
                        simulation[x, y] = chessPieces[x, y];
                        if (simulation[x, y].team != cp.team)
                        {
                            nextRoundAttackingPieces.Add(simulation[x, y]);
                        }
                    }
                }
            }

            //Simulate moving current piece
            simulation[actualX, actualY] = null;
            cp.currentX = simulationX;
            cp.currentY = simulationY;
            simulation[simulationX, simulationY] = cp;

            //Simulate current piece taking opponents piece, if any
            var deadPiece = nextRoundAttackingPieces.Find(x => x.currentX == simulationX && x.currentY == simulationY);
            if (deadPiece != null)
            {
                nextRoundAttackingPieces.Remove(deadPiece);
            }

            //Simulate and calculate whether King is endangered
            //1. Get moves of all attacking pieces
            List<Vector2Int> simMoves = new List<Vector2Int>();
            for (int a = 0; a < nextRoundAttackingPieces.Count; a++)
            {
                var pieceMoves = nextRoundAttackingPieces[a].GetAvailableMoves(ref simulation, TILE_COUNT_X, TILE_COUNT_Y);
                for (int b = 0; b < pieceMoves.Count; b++)
                {
                    simMoves.Add(pieceMoves[b]);
                }
            }
            //2. Add the King's move to be removed later, if any moves by opponent can take it next turn
            if (ContainsValidMove(ref simMoves, kingPositionThisSim))
            {
                movesToRemove.Add(moves[i]);
            }

            //Restore the position of current piece
            cp.currentX = actualX;
            cp.currentY = actualY;
        }

        //Remove all moves that will cause King to be checked
        for (int i = 0; i < movesToRemove.Count; i++)
        {
            moves.Remove(movesToRemove[i]);
        }
        return moves.Count;
    }
    private StateAfterMove CheckForCheckmate()
    {
        var lastMove = moveList[moveList.Count - 1];
        int defendingTeam = (chessPieces[lastMove[1].x, lastMove[1].y].team == 0) ? 1 : 0;
        ChessPiece targetKing = null;
        List<ChessPiece> attackingPieces = new List<ChessPiece>();
        List<ChessPiece> defendingPieces = new List<ChessPiece>();
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (chessPieces[x, y] != null)
                {
                    if (chessPieces[x, y].team == defendingTeam)
                    {
                        defendingPieces.Add(chessPieces[x, y]);
                        if (chessPieces[x, y].type == ChessPieceType.King)
                        {
                            targetKing = chessPieces[x, y];
                        }
                    }
                    else
                    {
                        attackingPieces.Add(chessPieces[x, y]);
                    }
                }
            }
        }

        //Is the king attacked right now
        List<Vector2Int> currentAvailableMoves = new List<Vector2Int>();
        for (int i = 0; i < attackingPieces.Count; i++)
        {
            var pieceMoves = attackingPieces[i].GetAvailableMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);
            for (int j = 0; j < pieceMoves.Count; j++)
            {
                currentAvailableMoves.Add(pieceMoves[j]);
            }
        }
        bool inChecked = ContainsValidMove(ref currentAvailableMoves, new Vector2Int(targetKing.currentX, targetKing.currentY));
        //Can any pieces defend the King, or can the King run
        //1. Can we move something to defend King?
        if (inChecked)
        {
            for (int i = 0; i < defendingPieces.Count; i++)
            {
                List<Vector2Int> defendingMoves = defendingPieces[i].GetAvailableMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);
                int numOfPossibleDefends = SimulateMoveForSinglePiece(defendingPieces[i], ref defendingMoves, targetKing);
                if (numOfPossibleDefends > 0)
                {
                    return StateAfterMove.Play;
                }
            }
            return StateAfterMove.Checkmate;
        }
        else
        {
            for (int i = 0; i < defendingPieces.Count; i++)
            {
                List<Vector2Int> defendingMoves = defendingPieces[i].GetAvailableMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);
                int numOfPossibleDefends = SimulateMoveForSinglePiece(defendingPieces[i], ref defendingMoves, targetKing);
                if (numOfPossibleDefends > 0)
                {
                    return StateAfterMove.Play;
                }
            }
            return StateAfterMove.Stalemate;
        }
    }

    //Operations
    private bool ContainsValidMove(ref List<Vector2Int> moves, Vector2Int pos)
    {
        for (int i = 0; i < moves.Count; i++)
        {
            if (moves[i].x == pos.x && moves[i].y == pos.y)
            {
                return true;
            }
        }
        return false;
    }
    private Vector2Int LookupTileIndex(GameObject hitInfo)
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (tiles[x, y] == hitInfo)
                {
                    return new Vector2Int(x, y);
                }
            }
        }
        return -Vector2Int.one; //invalid
    }

    private bool MoveTo(ChessPiece cp, int x, int y)
    {
        if (!ContainsValidMove(ref availableMoves, new Vector2Int(x, y)))
        {
            return false;
        }
        Vector2Int previousPosition = new Vector2Int(cp.currentX, cp.currentY);

        //Is there another piece on target position?
        if (chessPieces[x, y] != null)
        {
            ChessPiece other = chessPieces[x, y];
            if (cp.team == other.team)
            {
                return false;
            }
            //if the opponent's piece is white
            if (other.team == 0)
            {
                if (other.type == ChessPieceType.King)
                {
                    CheckMate(1);
                }
                deadWhites.Add(other);
                other.SetScale(Vector3.one * deathScale);
                other.SetPosition(new Vector3(8 * tileSize, yOffset, -1 * tileSize) - bounds
                + new Vector3(tileSize / 3, 0, tileSize / 2)
                + (Vector3.forward * deathSpacing) * deadWhites.Count
                + new Vector3(0, materialYOffset[(int)other.type - 1] * deathScale
                + heightDifferenceBetweenFrameAndBoard, 0));
            }
            else //opponent's piece is black
            {
                if (other.type == ChessPieceType.King)
                {
                    CheckMate(0);
                }
                deadBlacks.Add(other);
                other.SetScale(Vector3.one * deathScale);
                other.SetPosition(new Vector3(-1 * tileSize, yOffset, 8 * tileSize) - bounds
                + new Vector3(2 * tileSize / 3, 0, tileSize / 2)
                + (Vector3.back * deathSpacing) * deadBlacks.Count
                + new Vector3(0, materialYOffset[(int)other.type - 1] * deathScale
                + heightDifferenceBetweenFrameAndBoard, 0));
            }
        }
        chessPieces[x, y] = cp;
        chessPieces[previousPosition.x, previousPosition.y] = null;
        PositionSinglePiece(x, y);
        isWhiteTurn = !isWhiteTurn;
        if (localGame)
        {
            currentTeam = (currentTeam == 0) ? 1 : 0;
        }
        moveList.Add(new Vector2Int[] { previousPosition, new Vector2Int(x, y) });
        ProcessSpecialMove(); //e.g. if En Passant executed, need to remove opponent piece
        StateAfterMove state = CheckForCheckmate();
        if (state == StateAfterMove.Checkmate)
        {
            CheckMate(cp.team);
        }
        else if (state == StateAfterMove.Stalemate)
        {
            Stalemate();
        }
        return true;
    }

    #region
    private void RegisterEvents()
    {
        NetUtility.S_WELCOME += OnWelcomeServer;
        NetUtility.C_WELCOME += OnWelcomeClient;
        NetUtility.C_START_GAME += OnStartGameClient;
        GameUI.Instance.SetLocalGame += OnSetLocalGame;
    }

    private void UnregisterToEvents()
    {
        NetUtility.S_WELCOME -= OnWelcomeServer;
        NetUtility.C_WELCOME -= OnWelcomeClient;
        NetUtility.C_START_GAME -= OnStartGameClient;
        GameUI.Instance.SetLocalGame -= OnSetLocalGame;
    }

    //Server
    private void OnWelcomeServer(NetMessage msg, NetworkConnection cnn)
    {
        //Assign a team and return message to Client
        NetWelcome nw = msg as NetWelcome;

        //Assign a team, note that when we host we also immediately
        //connect to ourselves
        nw.AssignedTeam = ++playerCount;

        //Return back to the client
        Server.Instance.SendToClient(cnn, nw);

        //Start the game if there are 2 players
        if (playerCount == 1)
        {
            Server.Instance.Broadcast(new NetStartGame());
        }
    }

    //Client
    private void OnWelcomeClient(NetMessage msg)
    {
        //Receive the connection message
        NetWelcome nw = msg as NetWelcome;

        //Assign the team
        currentTeam = nw.AssignedTeam;

        Debug.Log($"My assigned team is {currentTeam}");

        if (localGame && currentTeam == 0)
        {
            Server.Instance.Broadcast(new NetStartGame());
        }
    }

    private void OnStartGameClient(NetMessage obj)
    {
        //Change camera position
        GameUI.Instance.ChangeCamera((currentTeam == 0) ?
        CameraAngle.whiteTeam : CameraAngle.blackTeam);
    }
    private void OnSetLocalGame(bool value)
    {
        localGame = value;
    }
    #endregion
}

public enum StateAfterMove
{
    Play, Stalemate, Checkmate
}
