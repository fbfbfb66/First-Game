using TMPro;
using UnityEngine;

public class Interaction : MonoBehaviour
{
    [SerializeField] private string npcName;
    [SerializeField] private TMP_Text speakerName;
    [SerializeField] private Transform target;
    [SerializeField] private Transform visualLayer;
    [SerializeField] private Movement movement;
    private void Awake()
    {
        if(speakerName == null)
            speakerName = GetComponentInChildren<TMP_Text>(true);

        if(movement == null)
            movement = GetComponentInParent<Movement>();
        
        Hide();
    }
    private void Update()
    {
        if(target == null)
        {
            return;
        }

        Vector2 dir = target.position - transform.root.position;
        movement.HandleFlip(dir);
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
}
