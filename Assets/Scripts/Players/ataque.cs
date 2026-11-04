using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class ataque : MonoBehaviour
{
    private BoxCollider2D hitbox;
    private SpriteRenderer visual;
    private Player dono;
    private int dano;
    private float multiplicadorKnockback;
    private Vector3 deslocamento;
    private bool ativo;
    private readonly HashSet<Enemy> atingidos = new HashSet<Enemy>();
    private readonly List<Collider2D> sobreposicoes = new List<Collider2D>();

    private void Awake()
    {
        PrepararComponentes();
        Encerrar();
    }

    private void PrepararComponentes()
    {
        if (hitbox == null)
            hitbox = GetComponent<BoxCollider2D>();
        if (visual == null)
            visual = GetComponentInChildren<SpriteRenderer>();
    }

    public void Inicializar(Player jogador, int danoGolpe, float knockback,
        bool esquerda, Vector3 offset)
    {
        PrepararComponentes();
        dono = jogador;
        dano = Mathf.Max(1, danoGolpe);
        multiplicadorKnockback = Mathf.Max(0f, knockback);
        deslocamento = offset;
        atingidos.Clear();
        ativo = true;
        hitbox.enabled = true;
        if (visual != null)
        {
            visual.flipY = esquerda;
            visual.enabled = true;
        }

        AtualizarPosicao();
        // Inclui alvos que já estão na área ao abrir a fase ativa.
        Physics2D.SyncTransforms();
        VerificarAcertos();
    }

    private void FixedUpdate()
    {
        if (!ValidarDono())
            return;

        AtualizarPosicao();
        Physics2D.SyncTransforms();
        VerificarAcertos();
    }

    private void LateUpdate()
    {
        if (ValidarDono())
            AtualizarPosicao();
    }

    private bool ValidarDono()
    {
        if (!ativo)
            return false;

        if (dono != null && dono.isActiveAndEnabled &&
            dono.FaseAtual == Player.FaseCombo.Ativo)
            return true;

        Encerrar();
        Destroy(gameObject);
        return false;
    }

    private void AtualizarPosicao()
    {
        transform.position = dono.transform.position + deslocamento;
    }

    private void VerificarAcertos()
    {
        ContactFilter2D filtro = new ContactFilter2D();
        filtro.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        filtro.useTriggers = true;
        hitbox.Overlap(filtro, sobreposicoes);
        foreach (Collider2D colisao in sobreposicoes)
        {
            if (!ativo)
                break;

            Enemy inimigo = colisao.GetComponentInParent<Enemy>();
            if (inimigo == null || !inimigo.gameObject.activeInHierarchy ||
                !inimigo.CompareTag("Enemy"))
                continue;

            // Deduplica por Enemy, mesmo com colliders filhos ou reentrada.
            if (atingidos.Add(inimigo))
                inimigo.ReceberDano(dano, multiplicadorKnockback, dono.transform.position);
        }
    }

    public void Encerrar()
    {
        PrepararComponentes();
        ativo = false;
        hitbox.enabled = false;
        if (visual != null)
            visual.enabled = false;
        atingidos.Clear();
    }

    private void OnDisable()
    {
        Encerrar();
    }
}
