using UnityEngine;

public class Entity : MonoBehaviour
{
    public int        Id;
    public EntityType Type;
    
    public void Destroy()
    {
        Destroy(gameObject);
    }
}
