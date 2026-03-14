using UnityEditor;
using UnityEngine;

public class SceneTransition : MonoBehaviour
{

    [SerializeField] private string goToScene;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        if (GameManager.Instance != null)
            GameManager.Instance.LoadSceneWithFade(goToScene);
        else
            Debug.LogError("[SceneTransition] GameManager.Instance é null.");
    }
}
