using System.Collections;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{   
    [Header("Movimento")]
    private Rigidbody2D rb; //RB do Objeto
    private Vector2 direcao; //Direção do movimento
    public float velocidade = 5f; //Controle de velocidade
    public SpriteRenderer sprite; //Sprite do boneco

    [Header("Ataque")]
    public BoxCollider2D attackHitbox;
    public GameObject attackPointerPrefab;
    public float attackDuration = 0.2f;
    private bool atacando;
    private bool olhandoEsquerda;
    public Vector2 attackOffset = new Vector2(0.8f, -0.2f);//Ajuste da posição do ataque

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>(); //Guarda RB do objeto a que esta anexado

        /*attackHitbox.enabled = false;*/ //tava bugando a hitbox do player 2
    }

    public void Mover(InputAction.CallbackContext context) //Função chamada pelo Input System, recebe o contexto da ação
    {
        direcao = context.ReadValue<Vector2>(); //Guarda direção recebida do Input System
    }

    public void Atacar(InputAction.CallbackContext context)
    {
        if (context.started && !atacando)
        {
            if(attackPointerPrefab != null)
            {   
                float posX = olhandoEsquerda ? -attackOffset.x : attackOffset.x;
                Vector3 spawnPos = transform.position + new Vector3(posX, attackOffset.y, 0f);
                Quaternion spawnRot = Quaternion.Euler(0, 0, 90);
                GameObject ptr = Instantiate(attackPointerPrefab, spawnPos, spawnRot);
                var sr = ptr.GetComponentInChildren<SpriteRenderer>();
                if (sr) sr.flipY = olhandoEsquerda;
                Destroy(ptr, attackDuration);
            }
            StartCoroutine(AttackCoroutine());
        }
    }

    private IEnumerator AttackCoroutine()
    {
        atacando = true;
        /*attackHitbox.enabled = true;*/
        yield return new WaitForSeconds(attackDuration);
        /*attackHitbox.enabled = false;*/
        atacando = false;
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(
            direcao.x * velocidade,
            direcao.y * velocidade
            );
    }

    private void Update()
    {
        Flip();
    }

    private void Flip()
    {
        if (direcao.x > 0)
        {
            olhandoEsquerda = false;
            sprite.flipX = false;

        }
        else if (direcao.x < 0)
        {
            olhandoEsquerda = true;
            sprite.flipX = true;
        }
    }

}
