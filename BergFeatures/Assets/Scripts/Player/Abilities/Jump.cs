using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Ground jump ability.
/// Reads the Jump action and sets PlayerMotor.VerticalVelocity when grounded.
/// </summary>
public class Jump : MonoBehaviour
{
    [SerializeField] private float jumpSpeed = 6f;

    private PlayerMain player;
    private PlayerMotor motor;

    private void Awake()
    {
        player = GetComponent<PlayerMain>();
        motor = GetComponent<PlayerMotor>();

        if (player == null) Debug.LogError("Jump requires PlayerMain on the same GameObject.");
        if (motor == null) Debug.LogError("Jump requires PlayerMotor on the same GameObject.");
    }

    private void OnEnable()
    {
        // If Controls isn't ready yet, bind next frame (safe with our current setup)
        StartCoroutine(BindNextFrame());
    }

    private System.Collections.IEnumerator BindNextFrame()
    {
        yield return null;

        if (player == null || player.Controls == null)
        {
            Debug.LogError("Jump: PlayerMain.Controls is null. Check PlayerMain initialization.");
            yield break;
        }

        player.Controls.Player.Jump.performed += OnJump;
    }

    private void OnDisable()
    {
        if (player == null || player.Controls == null) return;

        player.Controls.Player.Jump.performed -= OnJump;
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (motor == null) return;

        if (motor.IsGrounded)
        {
            motor.VerticalVelocity = jumpSpeed;
        }
    }
}
