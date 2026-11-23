using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public enum FaseCombo { Livre, Preparacao, Ativo, Recuperacao }

    [Serializable]
    public class GolpeCombo
    {
        [Min(0f)] public float preparacao;
        [Min(0.01f)] public float duracaoAtiva;
        [Min(0f)] public float recuperacao;
        [Min(1)] public int dano;
        [Min(0f)] public float multiplicadorKnockback;

        public GolpeCombo(float preparacao, float ativa, float recuperacao,
            int dano, float knockback)
        {
            this.preparacao = preparacao;
            duracaoAtiva = ativa;
            this.recuperacao = recuperacao;
            this.dano = dano;
            multiplicadorKnockback = knockback;
        }
    }

    [Header("Movimento")]
    private Rigidbody2D rb;
    private Vector2 direcao;
    public float velocidade = 5f;
    public SpriteRenderer sprite;
    public Animator animator;

    [Header("Ataque")]
    // Mantidos para preservar as referências dos prefabs existentes.
    public BoxCollider2D attackHitbox;
    public GameObject attackPointerPrefab;
    public Vector2 attackOffset = new Vector2(0.8f, -0.2f);

    [Header("Combo")]
    [SerializeField] private GolpeCombo[] golpes = {
        new GolpeCombo(0.06f, 0.12f, 0.24f, 1, 0.25f),
        new GolpeCombo(0.06f, 0.12f, 0.24f, 1, 0.25f),
        new GolpeCombo(0.10f, 0.16f, 0.40f, 2, 1f)
    };
    [SerializeField, Min(0f)] private float janelaEncadeamento = 0.20f;
    [SerializeField, Range(0f, 1f)] private float fatorMovimentoAtacando = 0.5f;

    private FaseCombo faseAtual = FaseCombo.Livre;
    private int indiceGolpe = -1;
    private float tempoRestante;
    private bool proximoGolpeReservado;
    private bool olhandoEsquerda;
    private ataque hitboxAtual;

    public FaseCombo FaseAtual => faseAtual;
    // Zero indica que não existe combo em andamento.
    public int GolpeAtual => indiceGolpe + 1;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (sprite == null)
            sprite = GetComponent<SpriteRenderer>();
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void Mover(InputAction.CallbackContext context)
    {
        direcao = context.ReadValue<Vector2>();
        AtualizarAnimacaoMovimento();
    }

    public void Atacar(InputAction.CallbackContext context)
    {
        if (context.started)
            SolicitarAtaque();
    }

    public void SolicitarAtaque()
    {
        if (!isActiveAndEnabled || Time.timeScale <= 0f)
            return;

        if (faseAtual == FaseCombo.Livre)
        {
            if (golpes == null || golpes.Length != 3 ||
                Array.Exists(golpes, golpe => golpe == null) ||
                attackPointerPrefab == null ||
                attackPointerPrefab.GetComponent<ataque>() == null)
            {
                Debug.LogError("Configure três golpes e um AttackPoint com ataque.", this);
                return;
            }

            IniciarGolpe(0);
            return;
        }

        // Só guarda um comando, nos instantes finais da recuperação.
        if (faseAtual == FaseCombo.Recuperacao &&
            indiceGolpe < golpes.Length - 1 &&
            tempoRestante <= Mathf.Max(0f, janelaEncadeamento))
            proximoGolpeReservado = true;
    }

    private void IniciarGolpe(int indice)
    {
        indiceGolpe = indice;
        proximoGolpeReservado = false;
        AtualizarDirecao();
        faseAtual = FaseCombo.Preparacao;
        tempoRestante = Mathf.Max(0f, golpes[indice].preparacao);
        AtualizarAnimacaoMovimento();
    }

    private void Update()
    {
        if (faseAtual == FaseCombo.Livre)
            AtualizarDirecao();

        AtualizarCombo(Time.deltaTime);
    }

    private void AtualizarCombo(float delta)
    {
        if (faseAtual == FaseCombo.Livre || delta <= 0f)
            return;

        tempoRestante -= delta;
        if (tempoRestante > 0f)
            return;

        switch (faseAtual)
        {
            case FaseCombo.Preparacao:
                AbrirGolpe();
                break;
            case FaseCombo.Ativo:
                RemoverHitbox();
                faseAtual = FaseCombo.Recuperacao;
                tempoRestante = Mathf.Max(0f, golpes[indiceGolpe].recuperacao);
                break;
            case FaseCombo.Recuperacao:
                if (proximoGolpeReservado && indiceGolpe < golpes.Length - 1)
                    IniciarGolpe(indiceGolpe + 1);
                else
                    CancelarCombo();
                break;
        }
    }

    private void AbrirGolpe()
    {
        GolpeCombo golpe = golpes[indiceGolpe];
        faseAtual = FaseCombo.Ativo;
        tempoRestante = Mathf.Max(Time.fixedDeltaTime, golpe.duracaoAtiva);

        float posX = olhandoEsquerda ? -attackOffset.x : attackOffset.x;
        Vector3 posicao = transform.position +
            new Vector3(posX, attackOffset.y, 0f);
        GameObject instancia = Instantiate(
            attackPointerPrefab, posicao, Quaternion.Euler(0f, 0f, 90f));

        hitboxAtual = instancia.GetComponent<ataque>();
        hitboxAtual.Inicializar(this, golpe.dano,
            golpe.multiplicadorKnockback, olhandoEsquerda,
            new Vector3(posX, attackOffset.y, 0f));
    }

    public void CancelarCombo()
    {
        // Nenhum callback/coroutine pode reativar o golpe após o cancelamento.
        RemoverHitbox();
        faseAtual = FaseCombo.Livre;
        indiceGolpe = -1;
        tempoRestante = 0f;
        proximoGolpeReservado = false;
        AtualizarAnimacaoMovimento();
    }

    private void RemoverHitbox()
    {
        if (hitboxAtual == null)
            return;

        hitboxAtual.Encerrar();
        Destroy(hitboxAtual.gameObject);
        hitboxAtual = null;
    }

    private void OnDisable()
    {
        CancelarCombo();
        direcao = Vector2.zero;
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
        AtualizarAnimacaoMovimento();
    }

    private void FixedUpdate()
    {
        float fator = faseAtual == FaseCombo.Livre
            ? 1f : Mathf.Clamp01(fatorMovimentoAtacando);
        rb.linearVelocity = direcao * velocidade * fator;
    }

    private void AtualizarDirecao()
    {
        if (direcao.x != 0f)
            olhandoEsquerda = direcao.x < 0f;

        if (sprite != null)
            sprite.flipX = olhandoEsquerda;
    }

    private void AtualizarAnimacaoMovimento()
    {
        if (animator != null)
            animator.SetBool("estaAndando", direcao.sqrMagnitude > 0f);
    }
}
