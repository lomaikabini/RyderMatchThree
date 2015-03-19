/*
 * File:			GAFBaseMovieClipEditor.cs
 * Version:			1.0
 * Last changed:	2014/12/15 8:17
 * Author:			Alexey_Nikitin
 * Copyright:		© GAF Media
 * Project:			UnityVS.UnityProject.CSharp.Editor
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

using GAF.Assets;
using GAF.Core;
using GAFEditor.Tracking;
using GAFEditor.Assets;

namespace GAFEditor.Core
{
	public class GAFBaseMovieClipEditor : Editor
	{
		new public List<GAFBaseMovieClip> targets
		{
			get
			{
				return base.targets.ToList().ConvertAll<GAFBaseMovieClip>(target => (GAFBaseMovieClip)target);
			}
		}

		new public GAFBaseMovieClip target
		{
			get
			{
				return base.target as GAFBaseMovieClip;
			}
		}

		protected int m_TimelineIndex	= 0;
		protected int m_TimelineID		= 0;

		protected bool m_ShowResourceManagement = true;
		protected bool m_ShowPlaceholder		= true;
		protected bool m_ShowSettings			= true;
		protected bool m_ShowPlayback			= true;


		protected virtual void drawAsset()
		{
			var initializedProperty = serializedObject.FindProperty("m_IsInitialized");
			var assetProperty = serializedObject.FindProperty("m_GAFAsset");

			if (!initializedProperty.hasMultipleDifferentValues)
			{
				if (!initializedProperty.boolValue)
				{
					if (hasPrefabs())
					{
						GUILayout.Space(10f);
						EditorGUILayout.BeginVertical(EditorStyles.textField);
						{
							EditorGUILayout.HelpBox("Cannot init movie clip in prefab!", MessageType.Warning);
						}
						EditorGUILayout.EndVertical();
					}
					else
					{
						GUILayout.Space(10f);
						EditorGUILayout.PropertyField(assetProperty, new GUIContent("Asset:"));

						if (assetProperty.objectReferenceValue != null &&
							!assetProperty.hasMultipleDifferentValues)
						{
							var asset = (GAFAnimationAsset)assetProperty.objectReferenceValue;
							if (!asset.isLoaded)
							{
								drawAssetIsNotLoaded(asset);
							}
							else
							{
								drawChooseTimeline(asset);
								drawInitMovieClipButton(asset);
							}
						}
					}
				}
				else
				{
					if (!assetProperty.hasMultipleDifferentValues)
					{
						var asset = (GAFAnimationAsset)assetProperty.objectReferenceValue;
						if (asset != null)
						{
							if (!asset.isLoaded)
							{
								drawAssetIsNotLoaded(asset);
							}
							else
							{
								GUILayout.Space(10f);
								var newAsset = EditorGUILayout.ObjectField("Asset: ", asset, typeof(GAFAnimationAsset), false) as GAFAnimationAsset;
								if (newAsset != asset)
								{
									foreach (var _target in targets)
										_target.clear(true);

									if (newAsset != null && newAsset.isLoaded)
									{
										foreach (var _target in targets)
										{
											_target.initialize(newAsset, _target.timelineID);
											_target.reload();
										}
									}
								}
							}
						}
						else
						{
							GUILayout.Space(10f);
							EditorGUILayout.BeginVertical(EditorStyles.textField);
							{
								EditorGUILayout.LabelField("Asset is not found!", EditorStyles.boldLabel);
							}
							EditorGUILayout.EndVertical();
						}
					}
					else
					{
						GUILayout.Space(10f);
						EditorGUILayout.BeginVertical(EditorStyles.textField);
						{
							EditorGUILayout.HelpBox("Multiple assets...", MessageType.Info);
						}
						EditorGUILayout.EndVertical();
					}
				}
			}
			else
			{
				GUILayout.Space(10f);
				EditorGUILayout.BeginVertical(EditorStyles.textField);
				{
					EditorGUILayout.HelpBox("Different clip states...", MessageType.Info);
				}
				EditorGUILayout.EndVertical();
			}
		}

		protected virtual void drawResourcesState()
		{
			int initializedCount = targets.Where(clip => clip.isInitialized).Count();
			if (initializedCount == targets.Count)
			{
				int validResourcesCount = targets.Where(clip => clip.resource != null && clip.resource.isValid && clip.resource.isReady).Count();
				if (validResourcesCount == 0)
				{
					drawResourcesMissing();
				}
				else if (validResourcesCount != targets.Count)
				{
					drawDifferentResourcesState();
				}
				else
				{
					drawCorrectResourcesState();
				}
			}
		}

		protected virtual void drawSettings()
		{
		}

		protected virtual void drawPlayback()
		{
			
		}

		protected virtual void drawDataButtons()
		{
			var initializedProperty = serializedObject.FindProperty("m_IsInitialized");
			var assetProperty = serializedObject.FindProperty("m_GAFAsset");

			if (!initializedProperty.hasMultipleDifferentValues &&
				 initializedProperty.boolValue &&
				!assetProperty.hasMultipleDifferentValues &&
				 assetProperty.objectReferenceValue != null)
			{
				var asset = (GAFAnimationAsset)assetProperty.objectReferenceValue;
				if (asset != null && asset.isLoaded)
				{
					GUILayout.Space(7f);
					drawBuildResources(asset);
					drawReloadAnimationButton();
				}
			}

            drawClearButtons();
		}


		protected virtual void drawAssetIsNotLoaded(GAFAnimationAsset _Asset)
		{
			GUILayout.Space(3f);
			EditorGUILayout.BeginVertical(EditorStyles.textField);
			{
				EditorGUILayout.LabelField("Asset '" + _Asset.name + "' is not loaded properly! Try to reimport .GAF file!");
			}
			EditorGUILayout.EndVertical();
		}

		protected virtual void drawResourcesMissing()
		{
			GUILayout.Space(3f);
			EditorGUILayout.BeginVertical(EditorStyles.textField);
			{
				EditorGUILayout.HelpBox("Your animation(s) missing resources! \nImport necessary textures OR Build resources OR Ensure your custom delegate works!", MessageType.Warning);
			}
			EditorGUILayout.EndVertical();
		}

		protected virtual void drawDifferentResourcesState()
		{
			GUILayout.Space(3f);
			EditorGUILayout.BeginVertical(EditorStyles.textField);
			{
				EditorGUILayout.HelpBox("Some of selected movie clips misses resource!", MessageType.Warning);
			}
			EditorGUILayout.EndVertical();
		}

		protected virtual void drawCorrectResourcesState()
		{
			GUILayout.Space(3f);
			EditorGUILayout.BeginVertical(EditorStyles.textField);
			{
				EditorGUILayout.HelpBox("Resources are available!", MessageType.Info);
			}
			EditorGUILayout.EndVertical();
		}

		protected virtual void drawBuildResources(GAFAnimationAsset _Asset)
		{
			GUILayout.Space(3f);

			if (GUILayout.Button("Build resources"))
			{
				GAFResourceManager.createResources(_Asset);

				foreach (var _target in targets)
					_target.reload();
			}
		}

		protected virtual void drawChooseTimeline(GAFAnimationAsset _Asset)
		{
			if (_Asset.getTimelines().Count > 1)
			{
				GUILayout.Space(6f);
				EditorGUILayout.BeginVertical(EditorStyles.textField);
				{
					EditorGUILayout.LabelField("Choose timeline ID:");
					EditorGUILayout.BeginHorizontal();
					{
						var timelineIDs = _Asset.getTimelines().ConvertAll(timeline => timeline.id.ToString() + (timeline.linkageName.Length > 0 ? " - " + timeline.linkageName : "")).ToArray();
						var index = GUILayout.SelectionGrid(m_TimelineIndex, timelineIDs, timelineIDs.Length < 4 ? timelineIDs.Length : 4);
						if (index != m_TimelineIndex)
						{
							m_TimelineIndex = index;
							var timeline = timelineIDs[index];
							m_TimelineID = timeline.IndexOf(" - ") > 0 ? int.Parse(timeline.Split('-')[0]) : int.Parse(timeline);
						}
					}
					EditorGUILayout.EndHorizontal();
				}
				EditorGUILayout.EndVertical();
			}
			else
			{
				GUILayout.Space(6f);
				EditorGUILayout.BeginVertical(EditorStyles.textField);
				{
					EditorGUILayout.LabelField("Timeline ID: 0 - rootTimeline");
					m_TimelineID = 0;
					m_TimelineIndex = 0;
				}
				EditorGUILayout.EndVertical();
			}
		}

		protected virtual void drawInitMovieClipButton(GAFAnimationAsset _Asset)
		{
			GUILayout.Space(3f);
			if (GUILayout.Button("Create GAF movie clip"))
			{
				foreach (var target in targets)
				{
					target.initialize(_Asset, m_TimelineID);
					target.reload();

					GAFTracking.sendMovieClipCreatedRequest(_Asset.name);
				}
			}
		}

		protected virtual void drawSortingID()
		{
			var settingProperty = serializedObject.FindProperty("m_Settings");
			var sortinglayerIDProperty = settingProperty.FindPropertyRelative("m_SpriteLayerID");
			var sortinglayerNameProperty = settingProperty.FindPropertyRelative("m_SpriteLayerName");

			List<string> layerNames = getSortingLayerNames().ToList();
			List<int> layerID = getSortingLayerUniqueIDs().ToList();

			var index = -1;
			if (sortinglayerIDProperty.hasMultipleDifferentValues)
				layerNames.Insert(0, "—");
			else
				index = layerID.FindIndex(__id => __id == sortinglayerIDProperty.intValue);

			if (index < 0)
				index = layerID.FindIndex(__id => __id == 0);

			EditorGUILayout.BeginHorizontal();
			{
				var style = sortinglayerIDProperty.isInstantiatedPrefab && sortinglayerIDProperty.prefabOverride ? EditorStyles.boldLabel : EditorStyles.label;
				EditorGUILayout.LabelField(new GUIContent("Sorting layer: ", "The layer used to define this animation’s overlay priority during rendering.​"), style);
				var nextIndex = EditorGUILayout.Popup(index, layerNames.ToArray());
				if (index != nextIndex)
				{
					var newIndex = sortinglayerIDProperty.hasMultipleDifferentValues ? nextIndex - 1 : nextIndex;
					sortinglayerIDProperty.intValue = layerID[newIndex];
					sortinglayerNameProperty.stringValue = layerNames[newIndex];
					serializedObject.ApplyModifiedProperties();
					reloadTargets();
				}
			}
			EditorGUILayout.EndHorizontal();
		}

		protected virtual void drawScales(SerializedProperty _AssetProperty)
		{
			var asset = (GAFAnimationAsset)_AssetProperty.objectReferenceValue;
			if (asset != null && asset.isLoaded)
			{
				var settingProperty = serializedObject.FindProperty("m_Settings");

				var scaleProperty = settingProperty.FindPropertyRelative("m_Scale");
				var scales = asset.scales.ConvertAll(__scale => __scale.ToString());

				var index = 0;
				if (scaleProperty.hasMultipleDifferentValues)
					scales.Insert(0, "—");
				else
					index = asset.scales.FindIndex(__scale => __scale == scaleProperty.floatValue);

				GUILayout.Space(3f);
				EditorGUILayout.BeginHorizontal();
				{
					var style = scaleProperty.isInstantiatedPrefab && scaleProperty.prefabOverride ? EditorStyles.boldLabel : EditorStyles.label;
					EditorGUILayout.LabelField(new GUIContent("Texture atlas scale: ", "Ability to change animation’s scale if you convert your animation with at least two scales. [float value]​"), style);
					var nextIndex = EditorGUILayout.Popup(index, scales.ToArray());
					if (index != nextIndex)
					{
						scaleProperty.floatValue = asset.scales[scaleProperty.hasMultipleDifferentValues ? nextIndex - 1 : nextIndex];
						serializedObject.ApplyModifiedProperties();
						reloadTargets();
					}
				}
				EditorGUILayout.EndHorizontal();
			}
		}

		protected virtual void drawCsfs(SerializedProperty _AssetProperty)
		{
			var asset = (GAFAnimationAsset)_AssetProperty.objectReferenceValue;
			if (asset != null && asset.isLoaded)
			{
				var settingProperty = serializedObject.FindProperty("m_Settings");

				var csfProperty = settingProperty.FindPropertyRelative("m_CSF");
				var csfs = asset.csfs.ConvertAll(__csf => __csf.ToString());

				var index = 0;
				if (csfProperty.hasMultipleDifferentValues)
					csfs.Insert(0, "—");
				else
					index = asset.csfs.FindIndex(__csf => __csf == csfProperty.floatValue);

				GUILayout.Space(3f);
				EditorGUILayout.BeginHorizontal();
				{
					var style = csfProperty.isInstantiatedPrefab && csfProperty.prefabOverride ? EditorStyles.boldLabel : EditorStyles.label;
					EditorGUILayout.LabelField(new GUIContent("Content scale factor: ", "Ability to use bigger textures in the same mesh if you convert your animation with two scale factors (for example 1 and 2 for non retina and retina). [integer value]​"), style);
					var nextIndex = EditorGUILayout.Popup(index, csfs.ToArray());
					if (index != nextIndex)
					{
						csfProperty.floatValue = asset.csfs[csfProperty.hasMultipleDifferentValues ? nextIndex - 1 : nextIndex];
						serializedObject.ApplyModifiedProperties();
						reloadTargets();
					}
				}
				EditorGUILayout.EndHorizontal();
			}
		}

		protected virtual void drawReloadAnimationButton()
		{
			GUILayout.Space(3f);
			if (GUILayout.Button("Reload animation"))
			{
				reloadTargets();
			}
		}

		protected virtual void drawClearButtons()
		{
			GUILayout.Space(3f);
			EditorGUILayout.BeginHorizontal();
			{
				if (GUILayout.Button("Clear animation"))
				{
					foreach (var target in targets)
					{
						if (target.isInitialized && PrefabUtility.GetPrefabType(target.gameObject) != PrefabType.Prefab)
						{
							target.clear(false);
							clearObjectManagerLists(target);
							EditorUtility.SetDirty(target);
						}
					}

					m_TimelineIndex = 0;
					m_TimelineID = 0;
				}

				if (GUILayout.Button("Clear animation (delete children)"))
				{
					foreach (var target in targets)
					{
						if (target.isInitialized && PrefabUtility.GetPrefabType(target.gameObject) != PrefabType.Prefab)
						{
							target.clear(true);
							clearObjectManagerLists(target);
							EditorUtility.SetDirty(target);
						}
					}

					m_TimelineIndex = 0;
					m_TimelineID = 0;
				}
			}
			EditorGUILayout.EndHorizontal();
		}

		protected virtual void drawProperty(SerializedProperty _Property, GUIContent _Content)
		{
			EditorGUI.showMixedValue = _Property.hasMultipleDifferentValues;
			EditorGUILayout.BeginHorizontal();
			{
				var style = _Property.isInstantiatedPrefab && _Property.prefabOverride ? EditorStyles.boldLabel : EditorStyles.label;
				EditorGUILayout.LabelField(_Content, style);
				EditorGUILayout.PropertyField(_Property, new GUIContent(""));
			}
			EditorGUILayout.EndHorizontal();
			EditorGUI.showMixedValue = false;
		}

		protected virtual void clearObjectManagerLists(GAFBaseMovieClip clip)
		{
			var objectManagers = (GAFObjectsManagerEditor[])Resources.FindObjectsOfTypeAll<GAFObjectsManagerEditor>();

			if (objectManagers != null && objectManagers.Length > 0)
			{
				for (int i = 0; i < objectManagers.Length; i++)
				{
					if (clip.manager == objectManagers[i].target)
					{
						objectManagers[i].clearLists();
						break;
					}
				}
			}
		}

		protected virtual void reloadTargets()
		{
			foreach (var target in targets)
			{
				if (target.isInitialized && PrefabUtility.GetPrefabType(target.gameObject) != PrefabType.Prefab)
				{
					target.reload();
					EditorUtility.SetDirty(target);
				}
			}
		}

		protected virtual bool hasPrefabs()
		{
			bool hasPrefabs = false;
			foreach (var target in targets)
			{
				if (PrefabUtility.GetPrefabType(target.gameObject) == PrefabType.Prefab)
				{
					hasPrefabs = true;
					break;
				}
			}

			return hasPrefabs;
		}

		protected virtual List<string> getSortingLayerNames()
		{
			System.Type internalEditorUtilityType = typeof(InternalEditorUtility);
			PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
			return ((string[])sortingLayersProperty.GetValue(null, new object[0])).ToList();
		}

		protected virtual List<int> getSortingLayerUniqueIDs()
		{
			System.Type internalEditorUtilityType = typeof(InternalEditorUtility);
			PropertyInfo sortingLayerUniqueIDsProperty = internalEditorUtilityType.GetProperty("sortingLayerUniqueIDs", BindingFlags.Static | BindingFlags.NonPublic);
			return ((int[])sortingLayerUniqueIDsProperty.GetValue(null, new object[0])).ToList();
		}
	}
}