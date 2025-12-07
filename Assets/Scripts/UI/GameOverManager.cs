using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    public void OnMenuButton()
    {
        GameManager.instance.LoadSceneWithFade("TitleScreen");
    }
}