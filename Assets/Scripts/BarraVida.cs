using UnityEngine;
using UnityEngine.UI;

public class BarraVida : MonoBehaviour
{
    [Header("UI imagens")]
    public Image fillImage; // Referência para a imagem que representa a barra de vida

    [Header("Vida valores")]
    public float vidaMaxima;
    private float vidaAtual;

    private void Start()
    {
        if (fillImage == null)
        {
            fillImage = GameObject.Find("FillVida").GetComponent<Image>();
        }

        vidaAtual = vidaMaxima; // Inicializa a vida atual com a vida máxima
        vidaAtual = 5f;
        AtualizarVida();
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
                // Aqui você pode implementar a lógica para energia, se houver.
                Debug.Log("Player recebeu buff de energia!");
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
        fillImage.fillAmount = vidaAtual / vidaMaxima;
    }
}