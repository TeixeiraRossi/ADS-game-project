using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{   
    private Rigidbody2D rb; //RB do Objeto
    private Vector2 direcao; //Direção do movimento
    public float velocidade = 5f; //Controle de velocidade

    private void Awake() //
    {
        rb = GetComponent<Rigidbody2D>(); //Guarda RB do objeto a que esta anexado
    }

    public void Mover(InputAction.CallbackContext context) //Função chamada pelo Input System, recebe o contexto da ação
    {
        direcao = context.ReadValue<Vector2>(); //Guarda direção recebida do Input System
    }

    private void FixedUpdate()
    {
        float desloc = velocidade * Time.fixedDeltaTime; //valor de tempo fixo da Unity
        Vector2 movimento = desloc * direcao;
        rb.MovePosition(rb.position + movimento);// movimenta o RB do objeto
    }
}
