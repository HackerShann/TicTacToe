// TicTacToeAI.cs
// Shannon Escoriaza
// Date: 3/5/2025

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum TicTacToeState { none, cross, circle }

[System.Serializable]
public class WinnerEvent : UnityEvent<int>
{
}

public class TicTacToeAI : MonoBehaviour
{

    int _aiLevel;

    TicTacToeState[,] boardState;

    [SerializeField]
    private bool _isPlayerTurn = true;

    [SerializeField]
    private int _gridSize = 3;

    [SerializeField]
    private TicTacToeState playerState = TicTacToeState.cross;
    TicTacToeState aiState = TicTacToeState.circle;

    [SerializeField]
    private GameObject _xPrefab;

    [SerializeField]
    private GameObject _oPrefab;

    public UnityEvent onGameStarted;

    //Call This event with the player number to denote the winner
    public WinnerEvent onPlayerWin;

    ClickTrigger[,] _triggers;

    private void Awake()
    {
        if (onPlayerWin == null)
        {
            onPlayerWin = new WinnerEvent();
        }

        _triggers = new ClickTrigger[_gridSize, _gridSize];
    }


    // It sets the difficulty for the AI and starts new gamses
    public void StartAI(int AILevel)
    {
        _aiLevel = AILevel;
        StartGame();
    }


    public void RegisterTransform(int myCoordX, int myCoordY, ClickTrigger clickTrigger)
    {
        _triggers[myCoordX, myCoordY] = clickTrigger;
    }


    //New board gets created every time
    //Checks that AI and player has a different symbol
    //Will set the player to go first
    private void StartGame()
    {
        boardState = new TicTacToeState[_gridSize, _gridSize];

        for (int x = 0; x < _gridSize; x++)
        {
            for (int y = 0; y < _gridSize; y++)
            {
                boardState[x, y] = TicTacToeState.none;
            }
        }

        aiState = (playerState == TicTacToeState.cross) ? TicTacToeState.circle : TicTacToeState.cross;

        _isPlayerTurn = true;
        onGameStarted.Invoke();
    }


    // A couple things happen when this class is called
    //When player clicks on the cell it will validate that it's the players turn and the cell will be emty
    //Also checks for win or draw conditions and passes the next turn on to the AI.
    //I also added a delay to the AI move because I thouht it looked nicer to give it some time to respond.

    public void PlayerSelects(int coordX, int coordY)
    {
        if (!_isPlayerTurn || boardState[coordX, coordY] != TicTacToeState.none)
            return;

        boardState[coordX, coordY] = playerState;

        SetVisual(coordX, coordY, playerState);

        if (CheckWin(playerState))
        {
            onPlayerWin.Invoke(1);
            return;
        }

        if (IsBoardFull())
        {
            onPlayerWin.Invoke(0);
            return;
        }

        _isPlayerTurn = false;
        Invoke("MakeAIMove", 0.5f);
    }


    // Called when it's the AI's turn and checks for win or draw conditions
    // Passes turn back to player if needed
    public void AiSelects(int coordX, int coordY)
    {
        if (boardState[coordX, coordY] != TicTacToeState.none)
            return;

        boardState[coordX, coordY] = aiState;

        SetVisual(coordX, coordY, aiState);

        if (CheckWin(aiState))
        {
            onPlayerWin.Invoke(2);
            return;
        }

        if (IsBoardFull())
        {
            onPlayerWin.Invoke(0);
            return;
        }

        _isPlayerTurn = true;
    }


    //Helps the player see prefab used, "X" or "O"
    private void SetVisual(int coordX, int coordY, TicTacToeState targetState)
    {
        GameObject prefabToUse = (targetState == TicTacToeState.cross) ? _xPrefab : _oPrefab;

        Instantiate(
            prefabToUse,
            _triggers[coordX, coordY].transform.position,
            Quaternion.identity
        );
    }


    // This class helps the AI decide the next move.
    //It's supposed to try to make a winning move when it can, block a player's move, thenn make a move.
    private void MakeAIMove()
    {
        if (_isPlayerTurn) return;

        Vector2Int? winningMove = FindWinningMove(aiState);
        if (winningMove.HasValue)
        {
            AiSelects(winningMove.Value.x, winningMove.Value.y);
            return;
        }

        Vector2Int? blockingMove = FindWinningMove(playerState);
        if (blockingMove.HasValue)
        {
            AiSelects(blockingMove.Value.x, blockingMove.Value.y);
            return;
        }

        Vector2Int bestMove = FindBestMove();
        if (bestMove.x != -1 && bestMove.y != -1)
        {
            AiSelects(bestMove.x, bestMove.y);
        }
        else
        {
            Debug.LogWarning("AI cannot make a move!");
        }
    }


    // This one tries to find a move that could result in a win
    //It simulates a move and thenn brings back the coordinates if it makes sense.
    private Vector2Int? FindWinningMove(TicTacToeState state)
    {
        for (int x = 0; x < _gridSize; x++)
        {
            for (int y = 0; y < _gridSize; y++)
            {
                if (boardState[x, y] == TicTacToeState.none)
                {
                    boardState[x, y] = state;

                    if (CheckWin(state))
                    {
                        boardState[x, y] = TicTacToeState.none;
                        return new Vector2Int(x, y);
                    }

                    boardState[x, y] = TicTacToeState.none;
                }
            }
        }
        return null;
    }


    // Will try to determine the best move to take if there are no winning moves left.
    private Vector2Int FindBestMove()
    {
        int center = _gridSize / 2;
        if (boardState[center, center] == TicTacToeState.none)
            return new Vector2Int(center, center);

        Vector2Int[] corners =
        {
            new Vector2Int(0, 0),
            new Vector2Int(0, _gridSize - 1),
            new Vector2Int(_gridSize - 1, 0),
            new Vector2Int(_gridSize - 1, _gridSize - 1)
        };

        foreach (var corner in corners)
        {
            if (boardState[corner.x, corner.y] == TicTacToeState.none)
                return corner;
        }

        for (int x = 0; x < _gridSize; x++)
        {
            for (int y = 0; y < _gridSize; y++)
            {
                if (boardState[x, y] == TicTacToeState.none)
                    return new Vector2Int(x, y);
            }
        }

        return new Vector2Int(-1, -1);
    }

    //Checks for the winer
    private bool CheckWin(TicTacToeState state)
    {
        for (int i = 0; i < _gridSize; i++)
        {
            if (CheckLine(state, i, 0, 0, 1) || CheckLine(state, 0, i, 1, 0))
                return true;
        }

        if (CheckLine(state, 0, 0, 1, 1) || CheckLine(state, 0, _gridSize - 1, 1, -1))
            return true;

        return false;
    }

    //Helps check
    private bool CheckLine(TicTacToeState state, int startX, int startY, int dx, int dy)
    {
        for (int i = 0; i < _gridSize; i++)
        {
            int x = startX + i * dx;
            int y = startY + i * dy;

            if (x < 0 || x >= _gridSize || y < 0 || y >= _gridSize || boardState[x, y] != state)
                return false;
        }
        return true;
    }


    //Checks to see if the board is filled
    private bool IsBoardFull()
    {
        for (int x = 0; x < _gridSize; x++)
        {
            for (int y = 0; y < _gridSize; y++)
            {
                if (boardState[x, y] == TicTacToeState.none)
                    return false;
            }
        }
        return true;
    }
}