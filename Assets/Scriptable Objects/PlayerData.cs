using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance;

    // xp = 50 * lvl * lvl / 2
    static int[] experienceToLevelUp = new int[10]
    { 50,100, 225, 400, 625, 900, 1225, 1600, 2025, 2500 };


    // Quick and easy way to store data, not safe tho
    public int score { get; private set; }
    public int maxScore { get; private set; }
    public int level { get; private set; } = 1;
    private int experience;
    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        Instance = this;
        DontDestroyOnLoad(this);
        maxScore = 0;
    }

    public bool OnGameOver(int _score)
    {
        this.score = _score;
        experience += score;
        if (experience >= experienceToLevelUp[level - 1])
        {
            experience -= experienceToLevelUp[level - 1];
            level++;
        }

        if (score > maxScore)
        {
            maxScore = score;
            return true;
        }
        return false;
    }
}
