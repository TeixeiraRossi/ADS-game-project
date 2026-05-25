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
        {//cria o objeto player usando Prefab se tem teclado, grupo wasd desativado e tecla W pressionada
            var player = PlayerInput.Instantiate(playerPrefab, controlScheme: "WASD", pairWithDevice: Keyboard.current);

            if (spawnPoint.Length > 0)
            {
                player.transform.position = spawnPoint[0].position;
                //faz surgir no spawn point 1
            }
            wasdAtivo = true;
        }

        foreach (var gamepad in Gamepad.all)
        {
            if (!gamepadAtivo && gamepad.buttonSouth.wasPressedThisFrame)
            {//cria o objeto player usando Prefab se tem gamepad, grupo gamepad desativado e botão South pressionado
                var player = PlayerInput.Instantiate(playerPrefab2, controlScheme: "Gamepad", pairWithDevice: gamepad);
                if (spawnPoint.Length > 1)
                {
                    player.transform.position = spawnPoint[1].position;
                    //faz surgir no spawn point 2
                }
                gamepadAtivo = true;
            }
        }
    }
}
