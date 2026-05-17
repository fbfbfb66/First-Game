using TMPro;
using UnityEngine;

public class Interaction : MonoBehaviour
{
    [SerializeField] private TMP_Text speakerName;
    [SerializeField] private string npcName;
    [SerializeField] private Transform target;
    [SerializeField] private Transform VisualLayer;
    private bool faingRight = false;
    private void Awake()
    {
        if(speakerName == null)
            speakerName = GetComponentInChildren<TMP_Text>(true);
        
        Hide();
    }
    private void Update()
    {
        Vector2 dir = target.position - transform.root.position;
        HandleFlip(dir);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.GetComponent<Player>() != null)
        {
            Show();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.GetComponent<Player>() != null)
        {
            Hide();
        }        
    }

    private void Show()
    {
        if(speakerName == null) return;
        speakerName.text = npcName;
        speakerName.gameObject.SetActive(true);
    }

    private void Hide()
    {
        if(speakerName == null) return;
        speakerName.gameObject.SetActive(false);        
    }

    private void HandleFlip(Vector2 input)
    {
        if(input.x < 0 && faingRight) Flip();
        if(input.x > 0 && !faingRight) Flip();
    }

    private void Flip()
    {
        faingRight = !faingRight;
        Vector3 scale = VisualLayer.localScale;
        scale.x = -scale.x;
        VisualLayer.localScale = scale;
    }
}
