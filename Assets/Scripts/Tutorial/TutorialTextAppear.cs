using UnityEngine;
using TMPro;
using System.Collections;

public class TutorialTextAppear : MonoBehaviour
{
    [SerializeField] private float fadeTime = 3;
    private TMP_Text text;
    private Color color;

    private bool isTriggered = false;

    void Awake()
    {
        text = GetComponentInChildren<TMP_Text>();
        color = text.color;
        text.color = new Color(color.r, color.g, color.b, 0f);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        if (!isTriggered)
            StartCoroutine("FadeIn");

    }


    IEnumerator FadeIn()
    {
        //ugly while, Update would be ideal
        while (text.color.a < 0.999f)
        {
            text.color = Color.Lerp(text.color, color, fadeTime * Time.deltaTime);
            if (text.color.a > 0.99f) text.color = color;
            yield return null;
        }
        //code after fading is finished
        isTriggered = true;
    }

}
