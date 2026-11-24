using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SpawnEnemyControl : MonoBehaviour
{
    public enum EstadoPartida
    {
        Aguardando,
        GerandoInimigos,
        EmCombate,
        Intervalo,
        Vitoria,
        Derrota
    }

    [Header("Spawn")]
    // Preserva a referência já configurada na cena.
    [SerializeField] private GameObject enemy;
    [SerializeField] private Transform[] pontosSpawn = new Transform[0];
    [SerializeField, Min(0.1f)] private float intervaloEntreSpawns = 0.6f;
    [SerializeField, Min(1)] private int limiteInimigosVivos = 6;
    [SerializeField, Min(0f)] private float distanciaSpawnPadrao = 6f;

    [Header("Progressão das ondas")]
    [SerializeField, Min(1)] private int totalOndas = 5;
    [SerializeField, Min(1)] private int inimigosPrimeiraOnda = 3;
    [SerializeField, Min(0)] private int aumentoPorOnda = 2;
    [SerializeField, Min(0f)] private float intervaloEntreOndas = 5f;

    [Header("Interface")]
    [SerializeField] private bool mostrarHUD = true;

    private readonly List<Enemy> inimigosDaOnda = new List<Enemy>();
    private EstadoPartida estadoAtual = EstadoPartida.Aguardando;
    private int ondaAtual;
    private int inimigosParaGerar;
    private int inimigosNestaOnda;
    private int indiceSpawn;
    private float tempoRestante;
    private GUIStyle estiloHUD;
    private GUIStyle estiloTitulo;
    private GUIStyle estiloDetalhe;
    private bool recarregandoCena;

    public EstadoPartida EstadoAtual => estadoAtual;
    public int OndaAtual => ondaAtual;
    public int InimigosRestantes => inimigosParaGerar + inimigosDaOnda.Count;

    private bool PartidaEmAndamento =>
        estadoAtual == EstadoPartida.GerandoInimigos ||
        estadoAtual == EstadoPartida.EmCombate ||
        estadoAtual == EstadoPartida.Intervalo;

    private void OnTriggerEnter2D(Collider2D colisao)
    {
        TentarIniciarPorJogador(colisao);
    }

    private void OnTriggerStay2D(Collider2D colisao)
    {
        // Permite iniciar também após reativar o controlador na arena.
        if (estadoAtual == EstadoPartida.Aguardando)
            TentarIniciarPorJogador(colisao);
    }

    private void TentarIniciarPorJogador(Collider2D colisao)
    {
        Player jogador = colisao.GetComponentInParent<Player>();
        if (jogador != null && jogador.CompareTag("Player"))
            IniciarPartida();
    }

    public void IniciarPartida()
    {
        // Vários colliders/jogadores ou reentradas não duplicam a partida.
        if (estadoAtual != EstadoPartida.Aguardando || !isActiveAndEnabled)
            return;

        if (enemy == null || enemy.GetComponent<Enemy>() == null ||
            !enemy.activeSelf)
        {
            Debug.LogError("Configure um prefab ativo com Enemy na raiz.", this);
            return;
        }

        // Não gera ondas antes de o jogador entrar na partida.
        if (!ExisteJogadorAtivo())
            return;

        indiceSpawn = 0;
        ondaAtual = 0;
        IniciarProximaOnda();
    }

    private void Update()
    {
        // Inclusive um spawn pendente com tempo zero aguarda sair da pausa.
        if (!PartidaEmAndamento || Time.deltaTime <= 0f)
            return;

        // Conta somente inimigos criados por este controlador.
        // Desativar um inimigo vivo não equivale a derrotá-lo.
        inimigosDaOnda.RemoveAll(inimigo =>
            inimigo == null ||
            inimigo.EstadoAtual == Enemy.EstadoInimigo.Morto);

        if (!ExisteJogadorAtivo())
        {
            EncerrarPartida(EstadoPartida.Derrota);
            return;
        }

        switch (estadoAtual)
        {
            case EstadoPartida.GerandoInimigos:
                // O limite impede que ondas maiores sobrecarreguem a arena.
                tempoRestante -= Time.deltaTime;
                if (inimigosDaOnda.Count >= Mathf.Max(1, limiteInimigosVivos) ||
                    tempoRestante > 0f)
                    return;

                GerarInimigo();
                inimigosParaGerar--;
                tempoRestante = Mathf.Max(0.1f, intervaloEntreSpawns);
                if (inimigosParaGerar == 0)
                    estadoAtual = EstadoPartida.EmCombate;
                break;

            case EstadoPartida.EmCombate:
                if (inimigosDaOnda.Count > 0)
                    return;

                if (ondaAtual >= Mathf.Max(1, totalOndas))
                {
                    EncerrarPartida(EstadoPartida.Vitoria);
                    return;
                }

                estadoAtual = EstadoPartida.Intervalo;
                tempoRestante = Mathf.Max(0f, intervaloEntreOndas);
                Debug.Log("Onda " + ondaAtual + " concluída.", this);
                break;

            case EstadoPartida.Intervalo:
                tempoRestante -= Time.deltaTime;
                if (tempoRestante <= 0f)
                    IniciarProximaOnda();
                break;
        }
    }

    private void IniciarProximaOnda()
    {
        ondaAtual++;
        inimigosNestaOnda = Mathf.Max(1, inimigosPrimeiraOnda) +
            (ondaAtual - 1) * Mathf.Max(0, aumentoPorOnda);
        inimigosParaGerar = inimigosNestaOnda;
        tempoRestante = 0f;
        estadoAtual = EstadoPartida.GerandoInimigos;
        Debug.Log("Onda " + ondaAtual + ": " + inimigosNestaOnda + " inimigos.", this);
    }

    private void GerarInimigo()
    {
        Vector3 posicao = transform.position +
            Vector3.right * distanciaSpawnPadrao * (indiceSpawn % 2 == 0 ? 1f : -1f);
        Quaternion rotacao = transform.rotation;

        // Pontos opcionais permitem adaptar a arena sem alterar o código.
        if (pontosSpawn != null && pontosSpawn.Length > 0)
        {
            // Ignora entradas vazias; se todas estiverem vazias, usa o padrão.
            for (int tentativa = 0; tentativa < pontosSpawn.Length; tentativa++)
            {
                Transform ponto = pontosSpawn[(indiceSpawn + tentativa) % pontosSpawn.Length];
                if (ponto == null)
                    continue;

                posicao = ponto.position;
                rotacao = ponto.rotation;
                break;
            }
        }

        indiceSpawn++;
        GameObject instancia = Instantiate(enemy, posicao, rotacao);
        inimigosDaOnda.Add(instancia.GetComponent<Enemy>());
    }

    private bool ExisteJogadorAtivo()
    {
        foreach (GameObject candidato in GameObject.FindGameObjectsWithTag("Player"))
        {
            Player jogador = candidato.GetComponent<Player>();
            if (jogador != null && jogador.isActiveAndEnabled)
                return true;
        }

        return false;
    }

    private void EncerrarPartida(EstadoPartida resultado)
    {
        estadoAtual = resultado;
        // Encerra também os inimigos restantes após perder a partida.
        if (resultado == EstadoPartida.Derrota)
            LimparInimigos();

        inimigosParaGerar = 0;
        tempoRestante = 0f;
        Debug.Log(resultado == EstadoPartida.Vitoria
            ? "Vitória! Todas as ondas foram concluídas."
            : "Derrota! Não há jogadores ativos.", this);
    }

    public void ReiniciarPartida()
    {
        // Os jogadores não são recriados aqui: a entrada/respawn pertence
        // ao PlayerInputManeger. Recarregue a cena após uma derrota.
        LimparInimigos();
        ondaAtual = 0;
        estadoAtual = EstadoPartida.Aguardando;
        IniciarPartida();
    }

    private void OnDisable()
    {
        LimparInimigos();
        ondaAtual = 0;
        estadoAtual = EstadoPartida.Aguardando;
    }

    private void LimparInimigos()
    {
        foreach (Enemy inimigo in inimigosDaOnda)
        {
            if (inimigo == null)
                continue;

            // Impede ataques enquanto Destroy aguarda o fim do frame.
            inimigo.gameObject.SetActive(false);
            Destroy(inimigo.gameObject);
        }

        inimigosDaOnda.Clear();
        inimigosParaGerar = 0;
        tempoRestante = 0f;
    }

    private void RecarregarCena()
    {
        if (recarregandoCena)
            return;

        Scene cena = gameObject.scene;
        if (cena.buildIndex < 0)
        {
            Debug.LogError("Inclua a cena da arena na lista de cenas do build para reiniciar a partida.", this);
            return;
        }

        recarregandoCena = true;
        SceneManager.LoadSceneAsync(cena.buildIndex, LoadSceneMode.Single);
    }

    private void OnGUI()
    {
        if (!mostrarHUD)
            return;

        if (estiloHUD == null)
        {
            estiloHUD = new GUIStyle(GUI.skin.box)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            estiloTitulo = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            estiloDetalhe = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
        }

        int ondas = Mathf.Max(1, totalOndas);
        float largura = Mathf.Min(400f, Screen.width - 24f);
        float esquerda = (Screen.width - largura) / 2f;

        switch (estadoAtual)
        {
            case EstadoPartida.Aguardando:
                if (PlayerInput.all.Count > 0)
                {
                    GUI.Box(new Rect(esquerda, Screen.height - 68f, largura, 56f),
                        "Vá para a direita para iniciar a primeira onda.", estiloHUD);
                    break;
                }

                DesenharPainel(
                    new Rect(esquerda, (Screen.height - 148f) / 2f, largura, 148f),
                    "Sobreviva na arena",
                    "Derrote todos os inimigos de " + ondas + " ondas.\n" +
                    "W para entrar  |  WASD para mover\n" +
                    "J para atacar  |  Espaço para esquivar",
                    false);
                break;

            case EstadoPartida.Vitoria:
            case EstadoPartida.Derrota:
                bool venceu = estadoAtual == EstadoPartida.Vitoria;
                DesenharPainel(
                    new Rect(esquerda, (Screen.height - 148f) / 2f, largura, 148f),
                    venceu ? "Vitória!" : "Derrota",
                    venceu
                        ? "Você concluiu todas as " + ondas + " ondas."
                        : "Você chegou à onda " + ondaAtual + " de " + ondas + ".",
                    true);
                break;

            case EstadoPartida.Intervalo:
                GUI.Box(new Rect(esquerda, 12f, largura, 72f),
                    "Onda " + ondaAtual + " concluída!\nPróxima onda em " +
                    Mathf.CeilToInt(Mathf.Max(0f, tempoRestante)) + " s", estiloHUD);
                break;

            default:
                GUI.Box(new Rect(esquerda, 12f, largura, 72f),
                    "Onda " + ondaAtual + " / " + ondas +
                    "\nInimigos restantes: " + InimigosRestantes +
                    "  |  Na arena: " + inimigosDaOnda.Count, estiloHUD);
                break;
        }
    }

    private void DesenharPainel(Rect painel, string titulo, string detalhe, bool mostrarReinicio)
    {
        Color corAnterior = GUI.color;
        GUI.color = new Color(0.075f, 0.067f, 0.13f, 0.96f);
        GUI.DrawTexture(painel, Texture2D.whiteTexture);
        GUI.color = corAnterior;

        GUI.Label(new Rect(painel.x + 12f, painel.y + 12f, painel.width - 24f, 36f),
            titulo, estiloTitulo);
        GUI.Label(new Rect(painel.x + 12f, painel.y + 48f, painel.width - 24f, mostrarReinicio ? 32f : 78f),
            detalhe, estiloDetalhe);

        if (mostrarReinicio && !recarregandoCena &&
            GUI.Button(new Rect(painel.center.x - 90f, painel.yMax - 52f, 180f, 36f), "Jogar novamente"))
            RecarregarCena();
    }
}
