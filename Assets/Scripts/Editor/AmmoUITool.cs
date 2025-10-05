using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using ShootingSystem;

namespace ShootingSystem.Editor
{
	public class AmmoUITool : EditorWindow
	{
		[MenuItem("Tools/Shooting System/Ammo UI")] 
		public static void ShowWindow()
		{
			GetWindow<AmmoUITool>("Ammo UI");
		}

		private void OnGUI()
		{
			GUILayout.Label("Ammo UI Tool", EditorStyles.boldLabel);
			GUILayout.Space(8);

			if (GUILayout.Button("Create or Select AmmoUI", GUILayout.Height(28)))
			{
				CreateOrSelectAmmoUI();
			}

			if (GUILayout.Button("Parent Under Game UI Canvas", GUILayout.Height(28)))
			{
				ParentAmmoUIUnderCanvas();
			}

			if (GUILayout.Button("Force Recreate UI Elements", GUILayout.Height(28)))
			{
				ForceRecreateElements();
			}
		}

		private static void CreateOrSelectAmmoUI()
		{
			AmmoUI ammoUI = Object.FindFirstObjectByType<AmmoUI>();
			if (ammoUI == null)
			{
				GameObject go = new GameObject("AmmoUI");
				ammoUI = go.AddComponent<AmmoUI>();
				Debug.Log("✅ Created AmmoUI GameObject.");
			}
			Selection.activeGameObject = ammoUI.gameObject;
		}

		private static void ParentAmmoUIUnderCanvas()
		{
			AmmoUI ammoUI = Object.FindFirstObjectByType<AmmoUI>();
			if (ammoUI == null)
			{
				Debug.LogError("❌ AmmoUI not found. Use 'Create or Select AmmoUI' first.");
				return;
			}

			Canvas canvas = FindMainCanvas();
			if (canvas == null)
			{
				// Create a simple overlay canvas if none exists
				GameObject canvasObj = new GameObject("Game UI Canvas");
				canvas = canvasObj.AddComponent<Canvas>();
				canvas.renderMode = RenderMode.ScreenSpaceOverlay;
				canvasObj.AddComponent<CanvasScaler>();
				canvasObj.AddComponent<GraphicRaycaster>();
				Debug.Log("✅ Created 'Game UI Canvas'.");
			}

			ammoUI.transform.SetParent(canvas.transform);
			Selection.activeGameObject = ammoUI.gameObject;
			Debug.Log("✅ AmmoUI parented under Canvas.");
		}

		private static void ForceRecreateElements()
		{
			AmmoUI ammoUI = Object.FindFirstObjectByType<AmmoUI>();
			if (ammoUI == null)
			{
				Debug.LogError("❌ AmmoUI not found. Use 'Create or Select AmmoUI' first.");
				return;
			}

			// Ensure it's under a canvas first
			ParentAmmoUIUnderCanvas();

			ammoUI.ForceRecreateUIElements();
			EditorUtility.SetDirty(ammoUI);
			Debug.Log("✅ UI elements recreated via tool.");
		}

		private static Canvas FindMainCanvas()
		{
			Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
			foreach (Canvas c in canvases)
			{
				if (c.name == "Game UI Canvas" || c.name.Contains("Game UI Canvas"))
				{
					return c;
				}
			}
			foreach (Canvas c in canvases)
			{
				if (c.renderMode == RenderMode.ScreenSpaceOverlay) return c;
			}
			return canvases.Length > 0 ? canvases[0] : null;
		}
	}
}


