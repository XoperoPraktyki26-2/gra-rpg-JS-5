using System.Collections.Generic;
using BibliotekaRPG.Inventory.Decorators;
using BibliotekaRPG.map;

public interface IGameEventListener
{
    void OnAttack(Character attacker, Character target, int damage);
    void OnLevelUp(Player player);

    void OnBattleStart(Enemy enemy);
    void OnEnemyDefeated(Enemy enemy);
    void OnPlayerDefeated(Player player);

    void OnItemUsed(Character user, IItem item);
    void OnEquipmentPutOn(IStatModifier item);
    void OnEquipmentSlotsFull();

    void OnShowStats(Player player);
    void OnShowInventory(List<IItem> items);
    void OnShowEquipment(IStatModifier[] items);
    void ShowMap(WorldMap map);
}
