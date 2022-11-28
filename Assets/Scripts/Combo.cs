using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class Combo
{
    private BoardCell[] board;
    private int score;
    private int value;
    private int startValue;
    private int streak;
    private BoardManager manager;
    private List<BoardCell> cells = new List<BoardCell>();
    private Vector2[] comboNeighbors = 
        new Vector2[4] { new Vector2(1, 0), new Vector2(-1, 0),
                         new Vector2(0, 1), new Vector2(0, -1)};
    public Combo(Vector2 position, int value)
    {        
        manager = BoardManager.Instance;
        board = manager.board;
        this.value = value;
        startValue = manager.globalScore;
        var startCell = board.FirstOrDefault(c => c.position == position);
        streak = 1;
        cells.Add(startCell);
        CreateCombo(position, this.value);
    }

    public Combo(Vector2 position, int value, int streak)
    {
        manager = BoardManager.Instance;
        board = manager.board;
        this.value = value;
        startValue = manager.globalScore;
        var startCell = board.FirstOrDefault(c => c.position == position);
        cells.Add(startCell);
        this.streak = streak;
        CreateCombo(position, this.value);
    }

    // Create the combo recursively by checking vertical and horizontal cells    
    private void CreateCombo(Vector2 _position, int _value)
    {
        foreach(Vector2 n in comboNeighbors)
        {
            var newPos = _position + n;
            var neighbor = board.FirstOrDefault(c => c.position == newPos);
            if (neighbor != null && neighbor.dice != null && 
                neighbor.dice.value == _value && !cells.Contains(neighbor))
            {
                cells.Add(neighbor);
                CreateCombo(neighbor.position, _value);
            }
        }        
    }

    // Signal the cells if they are in the combo
    public void CheckCombo()
    {                
        if (cells.Count < 3) return;
        cells.ForEach(c => c.dice.InCombo());        
    }
    public void ClearCombo()
    {
        if (cells.Count < 3) return;
        cells.ForEach(c => c.dice.NotInCombo());
    }

    // Execute combo based on position, add score
    public IEnumerator ExecuteCombo()
    {
        if (cells.Count < 3)
            yield break;
        else
        {
            manager.PlayAudioCombo(streak);
            score += cells.Count * value;
            var nextCell = CheckNextCombo();
            var nextComboValue = 0;
            manager.CreateComboText(
                nextCell.transform.position, 
                cells.Count * value, 
                nextCell.dice.GetColor());

            // Execute combo for each dice
            // Create result dice
            foreach (BoardCell c in cells)
            {
                Dice dice = c.dice;
                dice.ExecuteCombo(nextCell.transform.position, 0.3f);
                if (c.position == nextCell.position)
                {
                    nextCell = c;
                    nextComboValue = dice.value + 1;
                    nextCell.dice = manager.CreateComboDice(dice);
                }                                               
            }

            yield return new WaitForSeconds(0.8f);
            // After all dices moved to new dice position, change its layer to default
            nextCell.dice.ChangeToDefaultLayer(); 

            // Recursively get other combos
            var combo = new Combo(nextCell.position, value + 1, streak + 1);
            yield return combo.ExecuteCombo();
            if(combo.cells.Count < 3)
                manager.PlayAudioStreak(streak);
            score += combo.score;

        }
        
        manager.globalScore += score;
    }

    private BoardCell CheckNextCombo()
    {
        Combo bestCombo = null;
        BoardCell bestCell = null;
        foreach(BoardCell cell in cells)
        {
            var combo = new Combo(cell.position, cell.dice.value + 1);
            if (bestCell == null || combo.cells.Count > bestCombo.cells.Count)
            {
                bestCell = cell;
                bestCombo = combo;
            }            
        }
        return bestCell;
    }
}
