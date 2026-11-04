using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

// O runtime existente está em Assembly-CSharp. Resolve por reflexão para
// testar seu comportamento sem mover todos os scripts para um novo asmdef.
public class ComboPlayerTests
{
    private readonly List<GameObject> objetos = new List<GameObject>();
    private Type tipoPlayer;
    private Type tipoAtaque;
    private GameObject prefab;
    private float escalaAnterior;

    [SetUp]
    public void Preparar()
    {
        escalaAnterior = Time.timeScale;
        Time.timeScale = 1f;
        tipoPlayer = Tipo("Player");
        tipoAtaque = Tipo("ataque");
        prefab = NovoObjeto("Hitbox de teste", new Vector3(1000f, 0f, 0f));
        prefab.AddComponent<BoxCollider2D>().isTrigger = true;
        prefab.AddComponent<SpriteRenderer>();
        prefab.AddComponent(tipoAtaque);
    }

    [TearDown]
    public void Limpar()
    {
        foreach (GameObject objeto in objetos)
            if (objeto != null)
                Object.DestroyImmediate(objeto);
        objetos.Clear();
        Time.timeScale = escalaAnterior;
    }

    [Test]
    public void UmComandoTerminaNoPrimeiroGolpe()
    {
        Component jogador = CriarPlayer(Vector3.zero);
        Chamar(jogador, "SolicitarAtaque");
        Assert.That(Propriedade(jogador, "GolpeAtual"), Is.EqualTo(1));
        Avancar(jogador, 0.07f);
        Assert.That(Propriedade(jogador, "FaseAtual").ToString(), Is.EqualTo("Ativo"));
        Avancar(jogador, 0.13f);
        Avancar(jogador, 0.25f);
        Assert.That(Propriedade(jogador, "GolpeAtual"), Is.EqualTo(0));
        Assert.That(Propriedade(jogador, "FaseAtual").ToString(), Is.EqualTo("Livre"));
    }

    [Test]
    public void ComandosAntesDaJanelaNaoEncadeiam()
    {
        Component jogador = CriarPlayer(Vector3.zero);
        Chamar(jogador, "SolicitarAtaque");
        Chamar(jogador, "SolicitarAtaque"); // Preparação.
        Avancar(jogador, 0.07f);
        Chamar(jogador, "SolicitarAtaque"); // Ativo.
        Avancar(jogador, 0.13f);
        Chamar(jogador, "SolicitarAtaque"); // Ainda faltam 0,24 s.
        Avancar(jogador, 0.25f);
        Assert.That(Propriedade(jogador, "GolpeAtual"), Is.EqualTo(0));
    }

    [Test]
    public void SpamNaJanelaReservaSomenteUmProximoGolpe()
    {
        Component jogador = CriarPlayer(Vector3.zero);
        Chamar(jogador, "SolicitarAtaque");
        Avancar(jogador, 0.07f);
        Avancar(jogador, 0.13f);
        Avancar(jogador, 0.05f);
        for (int i = 0; i < 10; i++)
            Chamar(jogador, "SolicitarAtaque");
        Avancar(jogador, 0.20f);
        Assert.That(Propriedade(jogador, "GolpeAtual"), Is.EqualTo(2));
        Avancar(jogador, 0.07f);
        Avancar(jogador, 0.13f);
        Avancar(jogador, 0.25f);
        Assert.That(Propriedade(jogador, "GolpeAtual"), Is.EqualTo(0));
    }

    [Test]
    public void TerceiroGolpeEncerraSequencia()
    {
        Component jogador = CriarPlayer(Vector3.zero);
        Chamar(jogador, "SolicitarAtaque");
        Encadear(jogador);
        Encadear(jogador);
        Assert.That(Propriedade(jogador, "GolpeAtual"), Is.EqualTo(3));
        Avancar(jogador, 0.11f);
        Avancar(jogador, 0.17f);
        Avancar(jogador, 0.21f);
        Chamar(jogador, "SolicitarAtaque"); // Não reserva após o finalizador.
        Avancar(jogador, 0.20f);
        Assert.That(Propriedade(jogador, "GolpeAtual"), Is.EqualTo(0));
        Chamar(jogador, "SolicitarAtaque");
        Assert.That(Propriedade(jogador, "GolpeAtual"), Is.EqualTo(1));
    }

    [Test]
    public void CancelarDesativaHitboxSemAfetarOutroJogador()
    {
        Component primeiro = CriarPlayer(Vector3.zero);
        Component segundo = CriarPlayer(Vector3.right * 10f);
        Chamar(primeiro, "SolicitarAtaque");
        Avancar(primeiro, 0.07f);
        Component hitbox = (Component)Campo(primeiro, "hitboxAtual");
        Chamar(segundo, "SolicitarAtaque");
        Chamar(primeiro, "CancelarCombo");
        Assert.That(hitbox.GetComponent<BoxCollider2D>().enabled, Is.False);
        Assert.That(Propriedade(primeiro, "GolpeAtual"), Is.EqualTo(0));
        Assert.That(Propriedade(segundo, "GolpeAtual"), Is.EqualTo(1));
        Avancar(primeiro, 1f);
        Assert.That(Propriedade(primeiro, "FaseAtual").ToString(), Is.EqualTo("Livre"));
    }

