using UnityEngine;

public class Enemy : MonoBehaviour
{
    public enum EstadoInimigo
    {
        Parado,
        Perseguindo,
        Atordoado,
        Morto,
        PreparandoAtaque,
        Atacando,
        Recuperando
    }

    [Header("Movimento")]
    [SerializeField] private float velocidade = 3f;
    [SerializeField] private float distanciaDeParada = 1.5f;
    [SerializeField] private Animator animator;
    public SpriteRenderer sprite;

    [Header("Ataque")]
    [SerializeField] private ataqueDoInimigo ataqueInimigo;
    [SerializeField, Min(0f)] private float duracaoPreparacao = 0.4f;
    [SerializeField, Min(0.01f)] private float duracaoAtaque = 0.15f;
    [SerializeField, Min(0f)] private float duracaoRecuperacao = 0.6f;
    [SerializeField] private Color corPreparacao = new Color(1f, 0.5f, 0.2f, 1f);

    [Header("Vida")]
    [SerializeField] private int vidaMaxima = 3;

    [Header("Knockback")]
    [SerializeField] private float forcaKnockback = 1.5f;
    [SerializeField] private float duracaoStun = 0.2f;

    [Header("Estado atual — diagnóstico")]
    [SerializeField] private EstadoInimigo estadoAtual;

    private Transform player;
    private int vidaAtual;
    private float tempoRestanteEstado;
    private Color corOriginal;

    public EstadoInimigo EstadoAtual => estadoAtual;

    private void Awake()
    {
        vidaAtual = vidaMaxima;

        if (animator == null)
            animator = GetComponent<Animator>();

        if (sprite == null)
            sprite = GetComponent<SpriteRenderer>();

        if (sprite != null)
            corOriginal = sprite.color;

        if (ataqueInimigo == null)
            ataqueInimigo = GetComponentInChildren<ataqueDoInimigo>(true);

        TrocarEstado(EstadoInimigo.Parado);
    }

    private void OnDisable()
    {
        // Desativar o componente também interrompe qualquer golpe.
        TrocarEstado(estadoAtual == EstadoInimigo.Morto
            ? EstadoInimigo.Morto
            : EstadoInimigo.Parado);
    }

    private void Update()
    {
        if (estadoAtual == EstadoInimigo.Morto)
            return;

        // O inimigo fica imóvel durante todo o ciclo de ataque.
        switch (estadoAtual)
        {
            case EstadoInimigo.Atordoado:
                tempoRestanteEstado -= Time.deltaTime;
                if (tempoRestanteEstado > 0f)
                    return;
                break;

            case EstadoInimigo.PreparandoAtaque:
                if (!AlvoAtivo())
                {
                    TrocarEstado(EstadoInimigo.Recuperando);
                    return;
                }
                tempoRestanteEstado -= Time.deltaTime;
                if (tempoRestanteEstado <= 0f)
                    TrocarEstado(EstadoInimigo.Atacando);
                return;

            case EstadoInimigo.Atacando:
                tempoRestanteEstado -= Time.deltaTime;
                if (!AlvoAtivo() || tempoRestanteEstado <= 0f)
                    TrocarEstado(EstadoInimigo.Recuperando);
                return;

            case EstadoInimigo.Recuperando:
                tempoRestanteEstado -= Time.deltaTime;
                if (tempoRestanteEstado > 0f)
                    return;
                break;
        }

        AtualizarAlvo();

        if (player == null)
        {
            TrocarEstado(EstadoInimigo.Parado);
            return;
        }

        float distancia = Vector2.Distance(
            transform.position,
            player.position
        );

        TrocarEstado(
            distancia > distanciaDeParada
                ? EstadoInimigo.Perseguindo
                : ataqueInimigo != null && ataqueInimigo.isActiveAndEnabled
                    ? EstadoInimigo.PreparandoAtaque
                    : EstadoInimigo.Parado
        );

        if (estadoAtual == EstadoInimigo.Perseguindo)
            Perseguir();
    }

