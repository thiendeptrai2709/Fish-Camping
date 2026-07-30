using UnityEngine;

public class RealisticBoatPatrol : MonoBehaviour
{
    public enum MovementType { DiThang, DiChuU, DiZigZag }
    public enum BoatSize { TauBe, TauLon }

    [Header("--- THIẾT LẬP CƠ BẢN ---")]
    [Tooltip("Chọn kiểu di chuyển")]
    public MovementType moveType;
    [Tooltip("Phân loại để tự động tính toán quán tính và độ lướt")]
    public BoatSize boatSize;

    [Header("--- THỜI GIAN TUẦN TRA ---")]
    [Tooltip("Đi bao lâu thì bắt đầu bẻ lái quay đầu (giây)?")]
    public float patrolDuration = 8f;

    [Header("--- HIỆU ỨNG (VFX & MODEL) ---")]
    [Tooltip("Kéo ĐÚNG CÁI MODEL 3D CỦA TÀU vào đây để làm hiệu ứng bập bềnh")]
    public Transform boatModel; 
    [Tooltip("Kéo Particle System (bọt nước đuôi tàu) vào đây")]
    public ParticleSystem waterWakeVFX; 

    // Các thông số được tự động tính toán dựa trên Size
    private float speed;
    private float turnSpeed;
    private float bobSpeed;
    private float bobHeight;
    private float tiltAngle;

    // Quản lý trạng thái
    private float timer = 0f;
    private bool isTurning = false;
    private Quaternion targetRotation;
    private float startY; // Tọa độ Y ban đầu của model

    void Start()
    {
        // 1. TỰ ĐỘNG PHÂN BỔ THÔNG SỐ THEO KÍCH THƯỚC TÀU
        if (boatSize == BoatSize.TauBe)
        {
            speed = 8f;
            turnSpeed = 40f;    // Cua gắt, vòng cua hẹp
            bobSpeed = 2.5f;    // Nhấp nhô nhanh
            bobHeight = 0.15f;  // Biên độ nhỏ
            tiltAngle = 4f;     // Chòng chành nhiều
        }
        else // Tàu Lớn
        {
            speed = 3.5f;
            turnSpeed = 12f;    // Khối lượng lớn -> Cua chậm, vòng cua cực rộng
            bobSpeed = 1.2f;    // Lừ đừ
            bobHeight = 0.35f;  // Lún sâu xuống nước
            tiltAngle = 1.5f;   // Ít bị nghiêng
        }

        // Lưu vị trí ban đầu để làm hiệu ứng nhấp nhô
        if (boatModel != null)
        {
            startY = boatModel.localPosition.y;
        }

        // Bật bọt nước nếu có
        if (waterWakeVFX != null) waterWakeVFX.Play();
    }

    void Update()
    {
        HandleRealisticMovement();
        ApplyWaterEffects();
    }

    private void HandleRealisticMovement()
    {
        // Dù đang đi thẳng hay đang cua, động cơ luôn đẩy tàu tới trước.
        transform.Translate(Vector3.left * speed * Time.deltaTime);

        if (isTurning)
        {
            // Cua mượt mà tạo vòng cung, không quay ngoắt 180 độ
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

            // Kiểm tra xem đã quay xong chưa (sai số 1 độ)
            if (Quaternion.Angle(transform.rotation, targetRotation) < 1f)
            {
                isTurning = false;
                timer = 0f; // Reset thời gian đi thẳng
            }
        }
        else
        {
            // Đang đi thẳng
            timer += Time.deltaTime;
            if (timer >= patrolDuration)
            {
                StartTurning();
            }
        }
    }

    private void StartTurning()
    {
        isTurning = true;

        if (moveType == MovementType.DiThang || moveType == MovementType.DiChuU)
        {
            // Đi thẳng / Chữ U: Quay đầu 180 độ. Vì có Transform.Translate ở Update, 
            // việc quay từ từ sẽ tự động tạo ra một đường vòng cung chữ U cực kỳ thực tế.
            targetRotation = transform.rotation * Quaternion.Euler(0, 180, 0);
        }
        else if (moveType == MovementType.DiZigZag) // Thay thế đi chéo bằng ZigZag cho chuẩn tàu thủy
        {
            // Đang đi thì bẻ lái 90 độ sang một bên để rẽ hướng
            float randomTurn = Random.Range(0, 2) == 0 ? 90f : -90f;
            targetRotation = transform.rotation * Quaternion.Euler(0, randomTurn, 0);
        }
    }

    private void ApplyWaterEffects()
    {
        if (boatModel == null) return;

        // Dùng hàm Sine (sóng âm) để tạo hiệu ứng bập bềnh vật lý lên xuống
        float newY = startY + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        
        // Tạo hiệu ứng nghiêng thân tàu (chòng chành)
        float currentTilt = Mathf.Sin(Time.time * bobSpeed * 0.8f) * tiltAngle;

        // Nếu đang cua, tàu sẽ nghiêng thêm sang một bên do lực ly tâm
        if (isTurning) 
        {
            currentTilt += (boatSize == BoatSize.TauBe ? 5f : 2f); 
        }

        // Áp dụng vị trí và góc nghiêng mới cho riêng cái Model
        boatModel.localPosition = new Vector3(boatModel.localPosition.x, newY, boatModel.localPosition.z);
        boatModel.localRotation = Quaternion.Euler(0, 0, currentTilt);
    }
}