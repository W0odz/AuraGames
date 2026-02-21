using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    public void OnMenuButton()
    {
        GameManager.Instance.LoadSceneWithFade("TitleScreen");
    }
}