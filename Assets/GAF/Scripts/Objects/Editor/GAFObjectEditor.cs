/*
 * File:			GAFObjectEditor.cs
 * Version:			1.0
 * Last changed:	2014/12/8 16:27
 * Author:			Alexey_Nikitin
 * Copyright:		© GAF Media
 * Project:			UnityVS.UnityProject.CSharp.Editor
 */

using UnityEditor;
using UnityEngine;

using System.Linq;
using System.Collections.Generic;

using GAF.Objects;
using GAFEditor.ExternalEditor;

namespace GAFEditor.Objects
{
	[CustomEditor(typeof(GAFObject))]
	[CanEditMultipleObjects]
	public class GAFObjectEditor : Editor
	{
		new public List<GAFObject> targets
		{
			get
			{
				return base.targets.ToList().ConvertAll(_target => _target as GAFObject);
			}
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			serializedObject.Update();

			var properties = serializedObject.FindProperty("m_Data");

			var type = properties.FindPropertyRelative("m_Type");
			GUILayout.Space(3f);
			GAFInspectorLine.draw(new Color(125f / 255f, 125f / 255f, 125f / 255f), 1f);
			EditorGUILayout.LabelField("Type: " + (!type.hasMultipleDifferentValues ? type.enumNames[type.enumValueIndex] : "—"), EditorStyles.boldLabel);
			GAFInspectorLine.draw(new Color(125f / 255f, 125f / 255f, 125f / 255f), 1f);

			var visible = properties.FindPropertyRelative("m_IsVisible");
			GUILayout.Space(2f);
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(visible, new GUIContent("Visible: "));
			if (EditorGUI.EndChangeCheck())
			{
				serializedObject.ApplyModifiedProperties();
				reloadClips();
			}

			GAFInspectorLine.draw(new Color(125f / 255f, 125f / 255f, 125f / 255f), 1f);

			var offset = properties.FindPropertyRelative("m_Offset");
			GUILayout.Space(3f);
			EditorGUI.BeginChangeCheck();
			drawVector3Property(offset, "Position offset: ");
			if (EditorGUI.EndChangeCheck())
			{
				serializedObject.ApplyModifiedProperties();
				foreach(var _target in targets)
					_target.transform.localPosition = _target.serializedProperties.statePosition + offset.vector3Value + (Vector3)_target.serializedProperties.clip.settings.pivotOffset;
			}

			GAFInspectorLine.draw(new Color(125f / 255f, 125f / 255f, 125f / 255f), 1f);

			var meshSizeMultiplier = properties.FindPropertyRelative("m_MeshSizeMultiplier");
			GUILayout.Space(3f);
			EditorGUI.BeginChangeCheck();
			drawVector2Property(meshSizeMultiplier, "Mesh scale: ");
			if (EditorGUI.EndChangeCheck())
			{
				serializedObject.ApplyModifiedProperties();
				reloadClips();
			}

			GAFInspectorLine.draw(new Color(125f / 255f, 125f / 255f, 125f / 255f), 1f);

			var material = properties.FindPropertyRelative("m_Material");
			GUILayout.Space(3f);
			EditorGUILayout.LabelField("* Custom material will break dynamic batching!");
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(material, new GUIContent("Custom material: "));
			if (EditorGUI.EndChangeCheck())
			{
				serializedObject.ApplyModifiedProperties();
				reloadClips();
			}

			GAFInspectorLine.draw(new Color(125f / 255f, 125f / 255f, 125f / 255f), 1f);

			GUILayout.Space(3f);
			var useCustomTextureRectProperty = properties.FindPropertyRelative("m_UseCustomTextureRect");
			if (!useCustomTextureRectProperty.hasMultipleDifferentValues)
			{
				var useCustomTextureRect = EditorGUILayout.Foldout(useCustomTextureRectProperty.boolValue, "Custom texture rect:");
				if (useCustomTextureRect != useCustomTextureRectProperty.boolValue)
				{
					foreach (var _target in targets)
						_target.serializedProperties.useCustomTextureRect = useCustomTextureRect;

					if (!useCustomTextureRect)
						reloadClips();
				}

				if (useCustomTextureRect)
				{
					var atlasTextureRectProperty = properties.FindPropertyRelative("m_AtlasTextureRect");
					EditorGUI.BeginChangeCheck();
					drawRectProperty(atlasTextureRectProperty, "Atlas texture Rect: ");
					if (EditorGUI.EndChangeCheck())
					{
						serializedObject.ApplyModifiedProperties();
						reloadClips();
					}
				}
			}
			else
			{
				EditorGUILayout.HelpBox("Cannot edit custom texture rect for multiple targets.", MessageType.Info);
			}
		}

		private void OnEnable()
		{
			EditorApplication.update += OnUpdate;
		}

		private void OnUpdate()
		{
			var _targets = targets;
			if (_targets != null && _targets.Count > 0)
			{
				foreach (var _target in _targets)
				{
					if (_target != null && 
						_target.transform.localPosition !=	_target.serializedProperties.statePosition +
															_target.serializedProperties.offset +
															(Vector3)_target.serializedProperties.clip.settings.pivotOffset)
					{
						_target.serializedProperties.offset =	_target.transform.localPosition -
																_target.serializedProperties.statePosition -
																(Vector3)_target.serializedProperties.clip.settings.pivotOffset;
					}
				}
			}
			else
			{
				EditorApplication.update -= OnUpdate;
			}
		}

		private void reloadClips()
		{
			var clips = targets.Select(obj => obj.serializedProperties.clip).Distinct();
			foreach (var clip in clips)
				clip.reload();
		}

		private void drawVector3Property(SerializedProperty _Property, string _Label)
		{
			var x = _Property.FindPropertyRelative("x");
			var y = _Property.FindPropertyRelative("y");
			var z = _Property.FindPropertyRelative("z");

			EditorGUILayout.LabelField(_Label, GUILayout.MaxWidth(100f));

			EditorGUILayout.PropertyField(x);
			EditorGUILayout.PropertyField(y);
			EditorGUILayout.PropertyField(z);
		}

		private void drawVector2Property(SerializedProperty _Property, string _Label)
		{
			var x = _Property.FindPropertyRelative("x");
			var y = _Property.FindPropertyRelative("y");

			EditorGUILayout.LabelField(_Label, GUILayout.MaxWidth(100f));

			EditorGUILayout.PropertyField(x);
			EditorGUILayout.PropertyField(y);
		}

		private void drawRectProperty(SerializedProperty _Property, string _Label)
		{
			var x		= _Property.FindPropertyRelative("x");
			var y		= _Property.FindPropertyRelative("y");
			var width	= _Property.FindPropertyRelative("width");
			var height	= _Property.FindPropertyRelative("height");

			EditorGUILayout.LabelField(_Label, GUILayout.MaxWidth(200f));

			EditorGUILayout.PropertyField(x);
			EditorGUILayout.PropertyField(y);
			EditorGUILayout.PropertyField(width);
			EditorGUILayout.PropertyField(height);
		}
	}
}
