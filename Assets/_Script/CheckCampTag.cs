using UnityEngine;

public class CheckCampTag : MonoBehaviour
{
    [ContextMenu("QUÉT TẤT CẢ TAG CAMP")]
    public void ScanTags()
    {
        GameObject[] camps = GameObject.FindGameObjectsWithTag("Camp");
        Debug.Log($"<color=yellow>=== TÌM THẤY {camps.Length} GAMEOBJECT CÓ TAG Camp ===</color>");

        for (int i = 0; i < camps.Length; i++)
        {
            Debug.Log($"[{i + 1}] Tên: <b>{camps[i].name}</b> | Vị trí (Position): {camps[i].transform.position}", camps[i]);
        }
    }
}