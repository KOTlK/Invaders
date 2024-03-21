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
    public static EcsFilter PatrolQuery;
    
    public static void InitQueries()
    {
        TransformReferencesQuery = MainWorld.Filter<Transform>().Inc<GameObjectReference>().End();
        ShipsQuery               = MainWorld.Filter<Ship>().Inc<Transform>().Exc<Destroy>().End();
        PlayerShipQuery          = MainWorld.Filter<Ship>().Inc<Player>().Inc<Transform>().Exc<Destroy>().End();
        DestroyRefQuery          = MainWorld.Filter<Destroy>().Inc<GameObjectReference>().End();
        BulletsQuery             = MainWorld.Filter<Bullet>().Inc<Transform>().Inc<Movement>().Exc<Destroy>().End();
        DamageQuery              = MainWorld.Filter<Health>().Inc<Damage>().Exc<Destroy>().End();
        TempQuery                = MainWorld.Filter<Temporary>().Exc<Destroy>().End();
        FollowQuery              = MainWorld.Filter<FollowTarget>().Inc<Transform>().Inc<Ship>().Exc<Destroy>().End();
        MoveQuery                = MainWorld.Filter<Movement>().Inc<Transform>().Exc<Destroy>().Exc<Player>().End();
        PatrolQuery              = MainWorld.Filter<Transform>().Inc<Movement>().Inc<Ship>().Inc<Patrol>().Exc<Destroy>().End();
    }
}
