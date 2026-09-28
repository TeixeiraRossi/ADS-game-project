using UnityEngine;
using UnityEngine.UI;

public class BarraVida : MonoBehaviour
{
    [SerializeField] private Slider slider;

    // Atualiza a barra de acordo com a vida atual do Player.
    public void AtualizarVida(int vidaAtual, int vidaMaxima)
    {
        slider.maxValue = vidaMaxima;
        slider.value = vidaAtual;
    }
}
