using UnityEngine;

public class TargetScript : MonoBehaviour
{
    public int diemThuong = 10;
    private GameManagerMiniGame gameManager;

    void Start()
    {
        gameManager = Object.FindAnyObjectByType<GameManagerMiniGame>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (collision.CompareTag("Stone"))
        {
            if (gameManager != null)
            {
                gameManager.CongDiemNemDa(diemThuong);
            }

           
            Destroy(collision.gameObject);

            
            gameObject.SetActive(false);
        }
    }
}