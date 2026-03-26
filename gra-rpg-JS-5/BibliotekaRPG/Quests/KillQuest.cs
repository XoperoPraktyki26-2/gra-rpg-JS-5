using System;

namespace BibliotekaRPG.Quests
{
    public class KillQuest : Quest
    {
        public KillQuest(string id, string title, string description, string targetEnemyName, int requiredKills, int goldReward, int experienceReward)
            : base(id, title, description, goldReward, experienceReward)
        {
            TargetEnemyName = targetEnemyName;
            RequiredKills = requiredKills;
        }

        public string TargetEnemyName { get; }
        public int RequiredKills { get; }
        public int CurrentKills { get; private set; }

        public override string ProgressDescription => IsCompleted
            ? "Zakończone"
            : $"{CurrentKills}/{RequiredKills} x {TargetEnemyName}";

        public override bool TryTrackKill(Enemy enemy)
        {
            if (enemy == null || IsCompleted)
                return false;

            if (!string.Equals(enemy.Name, TargetEnemyName, StringComparison.OrdinalIgnoreCase))
                return false;

            CurrentKills = Math.Min(RequiredKills, CurrentKills + 1);

            if (CurrentKills >= RequiredKills)
                Complete();

            return true;
        }

        public override QuestData ToData()
        {
            return new QuestData
            {
                Type = QuestData.KillType,
                Id = Id,
                Title = Title,
                Description = Description,
                GoldReward = GoldReward,
                ExperienceReward = ExperienceReward,
                TargetEnemyName = TargetEnemyName,
                RequiredKills = RequiredKills,
                CurrentKills = CurrentKills,
                IsCompleted = IsCompleted,
                RewardClaimed = RewardClaimed
            };
        }

        internal static KillQuest FromData(QuestData data)
        {
            var quest = new KillQuest(data.Id, data.Title, data.Description, data.TargetEnemyName, data.RequiredKills, data.GoldReward, data.ExperienceReward)
            {
                CurrentKills = data.CurrentKills
            };

            if (data.IsCompleted)
                quest.Complete();

            if (data.RewardClaimed)
                quest.MarkRewardClaimed();

            return quest;
        }
    }
}
