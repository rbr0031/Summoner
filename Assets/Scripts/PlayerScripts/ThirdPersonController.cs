using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class ThirdPersonController : NetworkBehaviour
{
    public CinemachineFreeLook _cameraFreeLook;

    public CharacterController controller;
    public Transform mainCamera;

    public float speed = 19f;
    public float gravity = -15;
    public float jumpHeight = 1.5f;

    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    Vector3 velocity;
    bool isGrounded;

    public float fadeTime = .5f; // Time after which the cursor fades out
    public float fadeDuration = 1.0f; // Duration of the fade effect

    private float lastMouseMovementTime;
    private bool isFading = false;
    private CanvasGroup cursorCanvasGroup;


    private void Start()
    {
        Cursor.visible = true;

        // Create a CanvasGroup to control the cursor's visibility
        cursorCanvasGroup = new GameObject("CursorCanvasGroup").AddComponent<CanvasGroup>();
        cursorCanvasGroup.transform.SetParent(transform);
        cursorCanvasGroup.alpha = 1.0f;

    }

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = GameObject.FindGameObjectWithTag("MainCamera").transform;
        }
        if (_cameraFreeLook == null)
        {
            _cameraFreeLook = FindObjectOfType<CinemachineFreeLook>();
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if(IsClient && IsOwner)
        {
            _cameraFreeLook.Follow = transform;
            _cameraFreeLook.LookAt = transform;

        }
    }


    // Update is called once per frame
    void Update()
    { 
        if (!IsOwner)
        {
            return;
        }

        gravityController();
        playerMovement();


        Cursor.lockState = CursorLockMode.Confined;

        if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
        {
            lastMouseMovementTime = Time.time;
            isFading = false;
            cursorCanvasGroup.alpha = 1.0f; // Ensure cursor is visible
            Cursor.visible = true;
        }

        // Fade out the cursor after inactivity
        if (Time.time - lastMouseMovementTime > fadeTime && !isFading)
        {
            StartCoroutine(FadeCursor());
        }
    }

    void gravityController()
    {

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }  

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -1f * gravity);
        } 

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    void playerMovement()
    {

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + mainCamera.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDirection.normalized * speed * Time.deltaTime);

            
        }
    }

    private System.Collections.IEnumerator FadeCursor()
    {
        isFading = true;
        float startTime = Time.time;
        float initialAlpha = cursorCanvasGroup.alpha;

        while (Time.time < startTime + fadeDuration)
        {
            float t = (Time.time - startTime) / fadeDuration;
            cursorCanvasGroup.alpha = Mathf.Lerp(initialAlpha, 0.0f, t);
            yield return null;
        }

        cursorCanvasGroup.alpha = 0.0f;
        Cursor.visible = false;
        isFading = false;
    }
}
