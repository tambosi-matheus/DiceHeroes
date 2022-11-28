using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private enum States { Menu, Playing }
    private States state = States.Playing;
    [SerializeField] private PlayerData data;
    [SerializeField] private GameObject board, menu;
    [SerializeField] private TextMeshProUGUI maxScore, playerLevel;
    public static GameManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        state = States.Playing;        
        menu.SetActive(false);
        board.SetActive(true);
    }

    public void RestartBoard()
    {
        UpdateUI();
        menu.SetActive(false);
        board.SetActive(true);
    }

    public void SignalGameOver(int score)
    {
        data.OnGameOver(score);
        UpdateUI();
        menu.SetActive(true);
        board.SetActive(false);
    }

    public void UpdateUI()
    {
        maxScore.SetText(data.maxScore.ToString());
        playerLevel.SetText(data.level.ToString());
    }
}
