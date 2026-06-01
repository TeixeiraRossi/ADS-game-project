using System.Collections;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{   
    [Header("Movimento")]
    private Rigidbody2D rb; //RB do Objeto
    private Vector2 direcao; //Direção do movimento
    public float velocidade = 5f; //Controle de velocidade
    public SpriteRenderer sprite; //Sprite do boneco

    [Header("Ataque")]
    public BoxCollider2D attackHitbox;
    public float attackDuration = 0.2f;
    private bool atacando;

    private void Awake() //
    {
        rb = GetComponent<Rigidbody2D>(); //Guarda RB do objeto a que esta anexado

        attackHitbox.enabled = false;
    }

    public void Mover(InputAction.CallbackContext context) //Função chamada pelo Input System, recebe o contexto da ação
    {
        direcao = context.ReadValue<Vector2>(); //Guarda direção recebida do Input System
    }

    public void Atacar(InputAction.CallbackContext context)
    {
        Debug.Log("ATACAR CHAMADO");
        if (context.performed && !atacando)
        {
            Debug.Log("atq");
            StartCoroutine(AttackCoroutine());
        }
    }

    private IEnumerator AttackCoroutine()
    {
        atacando = true;
        attackHitbox.enabled = true;
        yield return new WaitForSeconds(attackDuration);
        attackHitbox.enabled = false;
        atacando = false;
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(
            direcao.x * velocidade,
            rb.linearVelocity.y
            );
    }

    private void Update()
    {
        Flip();
    }

    private void Flip()
    {
        if (direcao.x > 0)
        {
            sprite.flipX = false;

            attackHitbox.transform.localScale = new Vector2(0.7f,0);

        }else if (direcao.x < 0)
        {
            sprite.flipX = true;

            attackHitbox.transform.localPosition = new Vector2(-0.7f, 0);
        }
    }

}
