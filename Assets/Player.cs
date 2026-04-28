using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{   
    private Rigidbody2D rb;
    private Vector2 direcao;
    public float velocidade = 5f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Mover(InputAction.CallbackContext context)
    {
        direcao = context.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        float desloc = velocidade * Time.fixedDeltaTime;
        Vector2 movimento = desloc * direcao;
        rb.MovePosition(rb.position + movimento);
    }
}
