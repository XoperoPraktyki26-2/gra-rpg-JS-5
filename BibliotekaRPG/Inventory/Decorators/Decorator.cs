using System.Collections.Generic;

public class Decorator
{
    public readonly IStatModifier[] modifiers = new IStatModifier[6];

    private readonly List<IGameEventListener> listeners = new List<IGameEventListener>();

    public void AddListener(IGameEventListener listener)
    {
        listeners.Add(listener);
    }

    public void PutOn(IStatModifier item)
    {
        for (int i = 0; i < modifiers.Length; i++)
        {
            if (modifiers[i] == null)
            {
                modifiers[i] = item;

                listeners.ForEach(listener=>listener.OnEquipmentPutOn(item));

                return;
            }
        }
        listeners.ForEach(listener => listener.OnEquipmentSlotsFull());
        
        
    }

    public int GetBonusAttack()
    {
        int sum = 0;
        foreach (var m in modifiers)
            if (m != null)
                sum += m.ModifyAttack();
        return sum;
    }

    public int GetBonusHealth()
    {
        int sum = 0;
        foreach (var m in modifiers)
            if (m != null)
                sum += m.ModifyHealth();
        return sum;
    }

    public void ShowEquipment()
    {
        foreach (var listener in listeners)
            listener.OnShowEquipment(modifiers);
    }

    public void Remove(IStatModifier item)
    {
        for (int i = 0; i < modifiers.Length; i++)
        {
            if (modifiers[i] == item)
            {
                modifiers[i] = null;
                return;
            }
        }
    }
}
