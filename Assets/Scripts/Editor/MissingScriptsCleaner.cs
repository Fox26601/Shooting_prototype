using UnityEditor;
using UnityEngine;

namespace ShootingSystem.Editor
{
	public static class MissingScriptsCleaner
	{
		[MenuItem("Tools/Shooting System/Clean Missing Scripts (Selection)")]
		public static void CleanSelection()
		{
			GameObject[] selection = Selection.gameObjects;
			if (selection == null || selection.Length == 0)
			{
				Debug.LogWarning("No GameObjects selected. Select objects or use 'Clean Missing Scripts (Scene)'.");
				return;
			}

			int removed = 0;
			foreach (var go in selection)
			{
				removed += RemoveMissingOnHierarchy(go);
			}
			Debug.Log($"✅ Removed {removed} missing script components from selection.");
		}

		[MenuItem("Tools/Shooting System/Clean Missing Scripts (Scene)")]
		public static void CleanScene()
		{
			int removed = 0;
			var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
			foreach (var root in roots)
			{
				removed += RemoveMissingOnHierarchy(root);
			}
			Debug.Log($"✅ Removed {removed} missing script components from entire scene.");
		}

		private static int RemoveMissingOnHierarchy(GameObject root)
		{
			int removed = 0;
			var transforms = root.GetComponentsInChildren<Transform>(true);
			foreach (var t in transforms)
			{
				removed += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
			}
			return removed;
		}
	}
}


