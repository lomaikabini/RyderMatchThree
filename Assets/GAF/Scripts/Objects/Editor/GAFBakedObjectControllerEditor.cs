/*
 * File:			GAFBakedObjectControllerEditor.cs
 * Version:			1.0
 * Last changed:	2014/12/2 15:55
 * Author:			Alexey_Nikitin
 * Copyright:		© GAF Media
 * Project:			UnityVS.UnityProject.CSharp.Editor
 */

using UnityEditor;
using UnityEngine;

using GAF.Objects;
using GAFEditor.ExternalEditor;

namespace GAFEditor.Objects
{
	[CustomEditor(typeof(GAFBakedObjectController))]
	public class GAFBakedObjectControllerEditor : Editor
	{
		#region Properties

		new public GAFBakedObjectController target
		{
			get
			{
				return base.target as GAFBakedObjectController;
			}
		}

		#endregion // Properties

		private void OnEnable()
		{
			EditorApplication.update += OnUpdate;
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

            GUILayout.Space(3f);
            GAFInspectorLine.draw(new Color(125f / 255f, 125f / 255f, 125f / 255f), 1f);
            EditorGUILayout.LabelField("Type: " + target.bakedObject.serializedProperties.type.ToString());
            GAFInspectorLine.draw(new Color(125f / 255f, 125f / 255f, 125f / 255f), 1f);

			GUILayout.Space(3f);
			var offset = EditorGUILayout.Vector3Field("Position offset: ", target.bakedObject.serializedProperties.offset);
			if (offset != target.bakedObject.serializedProperties.offset)
			{
				target.bakedObject.serializedProperties.offset = offset;
				target.transform.localPosition = target.bakedObject.serializedProperties.offset;
				target.bakedObject.serializedProperties.clip.reload();
			}

			GAFInspectorLine.draw(new Color(125f / 255f, 125f / 255f, 125f / 255f), 1f);
			GUILayout.Space(2f);
			var visible = EditorGUILayout.Toggle("Visible: ", target.bakedObject.serializedProperties.visible);
			if (visible != target.bakedObject.serializedProperties.visible)
			{
				target.bakedObject.serializedProperties.visible = visible;
				target.bakedObject.serializedProperties.clip.reload();
			}

			GAFInspectorLine.draw(new Color(125f / 255f, 125f / 255f, 125f / 255f), 1f);
			GUILayout.Space(3f);
			EditorGUI.BeginChangeCheck();
			{
				target.bakedObject.serializedProperties.meshSizeMultiplier = EditorGUILayout.Vector2Field("Mesh scale: ", target.bakedObject.serializedProperties.meshSizeMultiplier);

				if (EditorGUI.EndChangeCheck())
				{
					target.bakedObject.serializedProperties.clip.reload();
				}
			}

			GAFInspectorLine.draw(new Color(125f / 255f, 125f / 255f, 125f / 255f), 1f);
			GUILayout.Space(3f);
			EditorGUILayout.LabelField("* Custom material will break dynamic batching!");
			var material = EditorGUILayout.ObjectField("Custom material: ", target.bakedObject.serializedProperties.material, typeof(Material), false) as Material;
			if (material != target.bakedObject.serializedProperties.material)
			{
				target.bakedObject.serializedProperties.material = material;
				target.bakedObject.serializedProperties.clip.reload();
			}

			GAFInspectorLine.draw(new Color(125f / 255f, 125f / 255f, 125f / 255f), 1f);
			GUILayout.Space(3f);
			var useCustomTextureRect = EditorGUILayout.Foldout(target.bakedObject.serializedProperties.useCustomTextureRect, "Custom texture rect:");
			if (useCustomTextureRect != target.bakedObject.serializedProperties.useCustomTextureRect)
			{
				target.bakedObject.serializedProperties.useCustomTextureRect = useCustomTextureRect;
				if (!useCustomTextureRect)
				{
					target.bakedObject.serializedProperties.clip.reload();
				}
			}

			if (useCustomTextureRect)
			{
				EditorGUI.BeginChangeCheck();
				{
					target.bakedObject.serializedProperties.atlasTextureRect = EditorGUILayout.RectField(target.bakedObject.serializedProperties.atlasTextureRect);

					if (EditorGUI.EndChangeCheck())
					{
						target.bakedObject.serializedProperties.clip.reload();
					}
				}
			}

			GAFInspectorLine.draw(new Color(125f / 255f, 125f / 255f, 125f / 255f), 1f);
			GUILayout.Space(5f);
			if (GUILayout.Button(new GUIContent("Copy mesh")))
			{
				target.copyMesh();
			}
		}

		private void OnUpdate()
		{
			if (target != null)
			{
				if (target.transform.localPosition != target.bakedObject.serializedProperties.offset)
				{
					target.bakedObject.serializedProperties.offset = target.transform.localPosition;
					target.bakedObject.serializedProperties.clip.reload();
				}
			}
			else
			{
				EditorApplication.update -= OnUpdate;
			}
		}
	}
}
