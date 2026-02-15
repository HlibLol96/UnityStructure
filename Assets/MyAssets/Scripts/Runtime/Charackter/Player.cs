using NUnit.Framework.Internal.Filters;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float gravity = 7f;

    [Header("Look")]
    [SerializeField] private float sensitivity = 100f;
    [SerializeField, Tooltip("Lower values = smoother camera")] private float lookSmoothTime = 0.05f;

    [Header("Crouch")]
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchSpeedMultiplier = 0.5f;
    [SerializeField] private float crouchTransitionSpeed = 5f;
    [SerializeField, Tooltip("Space needed above player to stand up")] private float crouchCheckDistance = 0.5f;

    [Header("Sprint")]
    [SerializeField] private float sprintStaminaCost = 10f;
    [SerializeField] private float staminaRegenRate = 15f;
    [SerializeField] private float maxStamina = 100f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer = ~0;
    [SerializeField] private float groundCheckDistance = 0.1f;

    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private CapsuleCollider capsuleCollider;
    [SerializeField] private Transform cameraTransform;

    [Header("Inventory")]
    [SerializeField] private GameObject fastInventoryUI;
    [SerializeField] private GameObject fullInventoryUI;

    [Header("Menu")]
    [SerializeField] private GameObject menuPanel;

    // Input system
    private MyInputSystem inputSystem;
    private MyInputSystem.MyCharActions playerActions;
    private MyInputSystem.MenuActions menuActions;

    // runtime state
    private Vector2 moveInput = Vector2.zero;
    private bool isCrouching = false;
    private bool wantsToCrouch = false;
    private bool isSprinting = false;
    private bool isMenuOpen = false;
    private bool isSettingsOpen = false;
    private float standingHeight;
    private float targetYaw;
    private float targetPitch;
    private float yawSmoothVelocity;
    private float pitchSmoothVelocity;
    private float currentStamina;


    [Inject] private CharackterModel model;
    [Inject]
    private void Init(CharackterModel model)
    {
        this.model = model;
        if (model != null)
        {
            Debug.Log($"The player has {model.Health} hp");
        }
        else
        {
            Debug.LogWarning("CharackterModel was not injected. Check your Zenject installer or binding.");
        }
    }

    void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (capsuleCollider == null) capsuleCollider = GetComponent<CapsuleCollider>();

        // Ensure we have a sensible standing height
        standingHeight = capsuleCollider != null ? capsuleCollider.height : 2f;

        inputSystem = new MyInputSystem();
        playerActions = inputSystem.MyChar;
        menuActions = inputSystem.Menu;
        Cursor.lockState = CursorLockMode.Locked;

        // Initialize stamina
        currentStamina = maxStamina;

        // Ensure menu panel is inactive on start
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }
    }

    void OnEnable()
    {
        playerActions.Enable();
        menuActions.Enable();
        Subscribe();
    }

    void OnDisable()
    {
        Unsubscribe();
        playerActions.Disable();
        menuActions.Disable();
    }

    private void Start()
    {
        if (model != null)
        {
            Debug.Log($"The player has {model.Health} hp");
        }
        else
        {
            Debug.LogWarning("CharackterModel was not injected. Check your Zenject installer or binding.");
        }
        Init(model);
    }

    void FixedUpdate()
    {
        ReadMoveInput();
        HandleMovePhysics();
        GravityAmp();
        UpdateStamina();
        UpdateCrouchState();
    }

    private void LateUpdate()
    {
        ReadLookInputAndApply();
    }
    private void OnTriggerEnter(Collider other)
    {

        if (other.TryGetComponent<Item>(out var item))
        {
            if (model.Inventory.Count == 0)
            {
                model.Inventory.Add(new InvenbtoryItem(item.ItemData, 1));
                Destroy(item.gameObject);
                return;
            }
            int index = -1;
            for (int i = 0; i < model.Inventory.Count; i++)
            {
                if (model.Inventory[i].ItemData.Name == item.ItemData.Name && !model.Inventory[i].IsFullstack)
                {
                    index = i;
                    break;
                }
                

            }
            if (index == -1)
            {
                model.Inventory.Add(new InvenbtoryItem(item.ItemData, 1));
                Destroy(item.gameObject);
               
            }
            else
            {
                model.Inventory[index].AddItem(1);
                Destroy(item.gameObject);
            }
        }
    }

    private void Subscribe()
    {

        playerActions.Jump.performed += Jump;
        playerActions.Crouch.performed += Crouch;
        playerActions.Crouch.canceled += Crouch;
        playerActions.Sprint.performed += Sprint;
        playerActions.Sprint.canceled += Sprint;
        menuActions.Menu.performed += ToggleMenu;
        menuActions.Settings.performed += Settings;
    }

    private void Unsubscribe()
    {
        playerActions.Jump.performed -= Jump;
        playerActions.Crouch.performed -= Crouch;
        playerActions.Crouch.canceled -= Crouch;
        playerActions.Sprint.performed -= Sprint;
        playerActions.Sprint.canceled -= Sprint;
        menuActions.Menu.performed -= ToggleMenu;
        menuActions.Settings.performed -= Settings;
    }


    private void ReadMoveInput()
    {
        moveInput = playerActions.Move.ReadValue<Vector2>();
    }

    // Read look each Update and smooth the rotation
    private void ReadLookInputAndApply()
    {
        Vector2 rawLook = playerActions.Look.ReadValue<Vector2>();

        // If there is no meaningful input, skip work
        if (rawLook.sqrMagnitude < Mathf.Epsilon) return;

        // Different devices report different magnitudes (mouse uses delta). Try to detect mouse delta to scale appropriately.
        float deviceScale = Time.deltaTime;
        var mouse = Mouse.current;
        if (mouse != null && mouse.delta.ReadValue().sqrMagnitude > 0f)
        {
            // mouse gives delta per frame, multiply by sensitivity and Time.deltaTime for framerate independence
            deviceScale = Time.deltaTime;
        }
        else
        {
            deviceScale = Time.deltaTime * 100f;
        }

        float yawDelta = rawLook.x * sensitivity * deviceScale;
        float pitchDelta = rawLook.y * sensitivity * deviceScale;

        targetYaw += yawDelta;
        targetPitch -= pitchDelta;
        targetPitch = Mathf.Clamp(targetPitch, -89f, 89f);

        // Smooth yaw and pitch using SmoothDampAngle for consistent smoothing across 360 wrap
        float smoothYaw = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetYaw, ref yawSmoothVelocity, lookSmoothTime);
        float currentPitch = cameraTransform != null ? NormalizeAngle(cameraTransform.localEulerAngles.x) : 0f;
        float smoothPitch = Mathf.SmoothDampAngle(currentPitch, targetPitch, ref pitchSmoothVelocity, lookSmoothTime);

        // Apply rotation:
        transform.rotation = Quaternion.Euler(0f, smoothYaw, 0f);

        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.Euler(smoothPitch, 0f, 0f);
        }
    }

    private void GravityAmp()
    {
        if (!IsGrounded())
        {
            rb.AddForce(Vector3.up * gravity, ForceMode.Acceleration);
        }
    }

    private void UpdateStamina()
    {
        if (isSprinting && moveInput.magnitude > 0.01f && IsGrounded())
        {
            // Drain stamina while sprinting and moving
            currentStamina -= sprintStaminaCost * Time.fixedDeltaTime;
            currentStamina = Mathf.Max(0f, currentStamina);

            // Stop sprinting if out of stamina
            if (currentStamina <= 0f)
            {
                isSprinting = false;
            }
        }
        else
        {
            // Regenerate stamina when not sprinting
            currentStamina += staminaRegenRate * Time.fixedDeltaTime;
            currentStamina = Mathf.Min(maxStamina, currentStamina);
        }
    }

    private void ToggleMenu(InputAction.CallbackContext context)
    {
        if (menuPanel == null || isSettingsOpen) return;

        isMenuOpen = !isMenuOpen;

        if (isMenuOpen)
        {
            fullInventoryUI.SetActive(true);
            fastInventoryUI.SetActive(false);
            playerActions.Disable();
            Cursor.lockState = CursorLockMode.None;

        }
        else
        {
            fullInventoryUI.SetActive(false);
            fastInventoryUI.SetActive(true);

            playerActions.Enable();
            Cursor.lockState = CursorLockMode.Locked;

        }
    }

    private void Settings(InputAction.CallbackContext context)
    {
        if (isSettingsOpen)
        {
            menuPanel.SetActive(false);
            playerActions.Enable();
            Cursor.lockState = CursorLockMode.Locked;
            Time.timeScale = 1f;
            isSettingsOpen = false;
        }
        else
        {

            fullInventoryUI.SetActive(false);
            fastInventoryUI.SetActive(false);
            isSettingsOpen = true;
            menuPanel.SetActive(true);
            playerActions.Disable();
            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 0f;
        }
    }

    private void Sprint(InputAction.CallbackContext context)
    {
        bool pressed;

        // Try to read a button or fallback to float value
        try
        {
            pressed = context.ReadValueAsButton();
        }
        catch
        {
            pressed = context.ReadValue<float>() > 0.5f;
        }

        // Only allow sprinting if we have stamina and moving on ground
        if (pressed && currentStamina > 0f && !isCrouching && IsGrounded())
        {
            isSprinting = true;
        }
        else
        {
            isSprinting = false;
        }
    }

    private void HandleMovePhysics()
    {
        if (rb == null) return;

        Vector3 desired;
        if (moveInput.magnitude > 0.01f)
        {
            // local space movement relative to player yaw
            desired = (transform.right * moveInput.x + transform.forward * moveInput.y).normalized;
        }
        else
        {
            desired = Vector3.zero;
        }

        // Calculate speed multiplier based on state
        float speedMultiplier = 1f;

        if (isCrouching)
        {
            speedMultiplier = crouchSpeedMultiplier;
        }
        else if (isSprinting && moveInput.magnitude > 0.01f)
        {
            speedMultiplier = sprintSpeed / moveSpeed;
        }

        Vector3 horizontalVelocity = desired * moveSpeed * speedMultiplier;

        // Preserve vertical velocity (gravity, jumping)
        Vector3 newVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
        rb.linearVelocity = newVelocity;
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (rb == null) return;

        if (IsGrounded())
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            // Cancel crouch when jumping
            wantsToCrouch = false;
        }
    }

    private void Crouch(InputAction.CallbackContext context)
    {
        bool pressed;


        try
        {
            pressed = context.ReadValueAsButton();
        }
        catch
        {
            pressed = context.ReadValue<float>() > 0.5f;
        }

        wantsToCrouch = pressed;


        if (wantsToCrouch)
        {
            isSprinting = false;
        }
    }

    private void UpdateCrouchState()
    {
        if (capsuleCollider == null) return;


        if (wantsToCrouch)
        {
            if (!isCrouching)
            {
                isCrouching = true;
                ApplyCrouchState();
            }
        }
        else
        {
            // Player released crouch key, try to stand up
            if (isCrouching)
            {
                // Check if there's enough space to stand up
                if (CanStandUp())
                {
                    isCrouching = false;
                    ApplyCrouchState();
                }

            }
        }
    }

    private void ApplyCrouchState()
    {
        if (capsuleCollider == null) return;

        if (isCrouching)
        {
            capsuleCollider.height = Mathf.Max(0.1f, crouchHeight);
        }
        else
        {
            capsuleCollider.height = Mathf.Max(0.1f, standingHeight);
        }

        var center = capsuleCollider.center;
        center.y = -capsuleCollider.height / 2f;
        capsuleCollider.center = center;
    }

    private bool CanStandUp()
    {
        if (capsuleCollider == null) return false;


        Vector3 castOrigin = transform.position + Vector3.up * (standingHeight / 2f);

        return !Physics.CheckCapsule(
            castOrigin - Vector3.up * (standingHeight / 2f - capsuleCollider.radius),
            castOrigin + Vector3.up * (standingHeight / 2f - capsuleCollider.radius),
            capsuleCollider.radius,
            groundLayer
        );
    }

    private bool IsGrounded()
    {
        if (capsuleCollider != null)
        {
            // Raycast from slightly above the collider center downwards to cover small steps and slopes
            Vector3 origin = transform.position + Vector3.up * 0.1f;
            float rayDistance = capsuleCollider.bounds.extents.y + groundCheckDistance;
            return Physics.Raycast(origin, Vector3.down, rayDistance, groundLayer);
        }

        // fallback
        return Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 1f + groundCheckDistance, groundLayer);
    }

    // Helper to normalize euler angle to [-180,180] range for smooth damping
    private static float NormalizeAngle(float angle)
    {
        angle = (angle + 180f) % 360f;
        if (angle < 0f) angle += 360f;
        return angle - 180f;
    }


    public float GetCurrentStamina() => currentStamina;
    public float GetMaxStamina() => maxStamina;
    public bool IsSprinting() => isSprinting;
    public bool IsCrouching() => isCrouching;
}
