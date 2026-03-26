using BibliotekaRPG;

namespace BibliotekaRPG.Quests
{
    public abstract class Quest
    {
        protected Quest(string id, string title, string description, int goldReward, int experienceReward)
        {
            Id = id;
            Title = title;
            Description = description;
            GoldReward = goldReward;
            ExperienceReward = experienceReward;
        }

        public string Id { get; }
        public string Title { get; }
        public string Description { get; }
        public int GoldReward { get; }
        public int ExperienceReward { get; }
        public bool IsCompleted { get; protected set; }
        public bool RewardClaimed { get; protected set; }

        public string RewardDescription => $"{GoldReward} zł, {ExperienceReward} exp";

        public abstract string ProgressDescription { get; }

        public abstract QuestData ToData();

        public abstract bool TryTrackKill(Enemy enemy);

        public static Quest? FromData(QuestData data)
        {
            if (data == null)
                return null;

            return data.Type switch
            {
                QuestData.KillType => KillQuest.FromData(data),
                _ => null
            };
        }

        protected void Complete()
        {
            IsCompleted = true;
        }

        protected internal void MarkRewardClaimed()
        {
            RewardClaimed = true;
        }
    }
}
