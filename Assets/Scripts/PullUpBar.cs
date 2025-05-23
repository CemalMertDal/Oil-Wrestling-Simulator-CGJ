using UnityEngine;

public class PullUpBar : InteractableObject
{
    [Header("Pull-Up Settings")]
    [SerializeField] private float pullUpSpeed = 2f;         // Çekme hızı
    [SerializeField] private float dropSpeed = 1f;           // İnme hızı
    [SerializeField] private float maxHeight = 0.5f;         // Maksimum yükselme miktarı
    [SerializeField] private Transform startPosition;        // Karakterin başlangıç pozisyonu
    [SerializeField] private Transform endPosition;          // Karakterin en yüksek pozisyonu
    
    [Header("Animation Settings")]
    [SerializeField] private float armAnimationSpeed = 90f;  // Kol animasyon hızı (derece/saniye)
    
    private bool isHanging = false;                          // Bar'a asılı mı?
    private bool isPullingUp = false;                        // Yukarı çekiliyor mu?
    private bool hasReachedTop = false;                      // Tepeye ulaştı mı?
    private bool isDescending = false;                       // Aşağı iniyor mu?
    private float currentHeight = 0f;                        // Mevcut yükseklik
    private float armAngle = 0f;                             // Kol açısı (animasyon için)
    
    private GameObject player;                               // Oyuncu referansı
    private Rigidbody playerRb;                              // Oyuncu rigidbody
    private CharacterMovement playerMovement;                // Oyuncu hareket script'i
    private PlayerInteraction playerInteraction;             // Oyuncunun etkileşim bileşeni
    private Vector3 originalPlayerPos;                       // Oyuncunun orijinal pozisyonu
    private Quaternion originalPlayerRot;                    // Oyuncunun orijinal rotasyonu
    
    void Start()
    {
        // Etkileşim mesajını ayarla
        interactionMessage = "Press E to hang on pull-up bar";
        
        // Başlangıç ve bitiş pozisyonları belirtilmemişse
        if (startPosition == null)
        {
            startPosition = transform;
            Debug.LogWarning("Start position not set. Using the bar's position.");
        }
        
        if (endPosition == null)
        {
            // Başlangıç pozisyonunun üstünde bir nokta oluştur
            GameObject endPosObj = new GameObject("EndPosition");
            endPosObj.transform.position = startPosition.position + new Vector3(0f, maxHeight, 0f);
            endPosObj.transform.parent = transform;
            endPosition = endPosObj.transform;
            Debug.LogWarning("End position not set. Created one above the start position.");
        }
    }
    
    void Update()
    {
        if (!isHanging) return;
        
        // G tuşuna basılırsa bardan düş
        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("G key pressed, dropping from bar");
            DropFromBar();
            return;
        }
        
        // Aşağı iniş sırasında F tuşu devre dışı
        if (isDescending)
        {
            // Aşağı indir
            currentHeight -= dropSpeed * Time.deltaTime;
            
            // Kol animasyonu için açıyı azalt
            armAngle -= armAnimationSpeed * Time.deltaTime;
            
            // En aşağıya indiğinde tekrar yukarı çıkabilir
            if (currentHeight <= 0f)
            {
                currentHeight = 0f;
                hasReachedTop = false;
                isDescending = false;
            }
        }
        // F tuşuna basılıyorsa ve yukarı çıkabilir durumdaysa
        else if (Input.GetKey(KeyCode.F) && !hasReachedTop && !isDescending)
        {
            isPullingUp = true;
            
            // Yukarı çek
            currentHeight += pullUpSpeed * Time.deltaTime;
            
            // Kol animasyonu için açıyı arttır
            armAngle += armAnimationSpeed * Time.deltaTime;
            
            // Maksimum yüksekliği geçmemesini sağla
            if (currentHeight >= 1f)
            {
                currentHeight = 1f;
                hasReachedTop = true;
                isPullingUp = false;
            }
        }
        // Tepeye ulaştıysa ve F tuşu bırakıldıysa, aşağı inmeye başla
        else if (hasReachedTop && !Input.GetKey(KeyCode.F))
        {
            isDescending = true;  // Aşağı inme modunu aktif et
        }
        // Normal durumda ve F basılı değilse, yavaşça aşağı in
        else if (!isPullingUp && !hasReachedTop && !isDescending)
        {
            // F tuşu basılı değilse ve yukarıda değilse, yavaşça aşağı in
            currentHeight -= dropSpeed * Time.deltaTime;
            
            // Kol animasyonu için açıyı azalt
            armAngle -= armAnimationSpeed * Time.deltaTime;
            
            // 0'ın altına inmemesini sağla
            if (currentHeight <= 0f)
            {
                currentHeight = 0f;
            }
        }
        
