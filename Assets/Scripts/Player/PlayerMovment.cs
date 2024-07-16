using Photon.Pun;
using UnityEngine;

public class PlayerMovment : MonoBehaviour
{
    [Header("Refernces")]
    [SerializeField] CharacterController characerController;
    [SerializeField] Transform groundCheck;

    [Space(10f)]

    [Header("Character Forces Parameters")]
    [SerializeField] float moveSpeed = 3;
    [SerializeField] float runSpeed = 6;
    [SerializeField] float gravity = -9.8f;
    [SerializeField] float JumpHeight = 3f;

    [Space(10f)]

    [Header("isGrounded Parameters")]
    [SerializeField] float groundDistance = 1.4f;
    [SerializeField] LayerMask groundMask;

    private Vector3 moveDirection;
    private Vector3 velocity;
    private bool isGrounded;

    private void Update()
    {
        ApplyGravity();
        Movment();
        playerJump();
    }

    private void ApplyGravity()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        velocity.y += gravity * Time.deltaTime;
        characerController.Move(velocity * Time.deltaTime);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    private void Movment()
    {

        float Xmovment = Input.GetAxisRaw("Horizontal");
        float Zmovment = Input.GetAxisRaw("Vertical");

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        moveDirection = (forward * Zmovment) + (right * Xmovment).normalized;

        float speedBool = Input.GetKey(KeyCode.LeftShift) ? runSpeed : moveSpeed;
        characerController.Move(moveDirection * speedBool * Time.deltaTime);

    }

    private void playerJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            ApplyJumpVelocity();
        }
    }

    void ApplyJumpVelocity()
    {
        // Equation for a velocity jump.
        velocity.y = Mathf.Sqrt(JumpHeight * -2f * gravity);
    }
}
