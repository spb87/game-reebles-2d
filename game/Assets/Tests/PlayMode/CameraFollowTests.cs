using System.Collections;
using NUnit.Framework;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Reebles2D.Tests.PlayMode
{
    /// <summary>
    /// PlayMode regression tests for REEB-156: a bare CinemachineCamera does not
    /// move the render camera — the vcam needs a CinemachinePositionComposer and
    /// the render Camera needs a CinemachineBrain. These tests build the same
    /// wiring VillageSceneBuilder produces and assert the camera tracks the player.
    /// </summary>
    public class CameraFollowTests
    {
        private const string PlayerPrefabPath = "Assets/Prefabs/Player.prefab";

        private GameObject player;
        private GameObject cameraObject;
        private GameObject vcamObject;

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject go in new[] { player, cameraObject, vcamObject })
            {
                if (go != null)
                {
                    Object.Destroy(go);
                }
            }
        }

        private Camera SetupFollowCamera()
        {
            GameObject prefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            Assert.That(prefab, Is.Not.Null, "Player.prefab missing at " + PlayerPrefabPath);
            player = Object.Instantiate(prefab);
            player.transform.position = Vector3.zero;

            cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            cameraObject.transform.position = new Vector3(-8f, -8f, -10f);
            cameraObject.AddComponent<CinemachineBrain>();

            vcamObject = new GameObject("PlayerCamera");
            CinemachineCamera vcam = vcamObject.AddComponent<CinemachineCamera>();
            vcam.Follow = player.transform;
            vcam.Lens.OrthographicSize = 5f;
            CinemachinePositionComposer composer =
                vcamObject.AddComponent<CinemachinePositionComposer>();
            composer.Composition.DeadZone.Enabled = false;
            composer.Composition.DeadZone.Size = Vector2.zero;

            return camera;
        }

        [UnityTest]
        public IEnumerator FollowCamera_TracksPlayerAfterMove()
        {
            Camera camera = SetupFollowCamera();
            Vector3 cameraStart = cameraObject.transform.position;

            // Move the player well away from the camera, then let Cinemachine
            // run a few frames (brain updates the camera in LateUpdate).
            player.transform.position = new Vector3(5f, 3f, 0f);
            for (int i = 0; i < 10; i++)
            {
                yield return null;
            }

            float distanceBefore = Vector2.Distance(
                cameraStart, player.transform.position);
            float distanceAfter = Vector2.Distance(
                cameraObject.transform.position, player.transform.position);
            Assert.That(distanceAfter, Is.LessThan(distanceBefore),
                "camera did not move toward the player");
            Assert.That(
                Vector2.Distance(cameraObject.transform.position,
                    player.transform.position),
                Is.LessThan(1f),
                "camera should be tightly centered on the player (zero dead zone)");
        }

        [UnityTest]
        public IEnumerator FollowCamera_KeepsPlayerNearScreenCenter()
        {
            Camera camera = SetupFollowCamera();
            player.transform.position = new Vector3(4f, -2f, 0f);
            for (int i = 0; i < 10; i++)
            {
                yield return null;
            }

            Vector3 screenPoint = camera.WorldToViewportPoint(
                player.transform.position);
            Assert.That(screenPoint.x,
                Is.InRange(0.45f, 0.55f), "player not horizontally centered");
            Assert.That(screenPoint.y,
                Is.InRange(0.45f, 0.55f), "player not vertically centered");
        }
    }
}
