using UnityEngine;
using System;
using System.Collections;
using UnityEngine.EventSystems;

public class Game : MonoBehaviour
{
    [System.Serializable]
    public class GameUI
    {
        public GameObject panel;
        public GameObject titleScreen;
        public GameObject startButton;
        public GameObject gameOverScreen;
        public GameObject retryButtonYes;
    }

    public GameUI gameUI;
    public Spawner spawner;
    public Transform leftBoundary;
    public Transform rightBoundary;
    public Transform bottomBoundary;
    public GameObject[] debugBlock;
    public float fallSpeed;
    private float tempSpeed;

    private GameObject tetroBlockContainer;
    private GameObject currentTetroblock;
    private Transform[,] grid = new Transform[10, 21];   // 20+1 for y to prevent array overflow if player rotates tetroblock right away when it's spawned
    private int minCheckRow, maxCheckRow = 0;
    private int[] clearedRowIndexes = new int[4] { -1, -1, -1, -1 };
    private float fallTimer = 0f;
    private float inputTimer = 0f;
    private float inputThreshold = 0.25f;
    private float nextMove = 0f;
    private bool leftKeyPressed;
    private bool rightKeyPressed;
    private bool receiveInput = true;

    // Use this for initialization
    void Start()
    {
        tempSpeed = fallSpeed;
        gameUI.gameOverScreen.SetActive(false);
        EventSystem.current.SetSelectedGameObject(gameUI.startButton);
    }

    public void StartGame()
    {
        gameUI.panel.SetActive(false);
        gameUI.titleScreen.SetActive(false);
        tetroBlockContainer = new GameObject("Tetroblock Container");
        SpawnNewTetroblock();
    }

    void SpawnNewTetroblock()
    {
        currentTetroblock = spawner.Spawn();
        currentTetroblock.transform.parent = tetroBlockContainer.transform;
        StartCoroutine("FallDown");
    }

    // Update is called once per frame
    void Update()
    {
        if (currentTetroblock == null)
            return;

        PlayerInput();
    }

