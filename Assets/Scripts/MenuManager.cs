using TMPro;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI finalScore, maxScore;
    [SerializeField] private PlayerData data;
    private void OnEnable()
    {        
        finalScore.SetText(data.score.ToString());
        if (data.maxScore == data.score) maxScore.enabled = true;
    }

    private void OnDisable()
    {
        maxScore.enabled = false;
    }
}
