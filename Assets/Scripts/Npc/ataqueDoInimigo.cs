using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class ataqueDoInimigo : MonoBehaviour
{
    [SerializeField] private int dano = 1;

    private BoxCollider2D hitbox;
    private bool golpeAtivo;
    private readonly HashSet<BarraVida> jogadoresAtingidos = new HashSet<BarraVida>();
    private readonly List<Collider2D> sobreposicoes = new List<Collider2D>();

    private void Awake()
    {
        EncerrarGolpe();
    }

    private void OnDisable()
    {
        EncerrarGolpe();
    }

    public void Orientar(bool esquerda)
    {
        Vector3 posicao = transform.localPosition;
        posicao.x = Mathf.Abs(posicao.x) * (esquerda ? -1f : 1f);
        transform.localPosition = posicao;
    }

    public void IniciarGolpe()
    {
        if (!isActiveAndEnabled)
            return;

        if (hitbox == null)
            hitbox = GetComponent<BoxCollider2D>();

        jogadoresAtingidos.Clear();
        golpeAtivo = true;
        hitbox.enabled = true;

        // Inclui jogadores que já estavam dentro da área ao abrir o golpe.
        Physics2D.SyncTransforms();
        VerificarAcertos();
    }

    public void EncerrarGolpe()
    {
        golpeAtivo = false;
        if (hitbox == null)
            hitbox = GetComponent<BoxCollider2D>();

        hitbox.enabled = false;
        jogadoresAtingidos.Clear();
    }

    private void FixedUpdate()
    {
        if (golpeAtivo)
            VerificarAcertos();
    }

    private void VerificarAcertos()
    {
        // Reutiliza a lista e respeita as camadas de colisão do projeto.
        ContactFilter2D filtro = new ContactFilter2D();
        filtro.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        filtro.useTriggers = true;
        hitbox.Overlap(filtro, sobreposicoes);

        foreach (Collider2D colisao in sobreposicoes)
        {
            if (!golpeAtivo)
                break;

            BarraVida vida = colisao.GetComponentInParent<BarraVida>();
            if (vida == null || !vida.gameObject.activeInHierarchy ||
                !vida.CompareTag("Player"))
                continue;

            // Múltiplos colliders e reentradas não duplicam o dano.
            if (jogadoresAtingidos.Add(vida))
                vida.ReceberDano(dano);
        }
    }
}
