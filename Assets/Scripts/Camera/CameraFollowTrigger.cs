using UnityEngine;

public class CameraFollowTrigger : MonoBehaviour
{
    [SerializeField] private GameObject mainCam;
    [SerializeField] private GameObject introCam;

    void OnTriggerEnter2D(Collider2D collision)
    {
        mainCam?.SetActive(true);
        introCam?.SetActive(false);
    }

}