    void PlayerInput()
    {
        if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            inputTimer = 0f;
            inputThreshold = 0.25f;
            nextMove = 0f;
            leftKeyPressed = false;
        }
        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            inputTimer = 0f;
            inputThreshold = 0.25f;
            nextMove = 0f;
            rightKeyPressed = false;
        }

        if (Input.GetKey(KeyCode.LeftArrow) && rightKeyPressed == false)
        {
            leftKeyPressed = true;

            inputTimer += Time.deltaTime;
            if (inputTimer >= nextMove)
            {
                nextMove = inputTimer + inputThreshold;
                inputThreshold = 0.08f;
                if (ValidateMoveTo(Vector3.left))
                    currentTetroblock.transform.position += Vector3.left;
            }
        }
        if (Input.GetKey(KeyCode.RightArrow) && leftKeyPressed == false)
        {
            rightKeyPressed = true;

            inputTimer += Time.deltaTime;
            if (inputTimer >= nextMove)
            {
                nextMove = inputTimer + inputThreshold;
                inputThreshold = 0.08f;
                if (ValidateMoveTo(Vector3.right))
                    currentTetroblock.transform.position += Vector3.right;
            }
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentTetroblock.transform.Rotate(0, 0, 90);
            if (ValidateMoveTo(Vector3.zero) == false)
                currentTetroblock.transform.Rotate(0, 0, -90);
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            if (receiveInput == true)
                fallSpeed = 20f;
            else
                fallSpeed = tempSpeed;
        }
        else
        {
            fallSpeed = tempSpeed;
            receiveInput = true;
        }
    }

    void InputTetroblock()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            currentTetroblock = spawner.Spawn(0);
            StartCoroutine(FallDown());
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            currentTetroblock = spawner.Spawn(1);
            StartCoroutine(FallDown());
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            currentTetroblock = spawner.Spawn(2);
            StartCoroutine(FallDown());
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            currentTetroblock = spawner.Spawn(3);
            StartCoroutine(FallDown());
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            currentTetroblock = spawner.Spawn(4);
            StartCoroutine(FallDown());
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            currentTetroblock = spawner.Spawn(5);
            StartCoroutine(FallDown());
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            currentTetroblock = spawner.Spawn(6);
            StartCoroutine(FallDown());
        }
    }

    IEnumerator FallDown()
    {
        while (true)
        {
            fallTimer += Time.deltaTime;
            if (fallTimer >= 1/fallSpeed)
            {
                fallTimer = 0f;
                if (ValidateMoveTo(Vector3.down) == true)
                    currentTetroblock.transform.position += Vector3.down;
                else
                {
                    ProcessTetroblock();
                    break;
                }
            }
            yield return null;
        }
    }
    
    /// <summary>
    /// Validate every block movement to see if target direction is block-free or within boundaries
    /// </summary>
    /// <param name="direction">Direction to move to</param>
    /// <returns>True if target direction is available, otherwise false</returns>
    bool ValidateMoveTo(Vector3 direction)
    {
        foreach(Transform block in currentTetroblock.transform)
        {
            if (block.position.x + direction.x <= leftBoundary.position.x || block.position.x + direction.x >= rightBoundary.position.x || block.position.y + direction.y <= bottomBoundary.position.y)
                return false;
            else
            {
                Vector2 coordinate = ConvertPosToCoordinate(block);
                if (grid[(int)(coordinate.x + direction.x), (int)(coordinate.y + direction.y)] != null)
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Convert block's transform position to grid coordinate
    /// </summary>
    /// <param name="blockTransform"></param>
    /// <returns></returns>
    Vector2 ConvertPosToCoordinate(Transform blockTransform)
    {
        float x = (RoundNumber(blockTransform.position.x) - leftBoundary.position.x) - 1;    // minus 1 because an array starts from 0
        float y = (RoundNumber(blockTransform.position.y) - bottomBoundary.position.y) - 1;
        return new Vector2(x, y);
    }

    /// <summary>
    /// Round the float number to make sure it is not short of 0.000001 value of the intended one (e.g 3.999999f)
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    float RoundNumber(float number)
    {
        return Mathf.Round(number);
    }

    /// <summary>
    /// Store blocks' position to grid(row,column) and save the lowest and highest row of tetroblock
    /// </summary>
    void ProcessTetroblock()
    {
        int loopCount = 0;
        foreach (Transform block in currentTetroblock.transform)
        {
            Vector2 coordinate = ConvertPosToCoordinate(block);
            if(grid[(int)coordinate.x, (int)coordinate.y] != null)
            {
                GameOver();
                return;
            }

            grid[(int)coordinate.x, (int)coordinate.y] = block;

            if (loopCount == 0)
            {
                // Store first block row coordinate
                minCheckRow = maxCheckRow = (int)coordinate.y;
            }
            else
            {
                // Compare the value to the previous one
                if ((int)coordinate.y < minCheckRow)
                    minCheckRow = (int)coordinate.y;
                else if ((int)coordinate.y > maxCheckRow)
                    maxCheckRow = (int)coordinate.y;
            }
            loopCount++;
        }
        StartCoroutine(CheckRows());
    }

    /// <summary>
    /// Check if any row between highest and lowest index are full.
    /// If so, register that particular rows in an array for reference.
    /// </summary>
    /// <returns></returns>
    IEnumerator CheckRows()
    {
        int fullRowCount = 0;

        // Start at the highest index of tetroblock's row to the lowest
        for (int rowIndex = maxCheckRow; rowIndex >= minCheckRow; rowIndex--)
        {
            if (IsRowFull(rowIndex))
            {
                ClearRow(rowIndex);
                clearedRowIndexes[fullRowCount] = rowIndex;
                fullRowCount++;
            }
        }

        // If at least one row is full, then remove it one by one
        if (fullRowCount > 0)
        {
            yield return new WaitForSeconds(0.2f);

            Array.Sort(clearedRowIndexes);      // Sort value
            MoveBlocksDown(clearedRowIndexes);
            ClearIntArray(clearedRowIndexes);   // Reset value
        }

        SpawnNewTetroblock();
        receiveInput = false;
    }

    /// <summary>
    /// Check if specified row is full
    /// </summary>
    /// <param name="yPos">Row index to check</param>
    /// <returns>True if specified row is full, otherwise false</returns>
    bool IsRowFull(int yPos)
    {
        for (int i = 0; i < 10; i++)
        {
            if (grid[i, yPos] == null)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Clear every block in a single row
    /// </summary>
    /// <param name="yPos">Row index to clear</param>
    void ClearRow(int yPos)
    {
        for (int i = 0; i < 10; i++)
        {
            if (grid[i, yPos] == null)
                continue;

            Destroy(grid[i, yPos].gameObject);
        }
    }


    /// <summary>
    /// Collapse block(s) above each row that is cleared, starting from the top
    /// </summary>
    /// <param name="rowsCleared"></param>
    void MoveBlocksDown(int[] rowsCleared)
    {
        for (int i = rowsCleared.Length - 1; i >= 0; i--)
        {
            if (rowsCleared[i] == -1)
                continue;

            for (int y = rowsCleared[i] + 1; y < 20; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    if (grid[x, y] != null)
                    {
                        grid[x, y].position += Vector3.down;
                        grid[x, y - 1] = grid[x, y];
                        grid[x, y] = null;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Reset array value to -1
    /// </summary>
    /// <param name="value">Array to reset</param>
    void ClearIntArray(int[] value)
    {
        for (int i = 0; i < value.Length; i++)
        {
            value[i] = -1;
        }
    }

    void GameOver()
    {
        gameUI.panel.SetActive(true);
        gameUI.gameOverScreen.SetActive(true);
        EventSystem.current.SetSelectedGameObject(gameUI.retryButtonYes);
    }

    public void RestartGame()
    {
        Destroy(tetroBlockContainer);
        gameUI.panel.SetActive(false);
        gameUI.gameOverScreen.SetActive(false);
        tetroBlockContainer = new GameObject("Tetroblock Container");
        SpawnNewTetroblock();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;

#elif UNITY_STANDALONE
        Application.Quit();

#endif
    }
}
