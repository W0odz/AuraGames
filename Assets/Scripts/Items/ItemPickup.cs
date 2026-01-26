using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    // Reference to the ScriptableObject data
    public Item item;
    public int amount = 1;

    // This function is called when the player interacts with the item
    public void Pickup()
    {
        bool wasPickedUp = InventoryManager.Instance.AddItem(item, amount);

        if (wasPickedUp)
        {
            Debug.Log($"Picked up {amount}x {item.itemName}");
            gameObject.SetActive(false);

            Destroy(gameObject);
        }
    }

    // Optional: Trigger pickup when player walks over the item
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Pickup();
        }
    }
}