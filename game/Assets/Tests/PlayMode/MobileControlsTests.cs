using System.Collections;
using NUnit.Framework;
using Reebles2D.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Reebles2D.Tests.PlayMode
{
    /// <summary>
    /// Verifies the generated Village scene carries the mobile on-screen
    /// controls and that their control paths resolve to bindings on the
    /// Player action map.
    /// </summary>
    public class MobileControlsTests
    {
        private const string ScenePath = "Assets/Scenes/Village.unity";
        private const string InputActionsPath = "Assets/Input/ReeblesInput.inputactions";

        private static bool HasBinding(InputAction action, string controlPath)
        {
            foreach (InputBinding binding in action.bindings)
            {
                if (!binding.isComposite && !binding.isPartOfComposite &&
                    binding.effectivePath == controlPath)
                {
                    return true;
                }
            }
            return false;
        }

        [UnityTest]
        public IEnumerator VillageScene_OnScreenControlsFeedPlayerMap()
        {
            EditorSceneManager.LoadSceneInPlayMode(
                ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            Scene villageScene = SceneManager.GetSceneByPath(ScenePath);
            Assert.That(villageScene.IsValid(), Is.True, "Village scene failed to load");

            MobileControlsHud hud =
                Object.FindFirstObjectByType<MobileControlsHud>(FindObjectsInactive.Include);
            Assert.That(hud, Is.Not.Null, "MobileControls canvas missing from Village scene");

            OnScreenStick stick = hud.GetComponentInChildren<OnScreenStick>(true);
            OnScreenButton button = hud.GetComponentInChildren<OnScreenButton>(true);
            Assert.That(stick, Is.Not.Null, "OnScreenStick missing");
            Assert.That(button, Is.Not.Null, "OnScreenButton missing");
            Assert.That(stick.controlPath, Is.EqualTo("<Gamepad>/leftStick"));
            Assert.That(button.controlPath, Is.EqualTo("<Gamepad>/buttonEast"));

            InputActionAsset actions =
                AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            Assert.That(actions, Is.Not.Null);
            InputActionMap playerMap = actions.FindActionMap("Player", true);
            InputAction move = playerMap.FindAction("Move", true);
            InputAction interact = playerMap.FindAction("Interact", true);

            Assert.That(HasBinding(move, stick.controlPath), Is.True,
                "Move has no binding matching the OnScreenStick control path");
            Assert.That(HasBinding(interact, button.controlPath), Is.True,
                "Interact has no binding matching the OnScreenButton control path");
            Assert.That(HasBinding(move, button.controlPath), Is.False,
                "Move should not be bound to the interact button path");

            // Leave no loaded scene behind: its world colliders (e.g. the
            // fountain at the origin) would pin kinematic test players.
            SceneManager.SetActiveScene(SceneManager.CreateScene("Empty"));
            yield return SceneManager.UnloadSceneAsync(villageScene);
        }
    }
}
