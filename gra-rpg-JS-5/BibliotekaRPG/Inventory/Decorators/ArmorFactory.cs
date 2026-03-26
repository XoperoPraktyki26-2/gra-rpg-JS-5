using System;
using BibliotekaRPG.Inventory.Items;

namespace BibliotekaRPG.Inventory.Decorators;




public class ArmorFactory : IStatmodierFactory
{
    class objArm(string name, int attBnsMin,int attBnsMax, int hltBnsMin, int hltBnsMax)
    {
        public string name { get; set; }
        public int attBnsMin{ get; set; }
        public int attBnsMax{ get; set; }
        public int hltBnsMin{ get; set; }
        public int hltBnsMax{ get; set; }
    }
    private Random random = new Random();

    private objArm[] arm =
    {
        new objArm("Hełm", 0,1, 6,8),
        new objArm("Napierśnik", 0,1, 14,17),
        new objArm("Spodnie", 0,0, 9,11)
        
        
    };

    
    public IStatModifier CreateStatModifier()
    {
        int r = random.Next(0, arm.Length);
        return new ArmorPiece(arm[r].name,
            (arm[r].attBnsMin!=arm[r].attBnsMax)?random.Next(arm[r].attBnsMin,arm[r].attBnsMax+1):arm[r].attBnsMin,
            (arm[r].hltBnsMin!=arm[r].hltBnsMax)?random.Next(arm[r].hltBnsMin,arm[r].hltBnsMax+1):arm[r].hltBnsMin); 
    }
}
