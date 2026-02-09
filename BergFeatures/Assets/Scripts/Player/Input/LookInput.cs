using UnityEngine;
using UnityEngine.InputSystem;

public class LookInput : MonoBehaviour
{
    public Vector2 Value { get; private set; }

    private PlayerMain player;

    private void Awake()
    {
        player = GetComponent<PlayerMain>();
        if (player == null)
            Debug.LogError("LookInput needs PlayerMain on the same GameObject.");
    }

    private void OnEnable()
    {
        // If PlayerMain hasn't initialized Controls yet, try again next frame.
        // This avoids null errors even if Unity enables components in a weird order.
        StartCoroutine(BindNextFrame());
    }

    private System.Collections.IEnumerator BindNextFrame()
    {
        // wait one frame
        yield return null;

        if (player == null || player.Controls == null)
        {
            Debug.LogError("LookInput: Controls still null after waiting a frame. Check PlayerMain.");
            yield break;
        }

        player.Controls.Player.Look.performed += OnLook;
        player.Controls.Player.Look.canceled += OnLook;
    }

    private void OnDisable()
    {
        if (player == null || player.Controls == null) return;

        player.Controls.Player.Look.performed -= OnLook;
        player.Controls.Player.Look.canceled -= OnLook;
    }

    private void OnLook(InputAction.CallbackContext ctx)
    {
        Value = ctx.ReadValue<Vector2>();
    }
}
