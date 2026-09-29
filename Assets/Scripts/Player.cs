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

    [Header("Vida")]
    [SerializeField] private int vidaMaxima = 5;
    private int vidaAtual;

    [SerializeField] private BarraVida barraVida;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Começa o Player com a vida máxima configurada no Inspector.
        vidaAtual = vidaMaxima;

        // Inicializa a barra mostrando a vida máxima.
        if (barraVida != null)
        {
            barraVida.AtualizarVida(vidaAtual, vidaMaxima);
        }
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

        /*attackHitbox.enabled = true;*/

        yield return new WaitForSeconds(attackDuration);

        /*attackHitbox.enabled = false;*/

        atacando = false;
    }

    // Reduz a vida do Player e verifica se ele deve morrer.
    public void ReceberDano(int dano)
    {
        vidaAtual -= dano;

        Debug.Log("Player recebeu dano! Vida: " + vidaAtual);

        if (barraVida != null)
        {
            barraVida.AtualizarVida(vidaAtual, vidaMaxima);
        }

        if (vidaAtual <= 0)
        {
            Morrer();
        }
    }

    public void ReceberBuff(Buff.TipoBuff tipo, float quantidade)
    {
        switch (tipo)
        {
            case Buff.TipoBuff.vida:
                vidaAtual += Mathf.RoundToInt(quantidade);
                vidaAtual = Mathf.Clamp(vidaAtual, 0, vidaMaxima);
                barraVida.AtualizarVida(vidaAtual, vidaMaxima);
                Debug.Log("Player recebeu buff de vida! Vida: " + vidaAtual);
                break;
            case Buff.TipoBuff.energia:
                // Aqui você pode implementar a lógica para energia, se houver.
                Debug.Log("Player recebeu buff de energia!");
                break;
            case Buff.TipoBuff.xp:
                // Aqui você pode implementar a lógica para XP, se houver.
                Debug.Log("Player recebeu buff de XP!");
                break;
        }
        if (barraVida != null)
        {
            barraVida.AtualizarVida(vidaAtual, vidaMaxima);
        }
    }

    // Executado quando a vida do Player chega a zero.
    private void Morrer()
    {
        Debug.Log("Player morreu!");

        // Por enquanto apenas desativa o Player.
        // Depois podemos substituir por animação, respawn ou Game Over.
        gameObject.SetActive(false);
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