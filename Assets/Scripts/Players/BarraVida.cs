using UnityEngine;
using UnityEngine.UI;

public class BarraVida : MonoBehaviour
{
    [Header("Identificação")]
    public int playerID;

    [Header("UI imagens")]
    public Image fillImageVida; // Referência para a imagem que representa a barra de vida
    public Image fillImageEnergia; // Referência para a imagem que representa a barra de energia

    [Header("Vida valores")]
    public float vidaMaxima;
    private float vidaAtual;

    [Header("Energia valores")]
    public float energiaMaxima;
    private float energiaAtual;

    private void Start()
    {
        if (fillImageVida == null)
        {
            fillImageVida = GameObject.Find("FillVida_P" + playerID).GetComponent<Image>();
        }

        if (fillImageEnergia == null)
        {
            fillImageEnergia = GameObject.Find("FillEnergia_P" + playerID).GetComponent<Image>();
        }

        vidaAtual = vidaMaxima; // Inicializa a vida atual com a vida máxima
        vidaAtual = 5f;
        energiaAtual = energiaMaxima; // Inicializa a energia atual com a energia máxima
        energiaAtual = 1f;
        AtualizarVida();
        AtualizarEnergia();
    }

    private void FixedUpdate()
    {
        if (energiaAtual < energiaMaxima)
        {
            energiaAtual += Time.fixedDeltaTime;

            // Garante que não passe do limite
            if (energiaAtual > energiaMaxima)
                energiaAtual = energiaMaxima;

            AtualizarEnergia();
        }
    }

    public void ReceberDano(float dano)
    {
        vidaAtual -= dano; // Reduz a vida atual pelo valor do dano
        vidaAtual = Mathf.Clamp(vidaAtual, 0, vidaMaxima);// Garante que a vida não fique abaixo de 0 ou acima da vida máxima

        if (vidaAtual <= 0)
        {
            Morrer(); // Chama o método de morte se a vida chegar a 0
        }

        AtualizarVida(); // Atualiza a barra de vida
    }

    private void Morrer()
    {
        Debug.Log("Player morreu!");

        // Por enquanto apenas desativa o Player.
        // Depois podemos substituir por animação, respawn ou Game Over.
        gameObject.SetActive(false);
    }

    public void ReceberBuff(ItemBuff.TipoBuff tipo, float quantidade)
    {
        switch (tipo)
        {
            case ItemBuff.TipoBuff.vida:
                vidaAtual += quantidade;
                vidaAtual = Mathf.Clamp(vidaAtual, 0, vidaMaxima);
                AtualizarVida();
                Debug.Log("Player recebeu buff de vida! Vida: " + vidaAtual);
                break;
            case ItemBuff.TipoBuff.energia:
                energiaAtual += quantidade;
                energiaAtual = Mathf.Clamp(energiaAtual, 0, energiaMaxima);
                AtualizarEnergia();
                Debug.Log("Player recebeu buff de energia! Energia: " + energiaAtual);
                break;
            case ItemBuff.TipoBuff.xp:
                // Aqui você pode implementar a lógica para XP, se houver.
                Debug.Log("Player recebeu buff de XP!");
                break;
        }
    }

    // Atualiza a barra de acordo com a vida atual do Player.
    public void AtualizarVida()
    {
        fillImageVida.fillAmount = vidaAtual / vidaMaxima;
    }

    public void AtualizarEnergia()
    {
        fillImageEnergia.fillAmount = energiaAtual / energiaMaxima;
    }
}   