using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManeger : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject playerPrefab2;
    [SerializeField] private Transform[] spawnPoint;

    private bool wasdAtivo = false;
    private bool gamepadAtivo = false;

    // Update is called once per frame
    void Update()
    {

        if (Keyboard.current != null && !wasdAtivo && Keyboard.current.wKey.wasPressedThisFrame)
        {
            var player = PlayerInput.Instantiate(playerPrefab, controlScheme: "WASD", pairWithDevice: Keyboard.current);

            if (spawnPoint.Length > 0)
            {
                player.transform.position = spawnPoint[0].position;
            }
            wasdAtivo = true;
        }

        foreach (var gamepad in Gamepad.all)
        {
            if (!gamepadAtivo && gamepad.buttonSouth.wasPressedThisFrame)
            {
                var player = PlayerInput.Instantiate(playerPrefab2, controlScheme: "Gamepad", pairWithDevice: gamepad);
                if (spawnPoint.Length > 1)
                {
                    player.transform.position = spawnPoint[1].position;
                }
                gamepadAtivo = true;
            }
        }
    }
}
