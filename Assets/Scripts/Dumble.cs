using UnityEngine;

public class Dumble : InteractableObject
{
    [Header("Curl Settings")]
    [SerializeField] private float maxAngle = 45f;        // Maksimum kaldırma açısı
    [SerializeField] private float liftSpeed = 90f;       // Kaldırma hızı (derece/saniye)
    [SerializeField] private float lowerSpeed = 30f;      // İndirme hızı (derece/saniye)
    [SerializeField] private Vector3 elbowPivotOffset = new Vector3(0, 0, -0.2f); // Dirsek pivot noktasının offseti
    
    private float currentAngle = 0f;                      // Mevcut açı
    private bool canLift = true;                          // Kaldırma izni
    private bool isLifting = false;                       // Kaldırılıyor mu?
    private bool isHeld = false;                          // Tutuluyur mu?
    
    private Rigidbody rb;
    private Collider dumbleCollider;
    private Transform originalParent;
    private Transform handPosition;
    private GameObject pivotObject;  // Pivot nesnesi
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            Debug.Log("Rigidbody added to the dumble");
        }
        
        dumbleCollider = GetComponent<Collider>();
        if (dumbleCollider == null)
        {
            // Eğer collider yoksa, bir tane ekleyelim
            dumbleCollider = gameObject.AddComponent<BoxCollider>();
            Debug.Log("BoxCollider added to the dumble");
        }
        
        originalParent = transform.parent;
        
        // Başlangıç açısını ayarla
        currentAngle = 0f;
        
        // Etkileşim mesajını ayarla
        interactionMessage = "Press E to pick up dumbbell";

        // Layer'ını ayarla (önemli)
        // gameObject.layer = LayerMask.NameToLayer("Interactable"); // Bu satırı interactable layer'ınızın adına göre ayarlayın
        
        Debug.Log("Dumble initialized successfully");
    }

    void Update()
    {
        if (!isHeld || pivotObject == null) return;
        
        // F tuşuna basılıyorsa ve kaldırma izni varsa
        isLifting = Input.GetKey(KeyCode.F) && canLift;
        
        if (isLifting)
        {
            // Yukarı kaldır
            currentAngle += liftSpeed * Time.deltaTime;
            
            // Maksimum açıyı geçmemesini sağla
            if (currentAngle >= maxAngle)
            {
                currentAngle = maxAngle;
                canLift = false;  // Maksimum açıya ulaşınca kaldırma iznini kapat
            }
        }
        else
        {
            // Aşağı indir
            currentAngle -= lowerSpeed * Time.deltaTime;
            
            // 0 derecenin altına inmemesini sağla
            if (currentAngle <= 0f)
            {
                currentAngle = 0f;
                canLift = true;   // 0 dereceye gelince kaldırma iznini aç
            }
        }
        
        // Pivot nesnesinin rotasyonunu güncelle 
        // Negatif açı ile döndür (yukarı kaldırmak için)
        pivotObject.transform.localRotation = Quaternion.Euler(-currentAngle, 0f, 0f);
    }
    
    public override void Interact(GameObject player)
    {
        // Temel sınıfın metodu çağırılır
        base.Interact(player);
        
        if (isHeld) return;
        
        Debug.Log("Trying to pick up dumble...");
        
        // Karakterin elini referans al
        PlayerInteraction playerInteraction = player.GetComponent<PlayerInteraction>();
        if (playerInteraction == null)
        {
            Debug.LogError("PlayerInteraction component not found on player!");
            return;
        }
        
        if (playerInteraction.handTransform == null)
        {
            Debug.LogError("Hand transform not assigned in PlayerInteraction!");
            return;
        }
        
        handPosition = playerInteraction.handTransform;
        
        // Pivot nesnesi oluştur - dirsek hareketini simüle etmek için
        CreatePivotObject();
        
        // Fizik özelliklerini devre dışı bırak
        rb.isKinematic = true;
        dumbleCollider.enabled = false;
        
        isHeld = true;
        canLift = true;
        currentAngle = 0f;
        
        // Oyuncuya dumbbell referansını ver
        playerInteraction.heldObject = this;
        
        Debug.Log("Dumble picked up successfully!");
    }
    
    private void CreatePivotObject()
    {
        // Eski pivot nesnesi varsa temizle
        if (pivotObject != null)
        {
            Destroy(pivotObject);
        }
        
        // Pivot nesnesini oluştur
        pivotObject = new GameObject("DumblePivot");
        pivotObject.transform.SetParent(handPosition);
        
        // Pivot pozisyonu - elin biraz gerisinde (dirsek konumu)
        pivotObject.transform.localPosition = elbowPivotOffset;
        pivotObject.transform.localRotation = Quaternion.identity;
        
        // Dumble'yi pivot nesnesine bağla
        transform.SetParent(pivotObject.transform);
        
        // Dumble'yi elden öne doğru konumlandır
        transform.localPosition = new Vector3(0f, 0f, 0.4f);
        
        // Dumble'yi yatay konumlandır
        transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        
        Debug.Log("Pivot object created: " + pivotObject.name);
    }
    
    public override void Drop()
    {
        base.Drop();
        
        if (!isHeld) return;
        
        Debug.Log("Dropping dumble...");
        
        // Dumbbell'i dünyaya bırak
        transform.SetParent(originalParent);
        
        // Pivot nesnesini temizle
        if (pivotObject != null)
        {
            Destroy(pivotObject);
            pivotObject = null;
        }
        
        // Fizik özelliklerini geri aç
        rb.isKinematic = false;
        dumbleCollider.enabled = true;
        
        isHeld = false;
        currentAngle = 0f;
        
        Debug.Log("Dumble dropped successfully!");
    }
}