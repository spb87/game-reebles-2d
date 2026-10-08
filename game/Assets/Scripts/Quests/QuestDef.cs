using System;

namespace Reebles2D.Quests
{
    /// <summary>
    /// Flat, JsonUtility-serializable quest definition loaded from JSON text assets
    /// (DEC-6). JsonUtility cannot deserialize nested dictionaries or polymorphic
    /// types, so the schema is intentionally flat with string arrays for dialogue.
    /// </summary>
    [Serializable]
    public class QuestDef
    {
        public string id;
        public string giverNpcId;
        public string title;
        public string objectiveText;
        public string[] offerLines;
        public string[] activeLines;
        public string[] completeLines;
        public string fetchItemId;
        public string fetchTargetId;
        public int rewardHearts;
    }
}