    private bool AlvoAtivo()
    {
        return player != null && player.gameObject.activeInHierarchy;
    }

    private void AtualizarAlvo()
    {
        // Mantém o mesmo alvo enquanto ele estiver ativo.
        if (player != null && player.gameObject.activeInHierarchy)
            return;

        player = null;

        GameObject jogador =
            GameObject.FindGameObjectWithTag("Player");

        if (jogador != null)
            player = jogador.transform;
    }

    private void Perseguir()
    {
        Vector2 direcao =
            (Vector2)(player.position - transform.position);

        if (sprite != null && direcao.x != 0f)
            sprite.flipX = direcao.x < 0f;

        // Preserva a coordenada Z do inimigo.
        Vector3 destino = new Vector3(
            player.position.x,
            player.position.y,
            transform.position.z
        );

        // Limita o passo para não avançar além da distância de parada.
        float distancia = direcao.magnitude;
        float passo = Mathf.Min(
            Mathf.Max(0f, velocidade) * Time.deltaTime,
            Mathf.Max(0f, distancia - distanciaDeParada)
        );

        transform.position = Vector3.MoveTowards(
            transform.position,
            destino,
            passo
        );
    }

    private void TrocarEstado(EstadoInimigo novoEstado)
    {
        // Toda saída do ataque desliga a hitbox imediatamente.
        if (ataqueInimigo != null)
            ataqueInimigo.EncerrarGolpe();

        estadoAtual = novoEstado;

        if (sprite != null)
            sprite.color = novoEstado == EstadoInimigo.PreparandoAtaque
                ? corPreparacao
                : corOriginal;

        switch (novoEstado)
        {
            case EstadoInimigo.PreparandoAtaque:
                tempoRestanteEstado = Mathf.Max(0f, duracaoPreparacao);
                // A direção fica fixa durante a preparação e o golpe.
                if (AlvoAtivo())
                {
                    float direcaoX = player.position.x - transform.position.x;
                    if (sprite != null && direcaoX != 0f)
                        sprite.flipX = direcaoX < 0f;

                    ataqueInimigo.Orientar(
                        direcaoX < 0f || (direcaoX == 0f && sprite != null && sprite.flipX)
                    );
                }
                break;
            case EstadoInimigo.Atacando:
                tempoRestanteEstado = Mathf.Max(Time.fixedDeltaTime, duracaoAtaque);
                if (ataqueInimigo != null)
                    ataqueInimigo.IniciarGolpe();
                break;
            case EstadoInimigo.Recuperando:
                tempoRestanteEstado = Mathf.Max(0f, duracaoRecuperacao);
                break;
            case EstadoInimigo.Atordoado:
                tempoRestanteEstado = Mathf.Max(0f, duracaoStun);
                break;
        }

        if (animator != null)
        {
            animator.SetBool(
                "estaAndando",
                novoEstado == EstadoInimigo.Perseguindo
            );
        }
    }

    public void ReceberDano(int dano)
    {
        if (estadoAtual == EstadoInimigo.Morto || dano <= 0)
            return;

        vidaAtual = Mathf.Max(0, vidaAtual - dano);
        Debug.Log("Inimigo recebeu dano! Vida: " + vidaAtual);

        // Dano fatal tem prioridade sobre stun e knockback.
        if (vidaAtual == 0)
        {
            Morrer();
            return;
        }

        AtualizarAlvo();
        AplicarKnockback();

        // Outro golpe reinicia a duração, sem callbacks pendentes.
        // O stun funciona mesmo quando não existe alvo.
        TrocarEstado(EstadoInimigo.Atordoado);
    }

    private void AplicarKnockback()
    {
        if (player == null)
            return;

        Vector2 direcaoKnockback =
            ((Vector2)(transform.position - player.position)).normalized;

        transform.position +=
            (Vector3)(direcaoKnockback * forcaKnockback);
    }

    private void Morrer()
    {
        TrocarEstado(EstadoInimigo.Morto);
        Debug.Log("Inimigo morreu!");
        Destroy(gameObject);
    }
}
