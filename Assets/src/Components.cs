using UnityEngine;

public struct Transform
{
    public float      orientation;
    public Vector3    position;
    public Vector3    scale;
}

public struct GameObjectReference
{
    public Entity                 go;
    public UnityEngine.Transform  transform;
}

public struct Movable
{
    public Vector3 direction;
    public Vector3 velocity;
}

public struct Ship
{
    public Vector3 lookDirection;
    public Vector2 size;
    public float   rotationSpeed;
    public float   maxSpeed;
    public float   acceleration;
    public float   reloadTime;
    public float   reloadProgress;
    public bool    shooting;
    public int     projectileId; // projectileId in projectiles table    
}

public struct Player
{
    
}

public struct Destroy
{
    
}

public struct Bullet
{
    public float   speed;
    public float   radius;
    public int     damage;
    public int     sender;
    public bool    canDamageSender;
}

public struct Temporary
{
    public float timeToLive;
    public float timePassed;
}

public struct Health
{
    public int current;
    public int max;
}

public struct Damage
{
    public int amount;
}


//AI

public struct FollowTarget
{
    public int   target;
    public float distance;
}


// line to fix code editor bug :)