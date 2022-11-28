using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ComboText : MonoBehaviour
{
    public int value;
    [SerializeField] private TextMeshPro text;
    // Start is called before the first frame update
    IEnumerator Start()
    {
        text.SetText("+" + value.ToString());
        var dist = Random.value * 3;
        var angle = Random.Range(-45, 45);
        Vector2 v = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        var target = (Vector2)transform.position + v;
        while (Vector2.Distance(transform.position, target) > 0.1f)
        {
            transform.position = Vector2.Lerp(transform.position, target, 0.05f);
            yield return new WaitForSeconds(0.02f);
        }
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
