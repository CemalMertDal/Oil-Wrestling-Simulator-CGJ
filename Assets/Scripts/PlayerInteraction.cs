using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionRadius = 3f;
    public LayerMask interactionLayer;
    public Transform handTransform;  // Karakterin eli - Unity editöründe belirtilmeli
    public InteractableObject heldObject;  // Şu anda tutulan nesne
    
    [Header("UI")]
    public Text interactionText;  // Etkileşim metni - Varsa
    
    private Camera cam;
    
    private void Start()
    {
        cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("Main Camera not found! Make sure there's a camera with 'MainCamera' tag in the scene.");
            
            // Alternatif olarak karakterin kamerasını kullanabiliriz
            Camera[] cameras = FindObjectsOfType<Camera>();
            if (cameras.Length > 0)
            {
                cam = cameras[0];
                Debug.Log("Using the first available camera instead.");
            }
        }
        
        // Etkileşim metnini başlangıçta gizle
        if (interactionText != null)
            interactionText.gameObject.SetActive(false);
    }
    
    private void Update()
    {
        // Eğer bir nesne tutuluyorsa ve G tuşuna basılırsa bırak
        if (heldObject != null && Input.GetKeyDown(KeyCode.G))
        {
            heldObject.Drop();
            heldObject = null;
            return;
        }
        
        // Etkileşilebilir nesne ara
        InteractableObject interactable = FindInteractableObject();
        
        // Etkileşim metni
        if (interactionText != null)
        {
            if (interactable != null && interactable.canInteract)
            {
                interactionText.text = interactable.interactionMessage;
                interactionText.gameObject.SetActive(true);
                
                // E tuşu ile etkileşim
                if (Input.GetKeyDown(KeyCode.E))
                {
                    interactable.Interact(gameObject);
                }
            }
            else
            {
                interactionText.gameObject.SetActive(false);
            }
        }
    }
    
    private InteractableObject FindInteractableObject()
    {
        // Kamera kontrolü
        if (cam == null)
        {
            // Kamera yoksa tekrar bulmayı deneyelim
            cam = Camera.main;
            if (cam == null)
                return null; // Kamera bulunamadıysa çık
        }
        
        // Raycast ile etkileşilebilir nesne bul
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, interactionRadius, interactionLayer))
        {
            InteractableObject interactable = hit.collider.GetComponent<InteractableObject>();
            return interactable;
        }
        
        // Küre şeklinde tarama yapalım (alternatif olarak)
        Collider[] colliders = Physics.OverlapSphere(transform.position, interactionRadius, interactionLayer);
        float closestDistance = interactionRadius;
        InteractableObject closestInteractable = null;
        
        foreach (Collider collider in colliders)
        {
            InteractableObject interactable = collider.GetComponent<InteractableObject>();
            if (interactable != null && interactable.canInteract)
            {
                float distance = Vector3.Distance(transform.position, collider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractable = interactable;
                }
            }
        }
        
        return closestInteractable;
    }
    
    private void OnDrawGizmosSelected()
    {
        // Etkileşim yarıçapını görselleştir
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
