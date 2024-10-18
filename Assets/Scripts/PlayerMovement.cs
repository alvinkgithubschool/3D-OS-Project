using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // For Messed Up Unity Input System

public class PlayerMovement : MonoBehaviour
{
    private PlayerControlsMap controls; 
    private Vector2 movementInput;
    private bool isSitting = false;
    public Rigidbody playerRigid;
    public Animator playerAnim;
    public Renderer playerRenderer;  
    public Transform playerTrans;
    public Transform cameraTrans; 
    public float w_speed = 5f, wb_speed = 3f, ro_speed = 90f;
    public Material blueMat, purpleMat, greenMat, yellowMat, redMat;  

    private bool isMovementLocked = true; // Initially lock movement
    private bool isReadyToRun = false; // Represents readiness after 3 seconds For New State
    private bool walking = false;

    void Awake()
    {
        controls = new PlayerControlsMap();

        // Bind movement and sit actions
        controls.PlayerControls.Movement.performed += ctx => movementInput = ctx.ReadValue<Vector2>();
        controls.PlayerControls.Movement.canceled += ctx => movementInput = Vector2.zero;
        controls.PlayerControls.Sit.performed += ctx => Sit();
        controls.PlayerControls.Jump.performed += ctx => Jump(); 
    }

    void OnEnable()
    {
        controls.Enable();
        StartCoroutine(InitialState()); // Start with 3 seconds of locked movement (blue) To Represent New State
    }

    void OnDisable()
    {
        controls.Disable();
    }

    void FixedUpdate()
    {
        if (isMovementLocked || isSitting) return; // Block movement if locked or sitting To Represent End

        Vector3 moveDirection = new Vector3(movementInput.x, 0, movementInput.y);

        // Move forward/backward or strafe left/right
        if (moveDirection != Vector3.zero)
        {
            Vector3 move = playerTrans.forward * movementInput.y + playerTrans.right * movementInput.x;
            playerRigid.MovePosition(playerRigid.position + move * w_speed * Time.deltaTime);
        }
    }

    void Update()
    {
        if (isMovementLocked || isSitting) return; // Prevent movement when locked or sitting

        // Handle animation and color transitions based on movement, Green for Movements
        if (movementInput.y > 0 && !walking) // Walk forward
        {
            walking = true;
            playerAnim.SetTrigger("walk");
            playerAnim.ResetTrigger("idle");
            playerAnim.ResetTrigger("walkback"); // Ensure walkback is reset when walking forward
            playerRenderer.material = greenMat; 
        }
        else if (movementInput.y < 0 && !walking) // Walk backwards
        {
            walking = true;
            playerAnim.SetTrigger("walkback");
            playerAnim.ResetTrigger("idle");
            playerAnim.ResetTrigger("walk"); // Ensure walk is reset when walking backward
            playerRenderer.material = greenMat; 
        }
        else if (movementInput.x != 0 && !walking) // Move left or right (strafing)
        {
            walking = true;
            playerAnim.SetTrigger("walk"); // Use walk animation for strafing as well
            playerAnim.ResetTrigger("idle");
            playerRenderer.material = greenMat; 
        }
        else if (movementInput == Vector2.zero && walking)
        {
            walking = false;
            playerAnim.SetTrigger("idle");
            playerRenderer.material = yellowMat; // Yellow when idle after movement
        }

        // Camera follow player smoothly
        Vector3 cameraTargetPos = playerTrans.position + new Vector3(0, 1, -3); // Example offset
        cameraTrans.position = Vector3.Lerp(cameraTrans.position, cameraTargetPos, Time.deltaTime * 3f); // Smooth follow
    }

    void Sit()
    {
        // Sit functionality with movement lock and red color To Represenr End
        isSitting = true;
        playerAnim.SetTrigger("sit");
        playerRenderer.material = redMat; // Red when sitting
    }

    void Jump()
    {
      
        if (!isSitting)
        {
            playerRigid.AddForce(Vector3.up * 5f, ForceMode.Impulse); 
        }
    }

    IEnumerator InitialState()
    {
        // Start game in blue state for 3 seconds with locked movement To Represent New
        playerRenderer.material = blueMat;
        yield return new WaitForSeconds(3f);

        // Unlock movement and set to purple (ready state)
        isMovementLocked = false;
        isReadyToRun = true;
        playerRenderer.material = purpleMat;
    }
}
