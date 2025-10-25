using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine;
using Esri.GameEngine.MapView;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    [Header("Movement Settings")]
    private float moveSpeed = 5.0f;
    private float sprintMultiplier = 2.0f;

    [SerializeField]
    [Header("Jump Settings")]
    private float jumpForce = 5.0f;
    private float gravity = 9.81f;

    [SerializeField]
    [Header("Look Settings")]
    private float lookSensitivity = 1.0f;
    private float upDownLookLimit = 80.0f;

    [SerializeField]
    [Header("Input Actions")]
    private InputActionAsset playerControls;

    [SerializeField]
    [Header("First Person Camera")]
    private Transform cameraTransform;

    private Vector3 moveDirection = Vector3.zero;
    private CharacterController characterController;
    private bool isMoving;
    private GameObject arcGISCamera;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction lookAction;
    private InputAction sprintAction;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float verticalRotation;
    private float horizontalRotation;
    bool applyGravity = false;

    [SerializeField]
    [Header("Map Component")]
    private ArcGISMapComponent arcGISMapComponent;

    IEnumerator CheckMapLoadStatus()
    {
        while (arcGISMapComponent == null)
        {
            arcGISMapComponent = FindObjectOfType<ArcGISMapComponent>();
            yield return new WaitForSeconds(0.1f);
        }

        while (arcGISMapComponent.Map.LoadStatus != ArcGISLoadStatus.Loaded)
        {
            applyGravity = false;
            Debug.Log($"Map load status: {arcGISMapComponent.Map.LoadStatus}");
            yield return new WaitForSeconds(0.1f);
            
        }

        Debug.Log("Map loaded successfully.");
        yield return applyGravity = true;
    }

    void Awake()
    {
        characterController = GetComponent<CharacterController>();

        moveAction = playerControls.FindActionMap("Gameplay").FindAction("Move");
        jumpAction = playerControls.FindActionMap("Gameplay").FindAction("Jump");
        lookAction = playerControls.FindActionMap("Gameplay").FindAction("Look");
        sprintAction = playerControls.FindActionMap("Gameplay").FindAction("Sprint");

        moveAction.performed += context => moveInput = context.ReadValue<Vector2>();
        moveAction.canceled += context => moveInput = Vector2.zero;

        lookAction.performed += context => lookInput = context.ReadValue<Vector2>();
        lookAction.canceled += context => lookInput = Vector2.zero;

        
    }

    void Start()
    {
        StartCoroutine(CheckMapLoadStatus());
        arcGISCamera = GameObject.FindGameObjectWithTag("MainCamera");

        cameraTransform = arcGISCamera.transform;
        cameraTransform.rotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        HandleRotation();
        UpdateCameraPosition();
        
    }

    private void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
        lookAction.Enable();
        sprintAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
        lookAction.Disable();
        sprintAction.Disable();
    }

    void HandleMovement()
    {
        moveInput = moveAction.ReadValue<Vector2>();

        float speedMultiplier = sprintAction.ReadValue<float>() > 0 ? sprintMultiplier : 1.0f;

        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;
        
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();
        
        Vector3 forwardMovement = cameraForward * moveInput.y * moveSpeed * speedMultiplier;
        Vector3 sidewaysMovement = cameraRight * moveInput.x * moveSpeed * speedMultiplier;

        Vector3 horizontalMovement = forwardMovement + sidewaysMovement;
        horizontalMovement = transform.rotation * horizontalMovement;

        if (applyGravity)
        {
            HandleGravityAndJumping();
        }

        moveDirection.x = horizontalMovement.x;
        moveDirection.z = horizontalMovement.z;

        characterController.Move(moveDirection * Time.deltaTime);
        isMoving = moveInput.y != 0 || moveInput.x != 0;
    }

    void HandleGravityAndJumping()
    {
        if (characterController.isGrounded)
        {

            if (jumpAction.triggered)
            {
                moveDirection.y = jumpForce;
            }
            else
            {
                moveDirection.y = -gravity * Time.deltaTime;
            }
        }
        else
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }
    }

    void HandleRotation()
    {
        Vector2 lookInput = lookAction.ReadValue<Vector2>();

        horizontalRotation += lookInput.x * lookSensitivity;

        verticalRotation -= lookInput.y * lookSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, -upDownLookLimit, upDownLookLimit);

        cameraTransform.rotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0);
        
    }

    void UpdateCameraPosition()
    {
        if (cameraTransform == null) return;

        Vector3 worldCameraPosition = transform.position + transform.rotation * new Vector3(0.0f, 1.0f, 0.0f);

        cameraTransform.position = worldCameraPosition;
    }
}
