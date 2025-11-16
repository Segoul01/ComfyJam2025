using UnityEngine;

public class BGMSetter : MonoBehaviour
{
    [SerializeField] private string songScene;

    void Start()
    {
        switch (songScene){
            case "MainMenu":
                BGMManager.Instance.PlayMenuBGM();
                break;
            case "Game":
                BGMManager.Instance.PlayInGameBGM();
                break;
        }
    }
}
