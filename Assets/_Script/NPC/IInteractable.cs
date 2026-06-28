using UnityEngine;

public interface IInteractable 
{
    string InteractionPrompt { get; } // Câu lệnh hiện lên UI (Ví dụ: "Nhấn E để nói chuyện")
    void Interact(); // Hành động xảy ra khi nhấn nút tương tác
}
