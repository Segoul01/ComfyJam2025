using System.Collections;
using UnityEngine;

public class Crow : MonoBehaviour
{
    [SerializeField] private Sprite flyingSprite;

    [SerializeField] private Vector2 flyDirection;
    [SerializeField] private float flySpeed;
    [SerializeField] private float flyTime;

    private SpriteRenderer sp;
    private BoxCollider2D col;
    private float currentFlyTime = 0f;


    void Awake()
    {
        sp = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // if (collision.CompareTag("Player"))
        // {
            sp.sprite = flyingSprite;
            StartCoroutine("Fly");
            col.enabled = false;
        // }
    }


    IEnumerator Fly()
    {
        while (true)
        {
            int dir = sp.flipX ? -1 : 1;

            transform.Translate(new Vector2(flyDirection.normalized.x * dir, flyDirection.normalized.y) * Time.deltaTime * flySpeed);
            currentFlyTime += Time.deltaTime;

            if (currentFlyTime > flyTime)
                break;
            
            yield return null;
        }

        gameObject.SetActive(false);
    }
}
