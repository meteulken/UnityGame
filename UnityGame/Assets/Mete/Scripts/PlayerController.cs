using UnityEngine;
using System.Collections;


public class PlayerController : MonoBehaviour
{
    [SerializeField] Transform playerCamera = null;
    [SerializeField] float mouseSensitivity = 3.5f;
    [SerializeField] float gravity = -13.0f;
    [SerializeField] float moveSmoothTime = 0.3f;
    [SerializeField] float mouseSmoothTime = 0.03f;
    [SerializeField] bool lockCursor = true;

    [SerializeField] float walkSpeed = 5.0f;
    [SerializeField] float runSpeed = 10.0f;
    [SerializeField] float runBuildUpSpeed = 0.5f;
    [SerializeField] KeyCode runKey = KeyCode.LeftShift;

    [SerializeField] AnimationCurve jumpFallOff;
    [SerializeField] float jumpMultiplier = 10.0f;
    [SerializeField] KeyCode jumpKey = KeyCode.Space;

    [SerializeField] float crouchHeight = 0.5f;
    [SerializeField] float originalhHeight = 0.5f;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
    [SerializeField] float crouchSpeed = 2.5f;

    private bool isJumping = false;
    private bool isCrouching = false;
    private float movementSpeed = 0.0f;
    private float cameraHeight = 0.0f;
    private Vector2 currentDir = Vector2.zero;
    private Vector2 currentDirVelocity = Vector2.zero;
    private Vector2 currentMouseDelta = Vector2.zero;
    private Vector2 currentMouseDeltaVelocity = Vector2.zero;
    private float cameraPitch = 0.0f;
    private float velocityY = 0.0f;
    private CharacterController controller = null;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        cameraHeight = playerCamera.localPosition.y;
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        UpdateMouseLook();
        UpdateMovement();
    }

    private void UpdateMouseLook()
    {
        Vector2 targetMouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseDeltaVelocity, mouseSmoothTime);

        cameraPitch = Mathf.Clamp(cameraPitch - currentMouseDelta.y * mouseSensitivity, -90.0f, 90.0f);

        playerCamera.localEulerAngles = Vector3.right * cameraPitch;

        transform.Rotate(Vector3.up * currentMouseDelta.x * mouseSensitivity);
    }

    private void UpdateMovement()
    {
    Vector2 targetDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

    currentDir = Vector2.SmoothDamp(currentDir, targetDir, ref currentDirVelocity, moveSmoothTime);

    if (controller.isGrounded)
    {
        velocityY = 0.0f;
    }

    velocityY += gravity * Time.deltaTime;

    Vector3 velocity = (transform.forward * currentDir.y + transform.right * currentDir.x) * movementSpeed + Vector3.up * velocityY;

    controller.Move(velocity * Time.deltaTime);

    SetMovementSpeed();
    JumpInput();
    CrouchInput();
    
    if (isCrouching)
    {
        isJumping = false;
        movementSpeed = crouchSpeed;
    }
    isCrouching = controller.height == crouchHeight;

}


private void CrouchInput()
{
    if (Input.GetKeyDown(crouchKey))
    {
        controller.height = crouchHeight;
        movementSpeed = crouchSpeed;
        isCrouching = true;
    }
    if (Input.GetKeyUp(crouchKey))
    {
        controller.height = originalhHeight;
        movementSpeed = walkSpeed;
        isCrouching = false;
    }
}


private void SetMovementSpeed()
{
    if (!isCrouching && Input.GetKey(runKey))
        movementSpeed = Mathf.Lerp(movementSpeed, runSpeed, Time.deltaTime * runBuildUpSpeed);
    else
        movementSpeed = Mathf.Lerp(movementSpeed, walkSpeed, Time.deltaTime * runBuildUpSpeed);
}

private void JumpInput()
{
    if (Input.GetKeyDown(jumpKey) && controller.isGrounded && !isCrouching && !isJumping)
    {
        isJumping = true;
        StartCoroutine(JumpEvent());
    }
}

private IEnumerator JumpEvent()
{
    float timeInAir = 0.0f;
    float jumpForce = 0.0f;

    do
    {
        jumpForce = jumpFallOff.Evaluate(timeInAir);
        controller.Move(Vector3.up * jumpForce * jumpMultiplier * Time.deltaTime);
        timeInAir += Time.deltaTime;
        yield return null;
    } while (jumpForce > 0.0f && !controller.isGrounded && controller.collisionFlags != CollisionFlags.Above);

    isJumping = false;
}
}