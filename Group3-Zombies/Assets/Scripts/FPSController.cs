using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]

public class FPSController : MonoBehaviour
{
    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public float stamina = 100f;
    public float recovery = 5.0f;
    public bool isGrounded;
    CharacterController characterController;
    Vector3 moveDirection = Vector3.zero;
    Animator animator;
    public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
    public RotationAxes axes = RotationAxes.MouseXAndY;
    public float sensitivityX = 12f;
    public float sensitivityY = 12f;
    float rotationY = 0f;

    [HideInInspector]
    public bool canMove = true;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isGrounded = false;
    }

    void Update()
    {
        // We are grounded, so recalculate move direction based on axes
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        // Press Left Shift to run
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);
        //characterController.collisionFlags == CollisionFlags.Below -> checks if touching ground and nothing else
        if((characterController.collisionFlags & CollisionFlags.Below) != 0 && !isGrounded) {
            isGrounded = true;
            animator.SetBool("Jump", false);
        }

        if (Input.GetButton("Jump") && canMove && isGrounded)
        {
            moveDirection.y = jumpSpeed;
            isGrounded = false;
            animator.SetBool("Jump", true);
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
        // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
        // as an acceleration (ms^-2)
        if (!isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Set animations based on movement
        if(moveDirection.x != 0 || moveDirection.z != 0) {
            animator.SetBool("Movement", true);
        } else {
            animator.SetBool("Movement", false);
        }
        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);
        updateStamina(isRunning);

        // Player and Camera rotation
        if (axes == RotationAxes.MouseXAndY && canMove)
        {
            float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;
            rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
            rotationY = Mathf.Clamp(rotationY, -60f, 60f);
            transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
        }
    }

    void updateStamina(bool isSprinting)
    {
        if (isSprinting)
        {
            stamina -= 10 * Time.deltaTime;
            if (stamina < 0)
                stamina = 0;
        }
        else
        {
            stamina += recovery * Time.deltaTime;
            if (stamina > 100)
                stamina = 100;
           
        }
    }
}
