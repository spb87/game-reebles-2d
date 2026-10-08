using System.Collections.Generic;
using NUnit.Framework;
using Reebles2D.Quests;
using UnityEngine;

namespace Reebles2D.Tests.EditMode
{
    /// <summary>
    /// EditMode tests for the quest JSON schema (DEC-6): every quest asset under
    /// Resources/Quests must parse via JsonUtility and have all required fields.
    /// </summary>
    public class QuestSchemaTests
    {
        private static List<QuestDef> AllQuests => QuestLibrary.LoadAll();

        [Test]
        public void LoadAll_AtLeastOneQuestExists()
        {
            Assert.Greater(AllQuests.Count, 0, "Expected at least one quest JSON under Resources/Quests");
        }

        [Test]
        public void LoadAll_EveryQuestParsesWithRequiredFields()
        {
            foreach (QuestDef quest in AllQuests)
            {
                Assert.IsNotNull(quest, "Quest JSON failed to parse via JsonUtility");
                Assert.IsFalse(string.IsNullOrEmpty(quest.id), "Quest missing 'id'");
                Assert.IsFalse(string.IsNullOrEmpty(quest.giverNpcId), $"Quest '{quest.id}' missing 'giverNpcId'");
                Assert.IsFalse(string.IsNullOrEmpty(quest.title), $"Quest '{quest.id}' missing 'title'");
                Assert.IsFalse(string.IsNullOrEmpty(quest.objectiveText), $"Quest '{quest.id}' missing 'objectiveText'");
                Assert.IsFalse(string.IsNullOrEmpty(quest.fetchItemId), $"Quest '{quest.id}' missing 'fetchItemId'");
                Assert.IsFalse(string.IsNullOrEmpty(quest.fetchTargetId), $"Quest '{quest.id}' missing 'fetchTargetId'");
                Assert.Greater(quest.rewardHearts, 0, $"Quest '{quest.id}' must reward at least 1 heart");

                Assert.NotNull(quest.offerLines, $"Quest '{quest.id}' missing 'offerLines'");
                Assert.NotNull(quest.activeLines, $"Quest '{quest.id}' missing 'activeLines'");
                Assert.NotNull(quest.completeLines, $"Quest '{quest.id}' missing 'completeLines'");
                Assert.Greater(quest.offerLines.Length, 0, $"Quest '{quest.id}' has empty 'offerLines'");
                Assert.Greater(quest.activeLines.Length, 0, $"Quest '{quest.id}' has empty 'activeLines'");
                Assert.Greater(quest.completeLines.Length, 0, $"Quest '{quest.id}' has empty 'completeLines'");
            }
        }

        [Test]
        public void LoadById_QuestIdsAreUnique()
        {
            Dictionary<string, QuestDef> byId = QuestLibrary.LoadById();
            Assert.AreEqual(AllQuests.Count, byId.Count, "Duplicate quest ids detected");
        }

        [Test]
        public void ErrandBerries_HasExpectedFields()
        {
            Dictionary<string, QuestDef> byId = QuestLibrary.LoadById();
            Assert.IsTrue(byId.ContainsKey("errand_berries"), "errand_berries quest not found");

            QuestDef quest = byId["errand_berries"];
            Assert.AreEqual("marla_baker", quest.giverNpcId);
            Assert.AreEqual("berries", quest.fetchItemId);
            Assert.AreEqual("berry_bush", quest.fetchTargetId);
            Assert.AreEqual(1, quest.rewardHearts);
        }

        [Test]
        public void FromJson_MalformedJsonDoesNotProduceValidQuest()
        {
            // Negative schema check: JsonUtility should not fill required fields
            // from a JSON payload that omits them.
            QuestDef quest = JsonUtility.FromJson<QuestDef>("{\"title\":\"Incomplete\"}");
            Assert.IsTrue(string.IsNullOrEmpty(quest.id));
            Assert.IsTrue(string.IsNullOrEmpty(quest.fetchItemId));
            Assert.AreEqual(0, quest.rewardHearts);
        }
    }
}