        // Armangle 0-90 derece arasında sınırla
        armAngle = Mathf.Clamp(armAngle, 0f, 90f);
        
        // Karakter pozisyonunu güncelle - Lerp ile yumuşak geçiş
        if (player != null)
        {
            // Başlangıç ve bitiş pozisyonları arasında interpolasyon
            player.transform.position = Vector3.Lerp(startPosition.position, endPosition.position, currentHeight);
            
            // Kolları simüle et - Burası oyuncunun modeline göre ayarlanmalı
            SimulateArmAnimation();
        }
    }
    
    // Kol animasyonunu simüle et
    private void SimulateArmAnimation()
    {
        // Bu metot, oyuncunuzun kol modelini veya animasyonunu kontrol etmelidir
        // Örnek olarak, eğer oyuncunun Transform'lerini kullanıyorsanız:
        
        // Örnek: Karakterin kollarını temsil eden bir Transform varsa
        Transform leftArm = player.transform.Find("LeftArm");
        Transform rightArm = player.transform.Find("RightArm");
        
        if (leftArm != null && rightArm != null)
        {
            // Kolların rotasyonunu ayarla
            leftArm.localRotation = Quaternion.Euler(armAngle, 0f, 0f);
            rightArm.localRotation = Quaternion.Euler(armAngle, 0f, 0f);
        }
        else
        {
            // Debug.Log("Arm transforms not found. Cannot animate arms.");
        }
        
        // NOT: Gerçek bir projede, muhtemelen karakter animasyonu için Animator veya Animation komponentlerini kullanmanız gerekir.
    }
    
    public override void Interact(GameObject player)
    {
        base.Interact(player);
        
        if (isHanging) return;
        
        Debug.Log("Hanging on pull-up bar...");
        
        // Oyuncu referansını kaydet
        this.player = player;
        
        // Oyuncunun Rigidbody, CharacterMovement ve PlayerInteraction bileşenlerini al
        playerRb = player.GetComponent<Rigidbody>();
        playerMovement = player.GetComponent<CharacterMovement>();
        playerInteraction = player.GetComponent<PlayerInteraction>();
        
        if (playerRb == null)
        {
            Debug.LogError("Player does not have a Rigidbody component!");
            return;
        }
        
        // Oyuncunun orijinal değerlerini kaydet
        originalPlayerPos = player.transform.position;
        originalPlayerRot = player.transform.rotation;
        
        // Oyuncuyu bara konumlandır
        player.transform.position = startPosition.position;
        player.transform.rotation = startPosition.rotation;
        
        // Oyuncunun fiziksel hareketini devre dışı bırak
        playerRb.isKinematic = true;
        
        // Oyuncu hareketini devre dışı bırak
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }
        
        // PlayerInteraction'a referansımızı kaydedelim
        if (playerInteraction != null)
        {
            playerInteraction.heldObject = this;
            Debug.Log("Set heldObject in PlayerInteraction");
        }
        else
        {
            Debug.LogError("PlayerInteraction component not found on player!");
        }
        
        // Asılma durumunu başlat
        isHanging = true;
        isPullingUp = false;
        hasReachedTop = false;
        isDescending = false;
        currentHeight = 0f;
        armAngle = 0f;
        
        // Etkileşim mesajını güncelle
        interactionMessage = "Press G to drop from bar";
    }
    
    // Bardan düşme/inme - PlayerInteraction.Update() içinde çağrılır
    public override void Drop()
    {
        base.Drop();
        DropFromBar();
    }
    
    // Bardan düşme/inme işlemlerini gerçekleştiren özel metot
    private void DropFromBar()
    {
        if (!isHanging) return;
        
        Debug.Log("Dropping from pull-up bar...");
        
        // Oyuncunun fiziğini geri aç
        if (playerRb != null)
        {
            playerRb.isKinematic = false;
        }
        
        // Oyuncu hareketini geri aç
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }
        
        // PlayerInteraction'dan referansımızı kaldıralım
        if (playerInteraction != null)
        {
            playerInteraction.heldObject = null;
            Debug.Log("Cleared heldObject in PlayerInteraction");
        }
        
        // Asılma durumunu sonlandır
        isHanging = false;
        
        // Etkileşim mesajını güncelle
        interactionMessage = "Press E to hang on pull-up bar";
        
        // Oyuncu referansını temizle
        player = null;
    }
    
    // Barın çizimini görselleştir (Scene modunda yardımcı olmak için)
    private void OnDrawGizmos()
    {
        if (startPosition != null && endPosition != null)
        {
            // Barın konumunu göster
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(startPosition.position, 0.1f);
            
            // Yukarı çekme yolunu göster
            Gizmos.color = Color.green;
            Gizmos.DrawLine(startPosition.position, endPosition.position);
            Gizmos.DrawSphere(endPosition.position, 0.1f);
        }
    }
}