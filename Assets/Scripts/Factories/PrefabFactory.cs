using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class PrefabFactory : Singleton<PrefabFactory>
{
    private const string UIManagerPrefabPath = "Prefabs/UIManagerPrefab";
    private const string BoardPrefabPath = "Prefabs/BoardPrefab";
    public UIManager CreateUIManager()
    {
        var uiManager = (Instantiate(Resources.Load(UIManagerPrefabPath)) as GameObject).GetComponent<UIManager>();
        uiManager.Init();
        return uiManager;
    }

    public BoardController CreateBoard()
    {
        var board = (Instantiate(Resources.Load(BoardPrefabPath)) as GameObject).GetComponent<Board>();
        BoardController boardController = new BoardController();
        boardController.Init(board);
        return boardController;
    }
}
