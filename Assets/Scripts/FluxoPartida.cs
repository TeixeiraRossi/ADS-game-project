using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FluxoPartida : MonoBehaviour
{
    public enum EstadoFluxo { Menu, Jogando, Pausado, Vitoria, Derrota, Carregando }

    [SerializeField] private SpawnEnemyControl ondas;
    [SerializeField] private PlayerInputManeger entrada;
    private EstadoFluxo estado;
    private GameObject interfaceCriada;
    private GameObject painel;
    private Text titulo;
    private Text mensagem;
    private Text hud;
    private Button jogar;
    private Button continuar;
    private Button repetir;
    private Button menu;
    private Button sair;
    private Font fonte;
    private static bool jogarAoCarregar;

    public EstadoFluxo EstadoAtual => estado;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void LimparEstadoGlobal()
    {
        jogarAoCarregar = false;
    }

    private void Awake()
    {
        if (ondas == null || entrada == null)
        {
            Debug.LogError("Configure ondas e entrada no FluxoPartida.", this);
            enabled = false;
            return;
        }

        entrada.EntradaLiberada = false;
    }

    private void Start()
    {
        // O fluxo existente cria os personagens explicitamente. Impede
        // uma segunda entrada automática pelo PlayerInputManager da cena.
        if (PlayerInputManager.instance != null)
            PlayerInputManager.instance.DisableJoining();

        CriarInterface();
        bool iniciar = jogarAoCarregar;
        jogarAoCarregar = false;
        if (iniciar)
            IniciarJogo();
        else
            MostrarMenu();
    }

    private void Update()
    {
        if (estado == EstadoFluxo.Jogando)
        {
            if (ondas.EstadoAtual == SpawnEnemyControl.EstadoPartida.Vitoria)
            {
                MostrarResultado(true);
                return;
            }
            if (ondas.EstadoAtual == SpawnEnemyControl.EstadoPartida.Derrota)
            {
                MostrarResultado(false);
                return;
            }
            AtualizarHUD();
        }

        if ((estado == EstadoFluxo.Jogando || estado == EstadoFluxo.Pausado) &&
            ApertouPausa())
        {
            if (estado == EstadoFluxo.Jogando)
                Pausar();
            else
                Continuar();
        }
    }

    private bool ApertouPausa()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            return true;
        foreach (Gamepad controle in Gamepad.all)
            if (controle.startButton.wasPressedThisFrame)
                return true;
        return false;
    }

    public void IniciarJogo()
    {
        if (estado != EstadoFluxo.Menu)
            return;
        estado = EstadoFluxo.Jogando;
        entrada.EntradaLiberada = true;
        DefinirPausa(false);
        painel.SetActive(false);
        hud.gameObject.SetActive(true);
        Selecionar(null);
        AtualizarHUD();
    }

    public void Pausar()
    {
        if (estado != EstadoFluxo.Jogando)
            return;
        estado = EstadoFluxo.Pausado;
        DefinirPausa(true);
        ExibirPainel("Pausa", "A partida está pausada.\nEsc / Start para continuar.");
        continuar.gameObject.SetActive(true);
        menu.gameObject.SetActive(true);
        Selecionar(continuar);
    }

    public void Continuar()
    {
        if (estado != EstadoFluxo.Pausado)
            return;
        estado = EstadoFluxo.Jogando;
        DefinirPausa(false);
        painel.SetActive(false);
        Selecionar(null);
    }

    private void MostrarMenu()
    {
        estado = EstadoFluxo.Menu;
        DefinirPausa(true);
        hud.gameObject.SetActive(false);
        ExibirPainel("Arena",
            "Sobreviva às ondas, sozinho ou em dupla.\n\n" +
            "Entrar: W (teclado) ou botão inferior (controle).\n" +
            "Mover: WASD / analógico esquerdo.\n" +
            "Atacar: clique esquerdo / X ou quadrado.\n" +
            "Pausa: Esc / Start.");
        jogar.gameObject.SetActive(true);
        sair.gameObject.SetActive(true);
        Selecionar(jogar);
    }

    private void MostrarResultado(bool venceu)
    {
        estado = venceu ? EstadoFluxo.Vitoria : EstadoFluxo.Derrota;
        DefinirPausa(true);
        hud.gameObject.SetActive(false);
        ExibirPainel(venceu ? "Vitória!" : "Fim de jogo",
            venceu ? "Todas as ondas foram concluídas!"
                : "Nenhum jogador sobreviveu.\nTente novamente!");
        repetir.gameObject.SetActive(true);
        menu.gameObject.SetActive(true);
        sair.gameObject.SetActive(true);
        Selecionar(repetir);
    }

    private void DefinirPausa(bool pausado)
    {
        entrada.EntradaLiberada = !pausado;
        // Suspende apenas as ações dos personagens; as ações de UI
        // continuam habilitadas para navegar com teclado e gamepad.
        foreach (PlayerInput jogador in PlayerInput.all)
        {
            if (pausado)
                jogador.DeactivateInput();
            else
                jogador.ActivateInput();
        }
        Time.timeScale = pausado ? 0f : 1f;
        AudioListener.pause = pausado;
    }

    public void JogarNovamente()
    {
        Recarregar(true);
    }

    public void VoltarAoMenu()
    {
        Recarregar(false);
    }

    private void Recarregar(bool iniciar)
    {
        if (estado == EstadoFluxo.Carregando)
            return;
        estado = EstadoFluxo.Carregando;
        entrada.EntradaLiberada = false;
        jogarAoCarregar = iniciar;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        // Recria jogadores, HUD, ondas e flags de entrada numa nova cena.
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
    }

    public void Sair()
    {
        Application.Quit();
    }

    private void ExibirPainel(string cabecalho, string descricao)
    {
        painel.SetActive(true);
        titulo.text = cabecalho;
        mensagem.text = descricao;
        foreach (Button botao in new[] { jogar, continuar, repetir, menu, sair })
            botao.gameObject.SetActive(false);
    }

    private void AtualizarHUD()
    {
        if (ondas.EstadoAtual == SpawnEnemyControl.EstadoPartida.Aguardando)
        {
            hud.text = "Entre: W / botão A do controle   |   Pausa: Esc / Start\n" +
                "Entre na arena para começar as ondas.";
            return;
        }
        if (ondas.EstadoAtual == SpawnEnemyControl.EstadoPartida.Intervalo)
        {
            hud.text = "Onda " + ondas.OndaAtual + " concluída!\nPróxima onda em " +
                Mathf.CeilToInt(ondas.TempoIntervalo) + " s";
            return;
        }
        hud.text = "Onda " + ondas.OndaAtual + " / " + ondas.TotalOndas +
            "\nInimigos restantes: " + ondas.InimigosRestantes;
    }

    private void CriarInterface()
    {
        fonte = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        interfaceCriada = new GameObject("InterfacePartida", typeof(RectTransform),
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = interfaceCriada.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = interfaceCriada.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject hudObjeto = NovoElemento("Ondas", interfaceCriada.transform);
        hud = ConfigurarTexto(hudObjeto, "", 28);
        RectTransform hudRect = hudObjeto.GetComponent<RectTransform>();
        hudRect.anchorMin = new Vector2(0.5f, 1f);
        hudRect.anchorMax = new Vector2(0.5f, 1f);
        hudRect.pivot = new Vector2(0.5f, 1f);
        hudRect.anchoredPosition = new Vector2(0f, -20f);
        hudRect.sizeDelta = new Vector2(950f, 100f);

        painel = NovoElemento("MenuPartida", interfaceCriada.transform);
        Esticar(painel.GetComponent<RectTransform>());
        Image fundo = painel.AddComponent<Image>();
        fundo.color = new Color(0.025f, 0.045f, 0.08f, 0.96f);

        GameObject conteudo = NovoElemento("Conteudo", painel.transform);
        conteudo.GetComponent<RectTransform>().sizeDelta = new Vector2(1000f, 800f);
        VerticalLayoutGroup layout = conteudo.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 18f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        titulo = TextoLayout("Titulo", conteudo.transform, "", 58, 90f);
        mensagem = TextoLayout("Mensagem", conteudo.transform, "", 26, 200f);
        jogar = CriarBotao(conteudo.transform, "Jogar", IniciarJogo);
        continuar = CriarBotao(conteudo.transform, "Continuar", Continuar);
        repetir = CriarBotao(conteudo.transform, "Jogar novamente", JogarNovamente);
        menu = CriarBotao(conteudo.transform, "Voltar ao menu", VoltarAoMenu);
        sair = CriarBotao(conteudo.transform, "Sair", Sair);

        if (EventSystem.current == null)
        {
            GameObject eventos = new GameObject("EventosPartida",
                typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventos.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }
    }

    private GameObject NovoElemento(string nome, Transform pai)
    {
        GameObject objeto = new GameObject(nome, typeof(RectTransform));
        objeto.transform.SetParent(pai, false);
        return objeto;
    }

    private Text ConfigurarTexto(GameObject objeto, string texto, int tamanho)
    {
        Text componente = objeto.AddComponent<Text>();
        componente.font = fonte;
        componente.fontSize = tamanho;
        componente.text = texto;
        componente.alignment = TextAnchor.MiddleCenter;
        componente.color = Color.white;
        componente.raycastTarget = false;
        return componente;
    }

    private Text TextoLayout(string nome, Transform pai, string texto,
        int tamanho, float altura)
    {
        GameObject objeto = NovoElemento(nome, pai);
        LayoutElement elemento = objeto.AddComponent<LayoutElement>();
        elemento.preferredWidth = 960f;
        elemento.preferredHeight = altura;
        return ConfigurarTexto(objeto, texto, tamanho);
    }

    private Button CriarBotao(Transform pai, string texto, UnityAction acao)
    {
        GameObject objeto = NovoElemento(texto, pai);
        Image imagem = objeto.AddComponent<Image>();
        imagem.color = Color.white;
        LayoutElement tamanho = objeto.AddComponent<LayoutElement>();
        tamanho.preferredWidth = 400f;
        tamanho.preferredHeight = 64f;
        Button botao = objeto.AddComponent<Button>();
        botao.targetGraphic = imagem;
        ColorBlock cores = botao.colors;
        cores.normalColor = new Color(0.12f, 0.32f, 0.57f, 1f);
        cores.highlightedColor = new Color(0.18f, 0.45f, 0.70f, 1f);
        cores.selectedColor = new Color(0.22f, 0.55f, 0.80f, 1f);
        cores.pressedColor = new Color(0.08f, 0.22f, 0.40f, 1f);
        botao.colors = cores;
        botao.onClick.AddListener(acao);
        GameObject rotulo = NovoElemento("Texto", objeto.transform);
        Esticar(rotulo.GetComponent<RectTransform>());
        ConfigurarTexto(rotulo, texto, 28);
        return botao;
    }

    private void Esticar(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void Selecionar(Button botao)
    {
        if (EventSystem.current != null)
        {
            Canvas.ForceUpdateCanvases();
            EventSystem.current.SetSelectedGameObject(
                botao != null ? botao.gameObject : null);
        }
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        if (interfaceCriada != null)
            Destroy(interfaceCriada);
    }
}
