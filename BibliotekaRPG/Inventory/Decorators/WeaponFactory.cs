using System;

namespace BibliotekaRPG.Inventory.Decorators;


public class WeaponFactory : IStatmodierFactory
{
    class objArm(string name, int attBnsMin,int attBnsMax, int hltBnsMin, int hltBnsMax)
    {
        public string name { get; set; }
        public int attBnsMin{ get; set; }
        public int attBnsMax{ get; set; }
        public int hltBnsMin{ get; set; }
        public int hltBnsMax{ get; set; }
    }
    private objArm[] arm =
    {
        new objArm("Miecz", 5,9, 0,2),
        new objArm("Większy Miecz", 10,15, 1,3),
        new objArm("Tarza", 0,2, 6,9)
        
        
    };
    private Random random = new Random();
    public IStatModifier CreateStatModifier()
    {
        
        int r = random.Next(0, arm.Length);
        return new Weapon(arm[r].name,
            (arm[r].attBnsMin!=arm[r].attBnsMax)?random.Next(arm[r].attBnsMin,arm[r].attBnsMax+1):arm[r].attBnsMin,
            (arm[r].hltBnsMin!=arm[r].hltBnsMax)?random.Next(arm[r].hltBnsMin,arm[r].hltBnsMax+1):arm[r].hltBnsMin); 
    }
}