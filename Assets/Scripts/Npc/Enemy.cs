using UnityEngine;

public class Enemy : MonoBehaviour
{
    public enum EstadoInimigo
    {
        Parado,
        Perseguindo,
        Atordoado,
        Morto
    }

    [Header("Movimento")]
    [SerializeField] private float velocidade = 3f;
    [SerializeField] private float distanciaDeParada = 1.5f;
    [SerializeField] private Animator animator;
    public SpriteRenderer sprite;

    [Header("Vida")]
    [SerializeField] private int vidaMaxima = 3;

    [Header("Knockback")]
    [SerializeField] private float forcaKnockback = 1.5f;
    [SerializeField] private float duracaoStun = 0.2f;

    [Header("Estado atual — diagnóstico")]
    [SerializeField] private EstadoInimigo estadoAtual;

    private Transform player;
    private int vidaAtual;
    private float tempoRestanteStun;

    public EstadoInimigo EstadoAtual => estadoAtual;

    private void Awake()
    {
        vidaAtual = vidaMaxima;

        if (animator == null)
            animator = GetComponent<Animator>();

        if (sprite == null)
            sprite = GetComponent<SpriteRenderer>();

        TrocarEstado(EstadoInimigo.Parado);
    }

    private void Update()
    {
        if (estadoAtual == EstadoInimigo.Morto)
            return;

        if (estadoAtual == EstadoInimigo.Atordoado)
        {
            tempoRestanteStun -= Time.deltaTime;

            if (tempoRestanteStun > 0f)
                return;

            // Ao terminar o stun, reavalia o alvo e a distância.
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
                : EstadoInimigo.Parado
        );

        if (estadoAtual == EstadoInimigo.Perseguindo)
            Perseguir();
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
        estadoAtual = novoEstado;

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
        tempoRestanteStun = Mathf.Max(0f, duracaoStun);
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
