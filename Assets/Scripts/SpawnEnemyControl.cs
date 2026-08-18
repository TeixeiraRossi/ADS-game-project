using UnityEngine;

public class SpawnEnemyControl : MonoBehaviour
{
    [SerializeField] private GameObject enemy;
    private void OnTriggerEnter2D(Collider2D colisao)
    {
        if (colisao.CompareTag("Player"))//Verifica se colidiu com player
        {
            Instantiate(enemy, transform.position + Vector3.right * 6f, transform.rotation);
        }
    }
}
