using UnityEngine;

public class BlueDoor : MonoBehaviour
{
    [Header("Requirements")]
    public KeyItem requiredKey; // Drag the Blue Gem ScriptableObject here
    public string deniedMessage = "The door is locked. You need a Blue Gem.";
    public string successMessage = "The Blue Gem glows, and the door opens!";

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            CheckDoor();
        }
    }

    private void CheckDoor()
    {
        // Ask the InventoryManager if the player has the gem
        if (InventoryManager.Instance.HasItem(requiredKey))
        {
            Debug.Log(successMessage);
            OpenDoor();
        }
        else
        {
            Debug.Log(deniedMessage);
            // Here you could trigger a UI message on the screen
        }
    }

    private void OpenDoor()
    {
        // For now, let's just disable the door. 
        // You can add an animation here later!
        gameObject.SetActive(false);
    }
}