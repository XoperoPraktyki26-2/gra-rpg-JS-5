using System;

public class Factory
{
    private readonly Random rand = new Random();

    private readonly IEnemyFactory[] factories =
    {
        new GoblinFactory(),
        new MageFactory(),
        new OrkFactory()
    };

    public Enemy Spawn()
    {
        return factories[rand.Next(factories.Length)].CreateEnemy();
    }
}