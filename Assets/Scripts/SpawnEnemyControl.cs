using UnityEngine;

public class SpawnEnemyControl : MonoBehaviour
{
    [SerializeField] private GameObject enemy;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D colisao)
    {
        if (colisao.CompareTag("Player"))//Verifica se colidiu com player
        {
            Instantiate(enemy);
            Destroy(gameObject);
        }
    }
}
