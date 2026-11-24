using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private static readonly List<Player> jogadoresAtivos = new List<Player>();

    internal static IReadOnlyList<Player> JogadoresAtivos => jogadoresAtivos;

    [Header("Movimento")]
    private Rigidbody2D rb;
    private BarraVida barraVida;
    private Vector2 direcao;
    public float velocidade = 5f;
    public SpriteRenderer sprite;
    public Animator animator;

    [Header("Esquiva")]
    [SerializeField, Min(0.01f)] private float duracaoEsquiva = 0.16f;
    [SerializeField, Min(0.1f)] private float velocidadeEsquiva = 12f;
    [SerializeField, Min(0.1f)] private float custoEsquiva = 2f;
    private Vector2 direcaoEsquiva;
    private float tempoEsquivaRestante;

    public bool EstaEsquivando { get; private set; }

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
        barraVida = GetComponent<BarraVida>();
    }

    private void OnEnable()
    {
        jogadoresAtivos.Add(this);
    }

    private void OnDisable()
    {
        jogadoresAtivos.Remove(this);

        if (EstaEsquivando)
            rb.linearVelocity = Vector2.zero;

        EstaEsquivando = false;
        tempoEsquivaRestante = 0f;
    }

    // Recebe a direção do movimento através do Input System.
    public void Mover(InputAction.CallbackContext context)
    {
        direcao = context.ReadValue<Vector2>();
        AtualizarAnimacao();
    }

    // Inicia o ataque quando o botão é pressionado.
    public void Atacar(InputAction.CallbackContext context)
    {
        if (context.started && !atacando && !EstaEsquivando)
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

    public void Esquivar(InputAction.CallbackContext context)
    {
        if (!context.performed || !isActiveAndEnabled || Time.timeScale <= 0f ||
            EstaEsquivando || atacando || barraVida == null ||
            !barraVida.TentarGastarEnergia(custoEsquiva))
            return;

        direcaoEsquiva = direcao == Vector2.zero
            ? (olhandoEsquerda ? Vector2.left : Vector2.right)
            : direcao.normalized;
        tempoEsquivaRestante = Mathf.Max(0.01f, duracaoEsquiva);
        EstaEsquivando = true;
        AtualizarAnimacao();
    }

    private void FixedUpdate()
    {
        if (EstaEsquivando)
        {
            if (tempoEsquivaRestante > 0f)
            {
                float passo = Mathf.Min(tempoEsquivaRestante, Time.fixedDeltaTime);
                rb.linearVelocity = direcaoEsquiva * (velocidadeEsquiva * passo / Time.fixedDeltaTime);
                tempoEsquivaRestante -= passo;
                return;
            }

            EstaEsquivando = false;
            AtualizarAnimacao();
        }

        rb.linearVelocity = direcao * velocidade;
    }

    private void Update()
    {
        Flip(EstaEsquivando ? direcaoEsquiva : direcao);
    }

    private void AtualizarAnimacao()
    {
        animator.SetBool("estaAndando", EstaEsquivando || direcao != Vector2.zero);
    }

    // Atualiza a direção visual do Player de acordo com o movimento.
    private void Flip(Vector2 movimento)
    {
        if (movimento.x > 0)
        {
            olhandoEsquerda = false;
            sprite.flipX = false;
        }
        else if (movimento.x < 0)
        {
            olhandoEsquerda = true;
            sprite.flipX = true;
        }
    }
}