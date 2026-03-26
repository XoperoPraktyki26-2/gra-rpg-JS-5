using System;
using System.Collections.Generic;

namespace BibliotekaRPG.Npcs
{
    public class NpcGenerator
    {
        private readonly Random rng = new();

        private static readonly string[] Names =
        {
            "Jakub", "Agnieszka", "Marek", "Zuzanna", "Tomasz", "Natalia", "Borys", "Helena"
        };

        private static readonly string[] Roles =
        {
            "Strażnik polany", "Kupiec przydrożny", "Mistrz łowczy", "Zielarka", "Zwiadowca"
        };

        private static readonly string[] Dialogues =
        {
            "Czuję, że potwory zbierają się na południu. Trzeba ich przegonić.",
            "Widzę, że masz dobrą minę. Może odpowiesz na kilka pytań o okoliczne patrole?",
            "Niewiele osób odwiedza nasze okolice. Twoje imię już tu brzmi dumnie.",
            "Przyniosłem kilka świeżych ziół, ale wiem, że większą potrzebą są informacje."
        };

        private static readonly string[] TaskHints =
        {
            "Orkowie przemykają w grupach po zachodnich szlakach, spróbuj zagarować ich drogę.",
            "Gobliny lubią zaskakiwać karawany nocą. Szukaj ich w lesie północnym.",
            "Zasil patrol miejski, zanim rośnie ich liczba. Porozmawiaj z szefem straży w wiosce."
        };

        public NpcTile Generate()
        {
            var data = new NpcData
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = Names[rng.Next(Names.Length)],
                Role = Roles[rng.Next(Roles.Length)],
                Dialogue = Dialogues[rng.Next(Dialogues.Length)],
                TaskHint = TaskHints[rng.Next(TaskHints.Length)],
                Opinion = rng.Next(35, 91)
            };

            return new NpcTile(data);
        }
    }
}
