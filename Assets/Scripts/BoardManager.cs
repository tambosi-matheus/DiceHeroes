using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    // Singleton
    public static BoardManager Instance;

    // Player
    private Touch touch;
        
    // Board
    public GameObject boardObject;
    public BoardCell[] board = new BoardCell[25];
    Func<bool> isBoardEmpty;


    // Dice 
    [SerializeField] public Dice dicePrefab;
    public Color[] diceColors;
    [SerializeField] private AudioEvent diceCombo, diceStreak, others;

    // Hand
    private Dice[] dicesInHand = new Dice[2];
    private bool singleDiceHand;
    [SerializeField] private Transform inHandPosition, spawnPosition;
    [SerializeField] private Transform[] handPositions = new Transform[5];
    private BoardCell[] handLastCell = new BoardCell[2];
    
    // States
    private enum BoardStates {Free, DicesOnHand, DiceMoving, Lost};
    private BoardStates state = BoardStates.Free;

    // Game
    [SerializeField] private float difficulty = 2f;
    private float startDifficulty;
    [Range(0.1f, 1f)]
    [SerializeField] private float difficultyIncrement;
    // Basic difficulty, further development can have a probability array
    // I hate to receive a 1 when I have only 6< on the board Zzz
    private int score = 0;
    public int globalScore
    {
        get
        {
            return score;
        }
        set
        {
            score = value;
            scoreText.SetText(score.ToString());
        }
    }

    // UI
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject comboTextPrefab;

    // Audio
    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        Instance = this;
        MakeBoard();
        isBoardEmpty = new Func<bool>(() => 
        Array.Find(board, c => c.dice != null) == null);
        startDifficulty = difficulty;
    }
    private void OnEnable()
    {
        ClearBoardAndHand();
        difficulty = startDifficulty;
        score = 0;
        scoreText.SetText("0");
        inHandPosition.position = spawnPosition.position;
        state = BoardStates.Free;
        CreateDiceSet();
    }

    private void ClearBoardAndHand()
    {
        // ClearBoard
        var fBoard = Array.FindAll(board, c => c.dice != null);
        if (fBoard.Length > 0)
            Array.ForEach(fBoard, c => Destroy(c.dice.gameObject));
        // And hand
        var fHand = Array.FindAll(dicesInHand, d => d != null);
        if (fHand.Length > 0)
            Array.ForEach(dicesInHand, d => Destroy(d.gameObject));
    }

    #region Main Game

    void Update()
    {
        switch (state)
        {
            case BoardStates.Free:
                if (CheckIfLost())
                {
                    state = BoardStates.Lost;
                    StartCoroutine(GameOver());
                }
                if (Input.touchCount == 0) return;
                touch = Input.GetTouch(0);
                switch (touch.phase)
                {
                    case TouchPhase.Ended: StartCoroutine(DiceOnClickUp()); break;
                    default: DiceOnClickStay(); break;
                }
                break;
        }
    }   

    private void DiceOnClickStay()
    {
        var newPos = Camera.main.ScreenToWorldPoint(touch.position);
        inHandPosition.position = new Vector3(newPos.x, newPos.y, 0);
        ClearDicesAnimation();
        if (singleDiceHand)
        {
            
            var cell = dicesInHand[0].GetClosestCell();
            if (cell != null && cell != handLastCell[0])
            {
                
                dicesInHand[0].NotInCombo();
                handLastCell[0] = cell;
                cell.dice = dicesInHand[0];
                var combo = new Combo(cell.position, cell.dice.value);
                combo.CheckCombo(); 
                cell.dice = null;
            }
        }
        else
        {
            var cell1 = dicesInHand[0].GetClosestCell();
            var cell2 = dicesInHand[1].GetClosestCell();
            if (cell1 != null && cell2 != null && cell1 != cell2 &&
                cell1 != handLastCell[0] && cell2 != handLastCell[1])
            {
                dicesInHand[0].NotInCombo();
                dicesInHand[1].NotInCombo();
                cell1.dice = dicesInHand[0];
                cell2.dice = dicesInHand[1];

                Combo combo1, combo2 = null;
                combo1 = new Combo(cell1.position, cell1.dice.value);
                combo1.CheckCombo();
                cell1.dice = null;

                if (dicesInHand[0].value != dicesInHand[1].value)
                {
                    combo2 = new Combo(cell2.position, cell2.dice.value);
                    combo2.CheckCombo();
                }
                cell2.dice = null;                    
                           
            }

        }
    }

    private IEnumerator DiceOnClickUp()
    {
        if (singleDiceHand)
        {
            var cell = dicesInHand[0].GetClosestCell();
            if (cell != null)
            {
                dicesInHand[0].PlaceIn(cell.transform);
                PlayAudio("Place");
                cell.dice = dicesInHand[0];
                var combo = new Combo(cell.position, cell.dice.value);
                state = BoardStates.DiceMoving;
                yield return StartCoroutine(combo.ExecuteCombo());
                difficulty += difficultyIncrement;
                
                state = BoardStates.Free;
                CreateDiceSet();
            }            
        }
        else
        {
            if (IsInSpawn())
            {
                yield return StartCoroutine(RotateDicesInHand());
            }
            else
            {
                var cell1 = dicesInHand[0].GetClosestCell();
                var cell2 = dicesInHand[1].GetClosestCell();
                if (cell1 != null && cell2 != null && cell1 != cell2)
                {
                    dicesInHand[0].PlaceIn(cell1.transform);
                    dicesInHand[1].PlaceIn(cell2.transform);
                    PlayAudio("Place");
                    cell1.dice = dicesInHand[0];
                    cell2.dice = dicesInHand[1];


                    var combo = new Combo(cell1.position, cell1.dice.value);
                    state = BoardStates.DiceMoving;
                    yield return StartCoroutine(combo.ExecuteCombo());
                    

                    if (dicesInHand[0].value != dicesInHand[1].value)
                    {
                        combo = new Combo(cell2.position, cell2.dice.value);
                        yield return StartCoroutine(combo.ExecuteCombo());
                        
                    }

                    difficulty += difficultyIncrement * 2;
                    state = BoardStates.Free;
                    CreateDiceSet();
                }
            }
        }
        inHandPosition.position = spawnPosition.position;
    }
    #endregion

    #region Dice Instatiations
    // Spawns a dice set in the spawn
    private void CreateDiceSet()
    {
        singleDiceHand = UnityEngine.Random.Range(1, 3) == 1 ? false : true;

        if (singleDiceHand)
            dicesInHand[0] = CreateHandDice(handPositions[0].position);
        else
        {
            dicesInHand[0] = CreateHandDice(handPositions[1].position);
            dicesInHand[1] = CreateHandDice(handPositions[2].position);
        }       
    }

    // Instantiate dice and set its value
    private Dice CreateHandDice(Vector3 _position)
    {
        var dice = Instantiate(original: dicePrefab, _position, rotation: Quaternion.identity);
        dice.transform.parent = inHandPosition;
        dice.value = (int)UnityEngine.Random.Range(1, difficulty);
        return dice;
    }

    public Dice CreateComboDice(Dice _dice)
    {
        var dice = Instantiate(original: dicePrefab, _dice.gameObject.transform.position, rotation: Quaternion.identity);
        Destroy(dice.GetComponent<BoxCollider2D>());
        dice.transform.parent = _dice.transform.parent;
        dice.value = _dice.value + 1;
        return dice;
    }
    #endregion

    // Rotate the dices in hand anti clockwise
    // For some reason switch case does not recognize arrays had to make ifs lol
    private IEnumerator RotateDicesInHand()
    {
        state = BoardStates.DiceMoving;
        var moveTime = 0.2f;
        inHandPosition.position = spawnPosition.position;
        if (dicesInHand[0].transform.position == handPositions[0].position)
            yield break;
        if (dicesInHand[0].transform.position == handPositions[1].position)
        {
            StartCoroutine(dicesInHand[0].MoveTo(handPositions[3].position, moveTime));
            yield return StartCoroutine(dicesInHand[1].MoveTo(handPositions[4].position, moveTime));            
        }
        else if (dicesInHand[0].transform.position == handPositions[3].position)
        {
            StartCoroutine(dicesInHand[0].MoveTo(handPositions[2].position, moveTime));
            yield return StartCoroutine(dicesInHand[1].MoveTo(handPositions[1].position, moveTime));            
        }
        else if (dicesInHand[0].transform.position == handPositions[2].position)
        {
            StartCoroutine(dicesInHand[0].MoveTo(handPositions[4].position, moveTime));
            yield return StartCoroutine(dicesInHand[1].MoveTo(handPositions[3].position, moveTime));            
        }
        else if (dicesInHand[0].transform.position == handPositions[4].position)
        {
            StartCoroutine(dicesInHand[0].MoveTo(handPositions[1].position, moveTime));
            yield return StartCoroutine(dicesInHand[1].MoveTo(handPositions[2].position, moveTime));            
        }        
        state = BoardStates.Free;
    }

    // Instantiate game board and inset game objects that represents the cells
    private void MakeBoard()
    {
        for (int i = 0; i < boardObject.transform.childCount; i++)
        {
            var child = boardObject.transform.GetChild(i).GetComponent<BoardCell>();
            child.position = new Vector2(i % 5, i / 5);
            board[i] = child;
        }
    }

    private bool CheckIfLost()
    {
        //if (Input.GetMouseButtonDown(0)) return true;
        var fBoard = Array.FindAll(board, c => c.dice == null);
        if (fBoard.Length > 13) return false;        
        if (singleDiceHand && fBoard.Length > 0) return false;
            
        foreach (BoardCell c in fBoard)
        {
            // Check if right cell or down cell is empty
            var pos = new Vector2(c.position.x + 1, c.position.y);
            var neighborCell = Array.Find(board, c => c.position == pos);
            if (neighborCell != null && neighborCell.dice == null) return false;

            pos = new Vector2(c.position.x, c.position.y + 1);
            neighborCell = Array.Find(board, c => c.position == pos);
            if (neighborCell != null && neighborCell.dice == null) return false;
        }
        return true;
    }
    private bool IsInSpawn()
    {
        var pos = Camera.main.ScreenToWorldPoint(touch.position);
        RaycastHit2D hit = Physics2D.Raycast(pos, Vector3.forward);
        if (hit.collider != null && !hit.collider.CompareTag("BoardCell"))
            return true;
        return false;
    }

    private void ClearDicesAnimation()
    {
        var filteredBoard = Array.FindAll(board, c => c.dice != null);
        Array.ForEach(filteredBoard, c => c.dice.NotInCombo());
    }    

    private IEnumerator GameOver()
    {
        var fBoard = Array.FindAll(board, c => c.dice != null);
        Array.ForEach(fBoard, c => 
        StartCoroutine(c.dice.MoveTo(spawnPosition.position, 1f, true)));
        PlayAudio("ClearBoard");
        yield return new WaitUntil(isBoardEmpty);
        GameManager.Instance.SignalGameOver(score);
    }

    public void PlayAudio(string name) => others.Play(audioSource, name);

    public void PlayAudioCombo(int streak) => diceCombo.Play(audioSource, streak - 1);

    public void PlayAudioStreak (int streak)
    {
        if (streak == 2) diceStreak.Play(audioSource, 0);
        else if (streak == 3) diceStreak.Play(audioSource, 1);
        else if (streak > 3) diceStreak.Play(audioSource, 2);
    }

    public void CreateComboText(Vector2 position, int value, Color color)
    {
        var obj = Instantiate(comboTextPrefab, position, Quaternion.identity);
        obj.GetComponent<ComboText>().value = value;
        obj.GetComponentInChildren<TextMeshPro>().color = color;
    }

}