    [Test]
    public void CancelarDescartaComandoReservado()
    {
        Component jogador = CriarPlayer(Vector3.zero);
        Chamar(jogador, "SolicitarAtaque");
        Avancar(jogador, 0.07f);
        Avancar(jogador, 0.13f);
        Avancar(jogador, 0.05f);
        Chamar(jogador, "SolicitarAtaque");
        Assert.That(Campo(jogador, "proximoGolpeReservado"), Is.EqualTo(true));
        Chamar(jogador, "CancelarCombo");
        Assert.That(Campo(jogador, "proximoGolpeReservado"), Is.EqualTo(false));
        Avancar(jogador, 1f);
        Assert.That(Propriedade(jogador, "GolpeAtual"), Is.EqualTo(0));
    }

    [Test]
    public void PausaIgnoraComandosENaoAvancaTemporizador()
    {
        Component jogador = CriarPlayer(Vector3.zero);
        Time.timeScale = 0f;
        Chamar(jogador, "SolicitarAtaque");
        Assert.That(Propriedade(jogador, "GolpeAtual"), Is.EqualTo(0));
        Time.timeScale = 1f;
        Chamar(jogador, "SolicitarAtaque");
        float antes = (float)Campo(jogador, "tempoRestante");
        Time.timeScale = 0f;
        Avancar(jogador, 0f);
        Assert.That(Campo(jogador, "tempoRestante"), Is.EqualTo(antes));
    }

    [Test]
    public void MultiplosCollidersSoRecebemUmAcertoPorGolpe()
    {
        Component jogador = CriarPlayer(Vector3.zero);
        GameObject alvo = NovoObjeto("Enemy de teste", new Vector3(0.8f, 0f, 0f));
        alvo.tag = "Enemy";
        alvo.AddComponent<BoxCollider2D>().size = Vector2.one * 4f;
        Component inimigo = alvo.AddComponent(Tipo("Enemy"));
        Definir(inimigo, "vidaAtual", 10);
        GameObject filho = NovoObjeto("Segundo collider", alvo.transform.position);
        filho.transform.SetParent(alvo.transform);
        filho.AddComponent<BoxCollider2D>();
        Chamar(jogador, "SolicitarAtaque");
        Avancar(jogador, 0.07f);
        Component hitbox = (Component)Campo(jogador, "hitboxAtual");
        Assert.That(Campo(inimigo, "vidaAtual"), Is.EqualTo(9));
        Chamar(hitbox, "VerificarAcertos");
        Chamar(hitbox, "VerificarAcertos");
        Assert.That(Campo(inimigo, "vidaAtual"), Is.EqualTo(9));
        Chamar(jogador, "CancelarCombo");
        Chamar(jogador, "SolicitarAtaque");
        Avancar(jogador, 0.07f);
        Assert.That(Campo(inimigo, "vidaAtual"), Is.EqualTo(8));
    }

    private void Encadear(Component jogador)
    {
        Avancar(jogador, 0.07f);
        Avancar(jogador, 0.13f);
        Avancar(jogador, 0.05f);
        Chamar(jogador, "SolicitarAtaque");
        Avancar(jogador, 0.20f);
    }

    private Component CriarPlayer(Vector3 posicao)
    {
        GameObject objeto = NovoObjeto("Player de teste", posicao);
        objeto.tag = "Player";
        objeto.AddComponent<Rigidbody2D>().gravityScale = 0f;
        objeto.AddComponent<SpriteRenderer>();
        Component jogador = objeto.AddComponent(tipoPlayer);
        Definir(jogador, "attackPointerPrefab", prefab);
        return jogador;
    }

    private GameObject NovoObjeto(string nome, Vector3 posicao)
    {
        GameObject objeto = new GameObject(nome);
        objeto.transform.position = posicao;
        objetos.Add(objeto);
        return objeto;
    }

    private static Type Tipo(string nome)
    {
        Type tipo = AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType(nome)).FirstOrDefault(t => t != null);
        Assert.That(tipo, Is.Not.Null, "Tipo de runtime não encontrado: " + nome);
        return tipo;
    }

    private static void Avancar(Component jogador, float delta)
    {
        Chamar(jogador, "AtualizarCombo", delta);
    }

    private static object Chamar(Component objeto, string metodo, params object[] args)
    {
        return objeto.GetType().GetMethod(metodo,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Invoke(objeto, args);
    }

    private static object Campo(Component objeto, string nome)
    {
        return objeto.GetType().GetField(nome,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .GetValue(objeto);
    }

    private static void Definir(Component objeto, string nome, object valor)
    {
        objeto.GetType().GetField(nome,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .SetValue(objeto, valor);
    }

    private static object Propriedade(Component objeto, string nome)
    {
        return objeto.GetType().GetProperty(nome).GetValue(objeto);
    }
}
