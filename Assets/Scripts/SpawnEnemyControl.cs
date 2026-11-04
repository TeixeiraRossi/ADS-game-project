using System.Collections.Generic;
using UnityEngine;

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

    private void OnGUI()
    {
        if (!mostrarHUD)
            return;

        if (estiloHUD == null)
        {
            estiloHUD = new GUIStyle(GUI.skin.box);
            estiloHUD.fontSize = 18;
            estiloHUD.alignment = TextAnchor.MiddleCenter;
            estiloHUD.wordWrap = true;
        }

        string texto;
        switch (estadoAtual)
        {
            case EstadoPartida.Aguardando:
                texto = "Entre na arena para iniciar";
                break;
            case EstadoPartida.Intervalo:
                texto = "Onda " + ondaAtual + " concluída!\nPróxima onda em " +
                    Mathf.CeilToInt(Mathf.Max(0f, tempoRestante)) + " s";
                break;
            case EstadoPartida.Vitoria:
                texto = "Vitória!\nTodas as ondas foram concluídas";
                break;
            case EstadoPartida.Derrota:
                texto = "Derrota!\nReinicie a cena para tentar novamente";
                break;
            default:
                texto = "Onda " + ondaAtual + " / " + Mathf.Max(1, totalOndas) +
                    "\nInimigos restantes: " + InimigosRestantes +
                    "  |  Na arena: " + inimigosDaOnda.Count;
                break;
        }

        GUI.Box(new Rect((Screen.width - 360f) / 2f, 12f, 360f, 72f), texto, estiloHUD);
    }
}
