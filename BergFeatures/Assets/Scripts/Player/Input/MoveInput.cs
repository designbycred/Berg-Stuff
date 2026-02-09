using UnityEngine;
using UnityEngine.InputSystem;

public class MoveInput : MonoBehaviour
{
    public Vector2 Value { get; private set; }

    private PlayerMain player;

    private void Awake()
    {
        player = GetComponent<PlayerMain>();
        if (player == null) player = GetComponentInParent<PlayerMain>();

        if (player == null)
            Debug.LogError("MoveInput couldn't find PlayerMain on this object or any parent.");
    }

    private void OnEnable()
    {
        StartCoroutine(BindNextFrame());
    }

    private System.Collections.IEnumerator BindNextFrame()
    {
        // Wait one frame so PlayerMain.OnEnable has time to create Controls
        yield return null;

        if (player == null || player.Controls == null)
        {
            Debug.LogError("MoveInput: PlayerMain or Controls is null after waiting a frame.");
            yield break;
        }

        player.Controls.Player.Move.performed += OnMove;
        player.Controls.Player.Move.canceled += OnMove;
    }

    private void OnDisable()
    {
        if (player == null || player.Controls == null) return;

        player.Controls.Player.Move.performed -= OnMove;
        player.Controls.Player.Move.canceled -= OnMove;
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        Value = ctx.ReadValue<Vector2>();
    }
}
