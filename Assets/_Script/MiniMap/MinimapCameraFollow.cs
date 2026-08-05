using UnityEngine;

public class MinimapCameraFollow : MonoBehaviour
{
    public Transform target;
    public float height = 20f;

    private void LateUpdate()
    {
        if (target != null)
        {
            transform.position = new Vector3(target.position.x, height, target.position.z);
        }
    }
}