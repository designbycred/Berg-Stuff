using UnityEngine;

public class PlayerMain : MonoBehaviour
{
    public PlayerMotor Motor { get; private set; }
    public PlayerControls Controls { get; private set; }

    private Walk walk;
    private Jetpack jetpack;

    private void Awake()
    {
        Motor = GetComponent<PlayerMotor>();
        walk = GetComponent<Walk>();
        jetpack = GetComponentInChildren<Jetpack>(); // child mount OK
    }

    private void OnEnable()
    {
        if (Controls == null)
            Controls = new PlayerControls();

        Controls.Player.Enable();

        if (walk == null) walk = GetComponent<Walk>();
        if (jetpack == null) jetpack = GetComponentInChildren<Jetpack>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        if (Controls != null)
            Controls.Player.Disable();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        if (Motor == null) return;

        float dt = Time.deltaTime;

        Vector3 planar = Vector3.zero;

        if (walk != null)
            planar += walk.GetPlanarVelocity(dt);

        if (jetpack != null)
            planar += jetpack.GetPlanarVelocity(dt);

        Motor.Move(planar, dt);
    }
}
