using UnityEngine;

public class MapButtonVisibility : MonoBehaviour
{
    public bool searchByTag = true;
    public string targetID;

    public void CheckVisibility()
    {
        gameObject.SetActive(true);
        GameObject target = searchByTag ? GameObject.FindGameObjectWithTag(targetID) : GameObject.Find(targetID);
        gameObject.SetActive(target != null);
    }
}