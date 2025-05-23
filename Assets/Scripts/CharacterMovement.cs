using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.WebCam;

public class CharacterMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 5f;
    private float currentSpeed = 0f;
    private float speedSmoothTime = 0.2f;
    private float speedSmoothVelocity;
    public bool isRunning = false;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    

    [Header("Mouse Look Settings")]
    private float mouseSensitivity = 2f;
    public Transform playerCamera;
    public Transform groundCheck;
    private float xRotation = 0f;
    private float yRotation = 0f;

    [Header("Interaction")]
    public Transform handTransform;  // Karakterin eli - Inspector'da atanmalı

    public AudioSource walkSound;

    [Header("Others")]
    private bool closed = true;
    private Rigidbody rb;
    private PlayerInteraction playerInteraction;
    
    
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Vector3 initialCamRotation = playerCamera.localEulerAngles;
        xRotation = initialCamRotation.x;
        yRotation = transform.eulerAngles.y;
        
        // PlayerInteraction bileşenini al veya ekle
        playerInteraction = GetComponent<PlayerInteraction>();
        if (playerInteraction == null)
        {
            playerInteraction = gameObject.AddComponent<PlayerInteraction>();
        }
        
        // El pozisyonunu ayarla
        if (handTransform != null)
        {
            playerInteraction.handTransform = handTransform;
        }
        else
        {
            Debug.LogWarning("Hand transform is not assigned! Dumbbell lifting won't work correctly.");
        }

        Invoke(nameof(enableLook), 0.5f);
    }

  
    void Update()
    {
        if (!closed)
        {
            LookAround();
        }

        Move();

        Jump();
       
        if (IsGrounded() && (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0))
        {
            if (!walkSound.isPlaying)
                walkSound.Play();
        }
        else
        {
            walkSound.Stop();
        }
    }

    private void FixedUpdate()
    {
        if (rb.linearVelocity.y < 0)
        {
            // Falling: apply stronger gravity
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1f) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            // Jump released early: apply more gravity for shorter jump
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
        }
    }

    private void enableLook()
    {
        closed = false;
    }

    private void LookAround()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        yRotation += mouseX;

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }

    private void Move()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        float targetSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        
        if(moveX != 0f || moveZ != 0f)
            isRunning = targetSpeed == runSpeed ? true : false;

        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, speedSmoothTime);

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        Vector3 newVelocity = new Vector3(move.x * currentSpeed, rb.linearVelocity.y, move.z * currentSpeed);
        rb.linearVelocity = newVelocity;
    }

    private void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            Debug.Log("jumping");
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(groundCheck.position, Vector3.down, 0.4f);
    }
}