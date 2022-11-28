using System.Collections;
using TMPro;
using UnityEngine;

public class Dice : MonoBehaviour
{
    public int value;
    [SerializeField] private SpriteRenderer rend, bgRend;
    [SerializeField] private Canvas canvas;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private BoxCollider2D coll;
    [SerializeField] private Animator anim;
    public Collider2D[] collidingWith;
    private ContactFilter2D contactFilter = new ContactFilter2D();
    private BoardCell currentCell;

    // Start is called before the first frame update
    void Start()
    {
        if(value == 0)
        {
            Destroy(gameObject);
            Debug.LogError("Error, dice has no number assigned, deleting");
            return;
        }
        valueText.SetText(value.ToString());

        var colorSelect = value % 10 - 1 < 0 ? 10 : value % 10 - 1;
        rend.color = BoardManager.Instance.diceColors[colorSelect];
        name = value.ToString();        
    }   

    public BoardCell GetClosestCell()
    {
        collidingWith = new Collider2D[5];
        coll.OverlapCollider(contactFilter, collidingWith);
        var closestCellDistance = 10f;
        Collider2D closestCollider = null;

        foreach (Collider2D collider in collidingWith)
        {
            if (collider == null) continue;
            if (collider.CompareTag("BoardCell") && 
                Vector2.Distance(collider.transform.position, transform.position) < closestCellDistance)
            {
                closestCellDistance = Vector2.Distance(collider.transform.position, transform.position);
                closestCollider = collider;
            }
        }
        if (closestCollider == null) return null;
        var cell = closestCollider.GetComponent<BoardCell>();        
        if (cell.dice != null) return null;
        currentCell = cell;
        return cell;
    }

    public bool IsOnBoard()
    {
        if (currentCell == null) return false;
        collidingWith = new Collider2D[5];
        coll.OverlapCollider(contactFilter, collidingWith);

        foreach (Collider2D collider in collidingWith)
        {
            if (collider == null) continue;
            if (collider.CompareTag("BoardCell"))            
                return true;            
        }
        NotInCombo();
        currentCell = null;
        return false;
    }    
   
    public void ChangeToDefaultLayer()
    {
        rend.sortingLayerName = "Default";
        bgRend.sortingLayerName = "Default";
        canvas.sortingLayerName = "Default";
    }

    public void PlaceIn(Transform cell)
    {
        ChangeToDefaultLayer();
        transform.SetParent(cell);
        transform.position = cell.position;
        Destroy(coll);
    }

    public void InCombo() => anim.SetBool("InCombo", true);

    public void NotInCombo() => anim.SetBool("InCombo", false);

    public void ExecuteCombo(Vector2 target, float time) => StartCoroutine(MoveTo(target, time, true));
    
    public IEnumerator MoveTo(Vector2 target, float time)
    {
        var velocity = Vector3.zero;
        while (time > 0)
        {
            transform.position = Vector3.SmoothDamp
                (transform.position, target, ref velocity, time);
            time -= Time.deltaTime;
            yield return null;
        }
        transform.position = target;
    }

    public IEnumerator MoveTo(Vector2 target, float time, bool andDestroy)
    {
        var velocity = Vector3.zero;
        while (time > 0)
        {
            transform.position = Vector3.SmoothDamp
                (transform.position, target, ref velocity, time);
            time -= Time.deltaTime;
            yield return null;
        }
        transform.position = target;
        Destroy(this.gameObject);
    }
    
    public Color GetColor() => rend.color;
    
}
