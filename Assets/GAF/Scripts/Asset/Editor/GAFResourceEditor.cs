/*
 * File:			GAFResourceEditor.cs
 * Version:			1.0
 * Last changed:	2014/12/12 14:37
 * Author:			Alexey_Nikitin
 * Copyright:		© GAF Media
 * Project:			UnityVS.UnityProject.CSharp.Editor
 */

using UnityEngine;
using UnityEditor;

using System.Collections.Generic;
using System.IO;

using GAF.Assets;

using GAFEditor.Utils;
using GAFEditor.ExternalEditor;

namespace GAFEditor.Assets
{
	[CustomEditor(typeof(GAFTexturesResource))]
	public class GAFResourceEditor : Editor
	{
		private string m_DrawPath = string.Empty;

		new private GAFTexturesResource target
		{
			get
			{
				return (GAFTexturesResource)base.target;
			}
		}

		private void OnEnable()
		{
			if (!target.isValid)
				AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(target));
			else
				normalizeDrawPath();
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			GUILayout.Space(3f);
			EditorGUILayout.ObjectField(target.asset, typeof(GAFAnimationAsset), false);

			GUILayout.Space(6f);
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("Scale = " + target.scale.ToString());
				GUILayout.Label("Csf = " + target.csf.ToString());
				GUILayout.FlexibleSpace();
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(6f);
			GUILayout.Label("* This folder will be used to find missing textures!");
			GAFEditorHelper.drawFolder(
				  ref m_DrawPath
				, target.currentDataPath
				, Application.dataPath + "/"
				, "Data directory: Assets/"
				, "Select destination folder"
				, (newPath) =>
				{
					relocateResourceData(target, "Assets/" + newPath);
					normalizeDrawPath();
				}
				, () =>
				{
					normalizeDrawPath();
				});

			GUI.enabled = !target.isReady;
			if (GUILayout.Button("Find textures!"))
			{
				GAFResourceManager.findResourceTextures(target, false);
			}
			GUI.enabled = true;

			if (target.data.Count > 0)
			{
				GUILayout.Space(6f);
				GUILayout.Label("Found resource data:");
				GUILayout.Space(3f);
				drawResourceDataList(target.data);
			}

			if (target.missingData.Count > 0)
			{
				GUILayout.Space(6f);
				GUILayout.Label("Not found resource data:");
				GUILayout.Space(3f);
				drawResourceDataList(target.missingData);
			}
		}

		private void drawResourceDataList(List<GAFResourceData> _Data)
		{
			foreach (var data in _Data)
			{
				GUILayout.Label(data.name);
				var texture = EditorGUILayout.ObjectField(data.sharedTexture, typeof(Texture2D), false) as Texture2D;
				if (texture != data.sharedTexture)
				{
					if (texture != null)
					{
						var mat = GAFResourceManager.getSharedMaterial(texture);
						mat.mainTexture = texture;
						data.set(texture, mat);
						EditorUtility.SetDirty(mat);
					}
					else
					{
						data.set(null, null);
					}

					EditorUtility.SetDirty(target);
				}

				var material = EditorGUILayout.ObjectField(data.sharedMaterial, typeof(Material), false) as Material;
				if (material != data.sharedMaterial)
				{
					data.set(data.sharedTexture, material);
				}

				GUILayout.Space(6f);
			}
		}

		private void normalizeDrawPath()
		{
			int length = "Assets/".Length;
			m_DrawPath = target.currentDataPath.Substring(length, target.currentDataPath.Length - length);
		}

		private void relocateResourceData(GAFTexturesResource _Resource, string _NewPath)
		{
			foreach (var data in _Resource.data)
			{
				var texturePath		= AssetDatabase.GetAssetPath(data.sharedTexture);
				var materialPath	= AssetDatabase.GetAssetPath(data.sharedMaterial);

				var newTexturePath	= _NewPath + Path.GetFileName(texturePath);
				var newMaterialPath = _NewPath + Path.GetFileName(materialPath);

				AssetDatabase.MoveAsset(texturePath, newTexturePath);
				AssetDatabase.MoveAsset(materialPath, newMaterialPath);
			}

			_Resource.currentDataPath = _NewPath;

			EditorUtility.SetDirty(_Resource);
		}
	}
}