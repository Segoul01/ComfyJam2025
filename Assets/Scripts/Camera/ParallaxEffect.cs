using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    private float length, startPos;
    [SerializeField] private GameObject cam;
    [SerializeField] private float parallaxStrength;
    [SerializeField] private bool repeating = true;

    float dist;
    float temp;

    private void Start()
    {
        startPos = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    private void FixedUpdate()
    {
        temp = cam.transform.position.x * (1 - parallaxStrength);
        dist = cam.transform.position.x * parallaxStrength;

        transform.position = new Vector3(startPos + dist, transform.position.y, transform.position.z);

        if (!repeating) return;
        if (temp > startPos + length) startPos += length;
        else if (temp < startPos - length) startPos -= length;
    }
}
