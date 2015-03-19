/*
 * File:			GAFConverterWindowListener.cs
 * Version:			1.0
 * Last changed:	2014/10/22 12:53
 * Author:			Alexey_Nikitin
 * Copyright:		© GAF Media
 * Project:			UnityVS.UnityProject.CSharp.Editor
 */

using UnityEditor;
using UnityEngine;

using System.Collections.Generic;
using System.IO;

using GAF.Core;
using GAF.Assets;
using GAF.Utils;

using GAFEditor.Converter.Window;

namespace GAFEditor.Core
{
	[InitializeOnLoad]
	public static class GAFConverterWindowListener
	{
		static GAFConverterWindowListener()
		{
			GAFConverterWindowEventDispatcher.onCreateMovieClipEvent                    += onCreateMovieClip;
			GAFConverterWindowEventDispatcher.onCreateMovieClipPrefabEvent              += onCreatePrefab;
			GAFConverterWindowEventDispatcher.onCreateMovieClipPrefabPlusInstanceEvent  += onCreatePrefabPlusInstance;
		}

		private static void onCreateMovieClip(string _AssetPath)
		{
			var assetName = Path.GetFileNameWithoutExtension(_AssetPath).Replace(" ", "_");
			var assetDir = "Assets" + Path.GetDirectoryName(_AssetPath).Replace(Application.dataPath, "") + "/";

			var asset = AssetDatabase.LoadAssetAtPath(assetDir + assetName + ".asset", typeof(GAFAnimationAsset)) as GAFAnimationAsset;
			if (!System.Object.Equals(asset, null))
			{
				var movieClipObject = createMovieClip(asset);

				var selected = new List<Object>(Selection.gameObjects);
				selected.Add(movieClipObject);
				Selection.objects = selected.ToArray();
			}
			else
			{
				GAFUtils.Log("Cannot find asset with path - " + _AssetPath, "");
			}
		}

		private static void onCreatePrefab(string _AssetPath)
		{
			var assetName = Path.GetFileNameWithoutExtension(_AssetPath).Replace(" ", "_");
			var assetDir = "Assets" + Path.GetDirectoryName(_AssetPath).Replace(Application.dataPath, "") + "/";

			var asset = AssetDatabase.LoadAssetAtPath(assetDir + assetName + ".asset", typeof(GAFAnimationAsset)) as GAFAnimationAsset;
			if (!System.Object.Equals(asset, null))
			{
				var selected = new List<Object>(Selection.gameObjects);

				var prefabPath = assetDir + assetName + ".prefab";
				var existingPrefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
				if (existingPrefab == null)
				{
					var movieClipObject = createMovieClip(asset);
					var prefab = PrefabUtility.CreateEmptyPrefab(assetDir + assetName + ".prefab");
					prefab = PrefabUtility.ReplacePrefab(movieClipObject, prefab, ReplacePrefabOptions.ConnectToPrefab);
					GameObject.DestroyImmediate(movieClipObject);
					selected.Add(prefab);
				}
				else
				{
					selected.Add(existingPrefab);
				}

				Selection.objects = selected.ToArray();
			}
			else
			{
				GAFUtils.Log("Cannot find asset with path - " + _AssetPath, "");
			}
		}

		private static void onCreatePrefabPlusInstance(string _AssetPath)
		{
			var assetName = Path.GetFileNameWithoutExtension(_AssetPath).Replace(" ", "_");
			var assetDir = "Assets" + Path.GetDirectoryName(_AssetPath).Replace(Application.dataPath, "") + "/";

			var asset = AssetDatabase.LoadAssetAtPath(assetDir + assetName + ".asset", typeof(GAFAnimationAsset)) as GAFAnimationAsset;
			if (!System.Object.Equals(asset, null))
			{
				var selected = new List<Object>(Selection.gameObjects);

				var prefabPath = assetDir + assetName + ".prefab";
				var existingPrefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
				if (existingPrefab == null)
				{
					var movieClipObject = createMovieClip(asset);
					var prefab = PrefabUtility.CreateEmptyPrefab(assetDir + assetName + ".prefab");
					prefab = PrefabUtility.ReplacePrefab(movieClipObject, prefab, ReplacePrefabOptions.ConnectToPrefab);

					selected.Add(movieClipObject);
					selected.Add(prefab);
				}
				else
				{
					var instance = PrefabUtility.InstantiatePrefab(existingPrefab) as GameObject;
					selected.Add(existingPrefab);
					selected.Add(instance);
				}

				Selection.objects = selected.ToArray();
			}
			else
			{
				GAFUtils.Log("Cannot find asset with path - " + _AssetPath, "");
			}
		}

		private static GameObject createMovieClip(GAFAnimationAsset _Asset)
		{
			var clipObject = new GameObject(_Asset.name);

		    var clip = clipObject.AddComponent<GAFMovieClip>();
			clip.initialize(_Asset);
			clip.reload();

			return clipObject;
		}
	}
}