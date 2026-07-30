using UnityEngine;

public class BoatPatrol : MonoBehaviour
{
    public enum MovementType 
    { 
        DiThang, 
        DiChuU, 
        DiCheo 
    }

    [Header("Cài đặt di chuyển")]
    [Tooltip("Chọn kiểu di chuyển cho vật thể này")]
    public MovementType moveType;
    
    [Tooltip("Tốc độ di chuyển")]
    public float speed = 5f;
    
    [Tooltip("Thời gian di chuyển trước khi quay đầu (giây)")]
    public float moveDuration = 4f;

    [Tooltip("Tốc độ bẻ lái cho đường chữ U")]
    public float turnSpeed = 45f; 
    private float timer = 0f;
    private int uTurnState = 0; 
    private float currentTurnAngle = 0f;

    void Update()
    {
        switch (moveType)
        {
            case MovementType.DiThang:
                MoveStraight();
                break;
            case MovementType.DiChuU:
                MoveUShape();
                break;
            case MovementType.DiCheo:
                MoveDiagonal();
                break;
        }
    }

    private void MoveStraight()
    {
        transform.Translate(Vector3.left * speed * Time.deltaTime);
        
        timer += Time.deltaTime;
        if (timer >= moveDuration)
        {
            // Hết thời gian thì quay đầu 180 độ
            transform.Rotate(0, 180, 0);
            timer = 0; // Reset đồng hồ
        }
    }

    private void MoveDiagonal()
    {
        // Đi chéo (kết hợp trục Z hướng tới và trục X sang phải)
        Vector3 diagonalDir = (Vector3.left + Vector3.right).normalized;
        transform.Translate(diagonalDir * speed * Time.deltaTime);

        timer += Time.deltaTime;
        if (timer >= moveDuration)
        {
            // Quay đầu 180 độ
            transform.Rotate(0, 180, 0);
            timer = 0;
        }
    }

    private void MoveUShape()
    {
        if (uTurnState == 0) // Giai đoạn 1: Đi thẳng một đoạn
        {
            transform.Translate(Vector3.left * speed * Time.deltaTime);
            timer += Time.deltaTime;
            
            if (timer >= moveDuration)
            {
                uTurnState = 1; // Chuyển sang giai đoạn bẻ lái
                timer = 0;
                currentTurnAngle = 0f;
            }
        }
        else if (uTurnState == 1) // Giai đoạn 2: Vừa đi vừa cua tạo hình đáy chữ U
        {
            transform.Translate(Vector3.left * speed * Time.deltaTime);

            float turnStep = turnSpeed * Time.deltaTime;
            transform.Rotate(0, turnStep, 0);
            currentTurnAngle += turnStep;
            
            if (currentTurnAngle >= 180f)
            {
                uTurnState = 0;
            }
        }
    }
}