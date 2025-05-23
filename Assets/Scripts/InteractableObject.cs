using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionDistance = 2.5f;
    public string interactionMessage = "Press E to pick up";
    public bool canInteract = true;
    
    // Bu metot, oyuncu etkileşim başlattığında çağrılır
    public virtual void Interact(GameObject player)
    {
        // Alt sınıflar bu metodu override ederek kendi davranışlarını ekleyebilir
        Debug.Log($"Interacted with {gameObject.name}");
    }
    
    // Bu metot, oyuncu nesneyi bıraktığında çağrılır
    public virtual void Drop()
    {
        // Alt sınıflar bu metodu override ederek kendi davranışlarını ekleyebilir
        Debug.Log($"Dropped {gameObject.name}");
    }
}
