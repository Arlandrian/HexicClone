using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Board : MonoBehaviour
{
    // Square root of three
    private const float SQRT_THREE = 1.732050807f;

    #region Serialized Fields

    [Header("Prefab References")]
    [SerializeField]
    private GameObject _hexSocketPrefab;
    [SerializeField]
    private GameObject _dotPrefab;

    [Header("Board Dimensions")]
    [SerializeField]
    private int _height = 9;
    [SerializeField]
    private int _width = 8;
    [SerializeField]
    private float _hexEdgeLength = 0.54f;
    [SerializeField]
    private Vector3 _boardStartOffset;
    [SerializeField]
    private float _defaultOffsetForFilling = 5f;

    [Header("HexType Types"), Tooltip("Set the types of hexagons")]
    [SerializeField]
    private List<HexType> _hexTypes;

    #endregion

    #region Events

    public event Action HexExplodedEvent;
    public event Action MovementIncrementedEvent;

    #endregion

    #region Private Fields

    private GameObject[,] _hexSockets;
    private Vector3 _screenBottomLeftPosition;

    private DotBehaviour[,] _dots;

    private HexBehaviour[,] _board;

    private BoardController _boardController;

    [HideInInspector]
    private Transform _hexParent, _dotParent, _socketsParent;

    private bool _shouldSpawnBomb = false;

    private float HeightOfHex => SQRT_THREE * _hexEdgeLength * .5f;
    private float WidthOfHex => 1.5f * _hexEdgeLength;
    #endregion

    private void Start()
    {
        InitBoard();
    }

    #region Initialize Board

    public void InitBoard()
    {
        var timestamp = Time.realtimeSinceStartup;

        GenerateSockets();

        GenerateDots();

        GenerateHexes();

        Debug.Log(string.Format("{0:0.000000000000}s elapsed.", Time.realtimeSinceStartup - timestamp));
    }

    private void GenerateSockets()
    {
        _hexSockets = new GameObject[_width, _height];
        _screenBottomLeftPosition = new Vector3(-Camera.main.orthographicSize * 9 / 16, -Camera.main.orthographicSize, 0.0f);
        _socketsParent = new GameObject("HexSockets").transform;
        _socketsParent.transform.parent = transform;

        Vector3 position;
        // Generate Socket Positions
        for (int y = 0; y < _height; y++)
        {
            float yPos = y * _hexEdgeLength * SQRT_THREE;
            for (int x = 0; x < _width; x++)
            {
                position = _screenBottomLeftPosition + _boardStartOffset;
                position.x += (1.5f * _hexEdgeLength) * x;
                position.y += yPos;
                if (x % 2 == 1)
                {
                    position.y -= (_hexEdgeLength * SQRT_THREE / 2);
                }
                GameObject go = Instantiate(_hexSocketPrefab, position, Quaternion.identity);
                go.transform.parent = _socketsParent.transform;
                go.name = $"HexSocket({x},{y})";
                _hexSockets[x, y] = go;
            }
        }
    }

    // Each dot will have 3 children
    private void GenerateDots()
    {
        _dots = new DotBehaviour[_width - 1, (_height - 1) * 2];
        _dotParent = new GameObject("Dots").transform;
        _dotParent.transform.parent = transform;
        
        for (int y = 0; y < (_height-1)*2; y++)
        {
            for (int x = 0; x < _width-1; x++)
            {
                Vector3 position = Vector3.zero;
                foreach (var childrenSocket in GetChildrenIndex(x, y))
                {
                    position += _hexSockets[childrenSocket.x, childrenSocket.y].transform.position;
                }
                // average of position is the middle point
                // dot position
                position /= 3f;

                Quaternion rotation;
                if (x % 2 == 0)
                {
                    if(y % 2 == 0)
                    {
                        rotation = Quaternion.Euler(0f, 0f, -180.0f);

                    }
                    else
                    {
                        rotation = Quaternion.Euler(0f, 0f, 0f);
                    }

                }
                else
                {
                    if (y % 2 == 0)
                    {
                        rotation = Quaternion.Euler(0f, 0f, 0f);
                    }
                    else
                    {
                        rotation = Quaternion.Euler(0f, 0f, -180.0f);
                    }
                }

                GameObject go = Instantiate(_dotPrefab, position, rotation);
                go.transform.parent = _dotParent;
                go.name = "Dot(" + x + "," + y + ")";

                _dots[x, y] = go.GetComponent<DotBehaviour>();
                _dots[x, y].Init(x, y);
            }
        }

    }

    private void GenerateHexes()
    {
        _board = new HexBehaviour[_width, _height];
        _hexParent = new GameObject("Hexes").transform;
        _hexParent.transform.parent = transform;

        // Generate Hexes that will be placed on sockets
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                CreateHex(x, y, 0);
            }
        }
        
        ReplaceMatchingHexesStart();
    }

    private void CreateHex(int x, int y, float yOffset)
    {
        GameObject socket = _hexSockets[x, y];
        HexType type = GetRandomType();
        GameObject go = Instantiate(type.Prefab, socket.transform.position + Vector3.up * yOffset, type.Prefab.transform.rotation);
        go.transform.parent = _hexParent.transform;
        go.name = "Hex(" + x + "," + y + ")";

        HexBehaviour hexBehaviour = go.GetComponent<HexBehaviour>();
        hexBehaviour.Init(socket.transform, x, y, type);
        hexBehaviour.HexExplodedEvent += OnHexExpoded;

        if (_shouldSpawnBomb)
        {
            _shouldSpawnBomb = false;
            hexBehaviour.InitBomb();
            MovementIncrementedEvent += hexBehaviour.OnMovementIncremented;
        }

        _board[x, y] = hexBehaviour;
    }

    // If there are matches on the board remove them until none remains
    private void ReplaceMatchingHexesStart()
    {
        List<Vector2Int> matchingHexesPositions = FindMatches();
        while (matchingHexesPositions.Count > 0)
        {
            foreach (Vector2Int pos in matchingHexesPositions)
            {
                ChangeHex(_board[pos.x, pos.y]);
            }
            matchingHexesPositions = FindMatches();
        }
    }

    #endregion

    #region Public Functions

    public List<Vector2Int> FindMatches()
    {
        List<Vector2Int> matchedIndexes = new List<Vector2Int>();

        for (int y = 0; y < _height - 1; y++)
        {
            if (_board[0, y].IsSameType(_board[0, y + 1]))
            {
                if (_board[0, y + 1].IsSameType(_board[1, y + 1]))
                {
                    matchedIndexes.Add(new Vector2Int(0, y));
                    matchedIndexes.Add(new Vector2Int(0, y + 1));
                    matchedIndexes.Add(new Vector2Int(1, y + 1));
                }
            }
        }

        for (int x = 1; x < _width - 1; x++)
        {
            if (x % 2 == 0)
            {
                for (int y = 0; y < _height - 1; y++)
                {
                    if (_board[x, y].IsSameType(_board[x, y + 1]))
                    {

                        if (_board[x, y + 1].IsSameType(_board[x + 1, y + 1]))
                        {
                            matchedIndexes.Add(new Vector2Int(x, y));
                            matchedIndexes.Add(new Vector2Int(x, y + 1));
                            matchedIndexes.Add(new Vector2Int(x + 1, y + 1));
                        }
                        if (_board[x, y + 1].IsSameType(_board[x - 1, y + 1]))
                        {
                            matchedIndexes.Add(new Vector2Int(x, y));
                            matchedIndexes.Add(new Vector2Int(x, y + 1));
                            matchedIndexes.Add(new Vector2Int(x - 1, y + 1));
                        }
                    }
                }
            }
            else
            {
                for (int y = 0; y < _height - 1; y++)
                {
                    if (_board[x, y].IsSameType(_board[x, y + 1]))
                    {

                        if (_board[x, y + 1].IsSameType(_board[x + 1, y]))
                        {
                            matchedIndexes.Add(new Vector2Int(x, y));
                            matchedIndexes.Add(new Vector2Int(x, y + 1));
                            matchedIndexes.Add(new Vector2Int(x + 1, y));
                        }
                        if (_board[x, y + 1].IsSameType(_board[x - 1, y]))
                        {
                            matchedIndexes.Add(new Vector2Int(x, y));
                            matchedIndexes.Add(new Vector2Int(x, y + 1));
                            matchedIndexes.Add(new Vector2Int(x - 1, y));
                        }
                    }
                }
            }
        }

        for (int y = 0; y < _height - 1; y++)
        {
            if (_board[_width - 1, y].IsSameType(_board[_width - 1, y + 1]))
            {
                if (_board[_width - 1, y + 1].IsSameType(_board[_width - 2, y]))
                {
                    matchedIndexes.Add(new Vector2Int(_width - 1, y));
                    matchedIndexes.Add(new Vector2Int(_width - 1, y + 1));
                    matchedIndexes.Add(new Vector2Int(_width - 2, y));
                }
            }
        }

        return matchedIndexes;
    }

    public void ExplodeMatches(List<Vector2Int> matches)
    {
        foreach(var matchIndex in matches)
        {
            // Destroy and play particles of matches
            _board[matchIndex.x, matchIndex.y]?.Explode();
            _board[matchIndex.x, matchIndex.y] = null;
        }

        HashSet<int> columnnChecked = new HashSet<int>();

        // Notify all above hexes by changing their target position
        foreach(var matchIndex in matches)
        {
            if (!columnnChecked.Contains(matchIndex.x))
            {
                columnnChecked.Add(matchIndex.x);
                ColumnFall(matchIndex.x);
            }
        }

        // Find Empty cells at top and fill them
        // iterate all columns for empty cell
        for (int x = 0; x < _width; x++)
        {
            // if cell is empty, look for down for an empty cell until hitting an existing cell (or bottom)
            if(_board[x,_height-1] == null)
            {
                int depth = 1;
                for (int y = _height - 1; y > 0 && _board[x, y] == null; y--)
                {
                    depth = y;
                }

                float offset = _defaultOffsetForFilling + ((float)depth * HeightOfHex);
                for (int y = _height - 1; y > 0 && _board[x, y] == null; y--)
                {
                    CreateHex(x, y, offset);
                }
            }
        }
    }


    public void RotateDot(DotBehaviour dot, bool clockwise)
    {
        if (dot.IsRotating)
        {
            return;
        }

        // Rotate the visuals
        var childrenHexes = GetChildrenHexes(dot.DotIndexX, dot.DotIndexY);
        foreach (HexBehaviour child in childrenHexes)
        {
            child.transform.parent = dot.transform;
        }
        dot.Rotate(clockwise,
            callback: () =>
            {
                foreach (HexBehaviour child in childrenHexes)
                {
                    child.transform.parent = _hexParent.transform;
                }

                // Rotate the board
                var children = clockwise ? GetChildrenIndex(dot.DotIndexX, dot.DotIndexY).Reverse() : GetChildrenIndex(dot.DotIndexX, dot.DotIndexY);

                var child1 = children.ElementAt(0);
                var child2 = children.ElementAt(1);
                var child3 = children.ElementAt(2);

                var temp = _board[child1.x, child1.y];

                _board[child1.x, child1.y] = _board[child2.x, child2.y];
                _board[child1.x, child1.y].SetIndex(child1.x, child1.y, _hexSockets[child1.x, child1.y].transform);

                _board[child2.x, child2.y] = _board[child3.x, child3.y];
                _board[child2.x, child2.y].SetIndex(child2.x, child2.y, _hexSockets[child2.x, child2.y].transform);

                _board[child3.x, child3.y] = temp;
                _board[child3.x, child3.y].SetIndex(child3.x, child3.y, _hexSockets[child3.x, child3.y].transform);


            });
    }

    public bool IsBoardChanging()
    {
        foreach(HexBehaviour hex in _board)
        {
            if(hex == null)
            {
                return true;
            }
            if (hex.IsMoving)
            {
                return true;
            }
        }
        return false;
    }

    public void SpawnBomb()
    {
        _shouldSpawnBomb = true;
    }

    internal void OnMovementIncremented()
    {
        MovementIncrementedEvent?.Invoke();
    }

    internal void DestroyBoard()
    {
        StartCoroutine(DestroyAll());
    }

    IEnumerator DestroyAll()
    {
        foreach (HexBehaviour hex in _board)
        {
            hex?.Explode();
            yield return new WaitForSeconds(0.05f);
        }
        GameManager.Instance.ShowGameOver();
    }

    #endregion

    #region Private Functions

    private void ColumnFall(int columnIndex)
    {
        int bottom = FindMinEmpty(columnIndex);
        int above = FindAboveHex(columnIndex, bottom);
        while (above != -1 && bottom != -1)
        {
            _board[columnIndex, bottom] = _board[columnIndex, above];
            _board[columnIndex, bottom].SetPosition(bottom, _hexSockets[columnIndex, bottom].transform);
            _board[columnIndex, above] = null;
            bottom = FindMinEmpty(columnIndex);
            above = FindAboveHex(columnIndex, bottom);
        }
    }

    private int FindMinEmpty(int columnIndex)
    {
        // Find Empty bottom
        for (int y = 0; y < _height; y++)
        {
            if (_board[columnIndex, y] == null)
            {
                return y;
            }
        }
        return -1;
    }

    private int FindAboveHex(int columnIndex, int rowIndex)
    {
        for (int y = rowIndex; y < _height; y++)
        {
            if (_board[columnIndex, y] != null)
            {
                return y;
            }
        }
        return -1;
    }


    private IEnumerable<HexBehaviour> GetChildrenHexes(int dotIndexX, int dotIndexY)
    {
        foreach (Vector2Int children in GetChildrenIndex(dotIndexX, dotIndexY))
        {
            yield return _board[children.x, children.y];
        }
    }

    private IEnumerable<Vector2Int> GetChildrenIndex(int dotIndexX, int dotIndexY)
    {
        int hexYIndex = Mathf.CeilToInt(((float)dotIndexY)/2f);
        if (dotIndexX % 2 == 0)
        {
            if(dotIndexY % 2 == 0)
            {
                yield return new Vector2Int(dotIndexX, hexYIndex);
                yield return new Vector2Int(dotIndexX + 1, hexYIndex + 1);
                yield return new Vector2Int(dotIndexX + 1, hexYIndex);
            }
            else
            {
                yield return new Vector2Int(dotIndexX, hexYIndex - 1);
                yield return new Vector2Int(dotIndexX, hexYIndex);
                yield return new Vector2Int(dotIndexX + 1, hexYIndex);
            }
        }
        else
        {
            if (dotIndexY % 2 == 0)
            {
                yield return new Vector2Int(dotIndexX, hexYIndex);
                yield return new Vector2Int(dotIndexX, hexYIndex + 1);
                yield return new Vector2Int(dotIndexX + 1, hexYIndex);
            }
            else
            {
                yield return new Vector2Int(dotIndexX, hexYIndex);
                yield return new Vector2Int(dotIndexX + 1, hexYIndex);
                yield return new Vector2Int(dotIndexX + 1, hexYIndex - 1);
            }
        }
    }

    private IEnumerable<Vector2Int> GetChildrenIndexss(int dotIndexX, int dotIndexY)
    {
        float y = dotIndexY * 2 + 1;
        if (dotIndexX % 2 == 0)
        {
            if (dotIndexY % 2 == 0)
            {
                yield return new Vector2Int(dotIndexX, dotIndexY);
                yield return new Vector2Int(dotIndexX + 1, dotIndexY + 1);
                yield return new Vector2Int(dotIndexX + 1, dotIndexY);
            }
            else
            {
                yield return new Vector2Int(dotIndexX, dotIndexY - 1);
                yield return new Vector2Int(dotIndexX, dotIndexY);
                yield return new Vector2Int(dotIndexX + 1, dotIndexY);
            }
        }
        else
        {
            if (dotIndexY % 2 == 0)
            {
                yield return new Vector2Int(dotIndexX, dotIndexY + 1);
                yield return new Vector2Int(dotIndexX, dotIndexY);
                yield return new Vector2Int(dotIndexX + 1, dotIndexY);
            }
            else
            {
                yield return new Vector2Int(dotIndexX, dotIndexY);
                yield return new Vector2Int(dotIndexX + 1, dotIndexY);
                yield return new Vector2Int(dotIndexX + 1, dotIndexY - 1);
            }
        }
    }

    private void ChangeHex(HexBehaviour hex)
    {
        // Get some other random type
        HexType newType = GetRandomType();
        while (newType.Id == hex.HexType.Id)
            newType = GetRandomType();
        // Create a new game object with new type
        GameObject newGO = Instantiate(newType.Prefab, hex.transform.position, hex.transform.rotation, hex.transform.parent);
        newGO.name = hex.gameObject.name;
        var newHex = newGO.GetComponent<HexBehaviour>();
        newHex.Init(hex.TargetSocket, hex.IndexX, hex.IndexY, newType);
        newHex.HexExplodedEvent += OnHexExpoded;
        // replace old hex with new hex
        _board[hex.IndexX, hex.IndexY] = newHex;
        // Destroy old

        Destroy(hex.gameObject);
    }

    private void DestroyHex(HexBehaviour hex)
    {
        //HexExplodedEvent
        //MovementIncrementedEvent

        if (hex.IsBomb)
        {
            MovementIncrementedEvent -= hex.OnMovementIncremented;
        }

        Destroy(hex.gameObject);
    }

    private HexType GetRandomType()
    {
        return _hexTypes[UnityEngine.Random.Range(0, _hexTypes.Count)];
    }

    #endregion

    #region Event Receivers

    private void OnHexExpoded()
    {
        HexExplodedEvent?.Invoke();
    }

    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        
        if (!EditorApplication.isPlaying)
        {
            _screenBottomLeftPosition = new Vector3(-Camera.main.orthographicSize * 9 / 16, -Camera.main.orthographicSize, 0.0f);
        }

        Gizmos.DrawSphere(_screenBottomLeftPosition + _boardStartOffset, .2f);

        if (_board != null)
        {
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    if (_board[x, y] && _hexSockets[x,y])
                    {
                        switch (_board[x, y].HexType.TypeName)
                        {
                            case "red":
                                Gizmos.color = Color.red;
                                Gizmos.DrawSphere(_hexSockets[x, y].transform.position, .3f);
                                break;
                            case "blue":
                                Gizmos.color = Color.blue;
                                Gizmos.DrawSphere(_hexSockets[x, y].transform.position, .3f);
                                break;
                            case "green":
                                Gizmos.color = Color.green;
                                Gizmos.DrawSphere(_hexSockets[x, y].transform.position, .3f);
                                break;
                            case "yellow":
                                Gizmos.color = Color.yellow;
                                Gizmos.DrawSphere(_hexSockets[x, y].transform.position, .3f);
                                break;
                            case "orange":
                                Gizmos.color = Color.gray;
                                Gizmos.DrawSphere(_hexSockets[x, y].transform.position, .3f);
                                break;
                        }
                    }
                    else
                    {
                        Gizmos.color = Color.white;
                        Gizmos.DrawSphere(_hexSockets[x, y].transform.position, .3f);
                    }

                }
            }
        }


    }
#endif
}
