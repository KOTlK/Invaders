using Leopotam.EcsLite;
using static Globals;

public static class Queries
{
    public static EcsFilter TransformReferencesQuery;
    public static EcsFilter ShipsQuery;
    public static EcsFilter PlayerShipQuery;
    public static EcsFilter DestroyRefQuery;
    public static EcsFilter BulletsQuery;
    public static EcsFilter DamageQuery;
    public static EcsFilter TempQuery;
    public static EcsFilter FollowQuery;
    public static EcsFilter MoveQuery;
    public static EcsFilter AiAgentsQuery;
    
    public static void InitQueries()
    {
        TransformReferencesQuery = MainWorld.Filter<Transform>().Inc<GameObjectReference>().End();
        ShipsQuery               = MainWorld.Filter<Ship>().Inc<Transform>().Inc<Movable>().Exc<Destroy>().End();
        PlayerShipQuery          = MainWorld.Filter<Ship>().Inc<Player>().Inc<Movable>().Exc<Destroy>().End();
        DestroyRefQuery          = MainWorld.Filter<Destroy>().Inc<GameObjectReference>().End();
        BulletsQuery             = MainWorld.Filter<Bullet>().Inc<Transform>().Exc<Destroy>().End();
        DamageQuery              = MainWorld.Filter<Health>().Inc<Damage>().Exc<Destroy>().End();
        TempQuery                = MainWorld.Filter<Temporary>().Exc<Destroy>().End();
        FollowQuery              = MainWorld.Filter<FollowTarget>().Inc<Transform>().Inc<Movable>().Exc<Destroy>().End();
        MoveQuery                = MainWorld.Filter<Movable>().Inc<Transform>().Exc<Destroy>().End();
        AiAgentsQuery            = MainWorld.Filter<Movable>().Inc<Transform>().Inc<AiAgent>().Exc<Destroy>().End();
    }
}
