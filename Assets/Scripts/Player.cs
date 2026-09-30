using System.Collections;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Movimento")]
    private Rigidbody2D rb;
    private Vector2 direcao;
    public float velocidade = 5f;
    public SpriteRenderer sprite;

    [Header("Ataque")]
    public BoxCollider2D attackHitbox;
    public GameObject attackPointerPrefab;
    public float attackDuration = 0.2f;
    private bool atacando;
    private bool olhandoEsquerda;
    public Vector2 attackOffset = new Vector2(0.8f, -0.2f);

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Recebe a direção do movimento através do Input System.
    public void Mover(InputAction.CallbackContext context)
    {
        direcao = context.ReadValue<Vector2>();
    }

    // Inicia o ataque quando o botão é pressionado.
    public void Atacar(InputAction.CallbackContext context)
    {
        if (context.started && !atacando)
        {
            if (attackPointerPrefab != null)
            {
                float posX = olhandoEsquerda ? -attackOffset.x : attackOffset.x;
                Vector3 spawnPos = transform.position + new Vector3(posX, attackOffset.y, 0f);
                Quaternion spawnRot = Quaternion.Euler(0, 0, 90);

                GameObject ptr = Instantiate(
                    attackPointerPrefab,
                    spawnPos,
                    spawnRot
                );

                var sr = ptr.GetComponentInChildren<SpriteRenderer>();

                if (sr)
                    sr.flipY = olhandoEsquerda;

                Destroy(ptr, attackDuration);
            }

            StartCoroutine(AttackCoroutine());
        }
    }

    // Controla o pequeno intervalo em que o Player está realizando o ataque.
    private IEnumerator AttackCoroutine()
    {
        atacando = true;

        yield return new WaitForSeconds(attackDuration);

        atacando = false;
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(
            direcao.x * velocidade,
            direcao.y * velocidade
        );
    }

    private void Update()
    {
        Flip();
    }

    // Atualiza a direção visual do Player de acordo com o movimento.
    private void Flip()
    {
        if (direcao.x > 0)
        {
            olhandoEsquerda = false;
            sprite.flipX = false;
        }
        else if (direcao.x < 0)
        {
            olhandoEsquerda = true;
            sprite.flipX = true;
        }
    }
}