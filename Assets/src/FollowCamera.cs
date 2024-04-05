using UnityEngine;
using static World;
using static Vars;

public class FollowCamera : MonoBehaviour
{
    public Camera Camera;
    
    private static Camera _camera;
    
    private void Awake()
    {
        _camera = Camera;
    }
    
    public static void FollowTarget(Vector3 target)
    {
        var position = _camera.transform.position;
        var nextPos  = Vector3.MoveTowards(position, target, CameraSpeed * Time.deltaTime);
        nextPos.z    = -10;
        
        var verticalSize   = _camera.orthographicSize;
        var horizontalSize = verticalSize * _camera.aspect;
        var totalSize      = new Vector3(horizontalSize, verticalSize);
        var max            = nextPos + totalSize;
        var min            = nextPos - totalSize;
        var worldHalfSize  = Size * 0.5f;
        var worldMax       = Center + worldHalfSize;
        var worldMin       = Center - worldHalfSize;
        
        if(min.x < worldMin.x)
            nextPos.x = worldMin.x + horizontalSize;
            
        if(max.x > worldMax.x)
            nextPos.x = worldMax.x - horizontalSize;
        
        if(min.y < worldMin.y)
            nextPos.y = worldMin.y + verticalSize;
            
        if(max.y > worldMax.y)
            nextPos.y = worldMax.y - verticalSize;
        
        
        _camera.transform.position = nextPos;
    }
}
