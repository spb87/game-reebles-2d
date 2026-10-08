using System.Collections.Generic;
using UnityEngine;

namespace Reebles2D.Quests
{
    /// <summary>
    /// Loads all quest JSON definitions shipped in <c>Assets/Resources/Quests/</c>.
    /// Resources is used instead of a plain data folder because only Resources
    /// assets are included in WebGL builds at runtime.
    /// </summary>
    public static class QuestLibrary
    {
        private const string ResourcePath = "Quests";

        /// <summary>
        /// Parses every quest <see cref="TextAsset"/> under Resources/Quests.
        /// Returns an empty list when no quests are present.
        /// </summary>
        public static List<QuestDef> LoadAll()
        {
            var quests = new List<QuestDef>();
            foreach (TextAsset asset in Resources.LoadAll<TextAsset>(ResourcePath))
            {
                QuestDef quest = JsonUtility.FromJson<QuestDef>(asset.text);
                if (quest == null)
                {
                    Debug.LogError($"QuestLibrary: failed to parse quest JSON '{asset.name}'");
                    continue;
                }
                quests.Add(quest);
            }
            return quests;
        }

        /// <summary>
        /// Loads all quests into a dictionary keyed by <see cref="QuestDef.id"/>.
        /// Logs an error and keeps the first entry on duplicate ids.
        /// </summary>
        public static Dictionary<string, QuestDef> LoadById()
        {
            var byId = new Dictionary<string, QuestDef>();
            foreach (QuestDef quest in LoadAll())
            {
                if (byId.ContainsKey(quest.id))
                {
                    Debug.LogError($"QuestLibrary: duplicate quest id '{quest.id}'");
                    continue;
                }
                byId.Add(quest.id, quest);
            }
            return byId;
        }
    }
}
