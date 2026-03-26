namespace BibliotekaRPG.Quests
{
    public class QuestData
    {
        public const string KillType = "kill";

        public string Type { get; set; }
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int GoldReward { get; set; }
        public int ExperienceReward { get; set; }
        public string TargetEnemyName { get; set; }
        public int RequiredKills { get; set; }
        public int CurrentKills { get; set; }
        public bool IsCompleted { get; set; }
        public bool RewardClaimed { get; set; }
    }
}
