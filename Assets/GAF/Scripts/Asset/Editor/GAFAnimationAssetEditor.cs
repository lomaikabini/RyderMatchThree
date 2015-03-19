/*
 * File:			GAFAnimationAssetEditor.cs
 * Version:			1.0
 * Last changed:	2014/12/3 12:04
 * Author:			Alexey_Nikitin
 * Copyright:		© GAF Media
 * Project:			UnityVS.UnityProject.CSharp.Editor
 */

using UnityEngine;
using UnityEditor;

using System.Linq;
using System.IO;
using System.Collections.Generic;

using GAF.Core;
using GAF.Assets;

using GAFEditor;
using GAFEditor.ExternalEditor;

namespace GAFEditor.Assets
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(GAFAnimationAsset))]
    public class GAFAnimationAssetEditor : Editor
    {
        private bool m_MovieClipFoldout = true;
		private bool m_AnimatorFoldout = true;

        new public GAFAnimationAsset target
        {
            get
            {
                return base.target as GAFAnimationAsset;
            }
        }

        new public List<GAFAnimationAsset> targets
        {
            get
            {
                return base.targets.ToList().ConvertAll(_target => _target as GAFAnimationAsset);
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var nameProperty = serializedObject.FindProperty("m_Name");

            GUILayout.Space(5f);
            EditorGUILayout.LabelField("Asset name: " + (!nameProperty.hasMultipleDifferentValues ? nameProperty.stringValue : "—"), EditorStyles.boldLabel);

            var loadedCount = targets.Where(_target => _target.isLoaded).Count();
            if (loadedCount == targets.Count)
            {
                if (loadedCount == 1)
                {
                    drawAssetData();

                    GUILayout.Space(5f);
                    drawTimelines();

					GUILayout.Space(5f);
					drawCreateClip(false);

					GUILayout.Space(5f);
					drawCreateClip(true);

                    GUILayout.Space(10f);
                    drawAnimationColor();

                    GUILayout.Space(10f);
                    drawResources();
                }
                else
                {
                    GUILayout.Space(5f);
                    EditorGUILayout.HelpBox("Cannot view multiple asset settings.", MessageType.Info);

					GUILayout.Space(5f);
					drawCreateClip(false);

					GUILayout.Space(5f);
					drawCreateClip(true);
                }
            }
            else if (loadedCount == 0)
            {
                GUILayout.Space(5f);
                EditorGUILayout.HelpBox("Asset(s) is(are) not loaded! Please reload asset(s) or reimport '.gaf' file.", MessageType.Warning);
            }
            else
            {
                GUILayout.Space(5f);
                EditorGUILayout.HelpBox("Some of assets are not loaded! Please reload asset(s) or reimport '.gaf' file.", MessageType.Warning);
            }

            GUI.enabled = loadedCount == targets.Count;
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Delete resources"))
                {
                    foreach (var _target in targets)
                        GAFResourceManager.deleteResources(_target);
                }

                if (GUILayout.Button("Rebuild resources"))
                {
                    foreach (var _target in targets)
                        GAFResourceManager.createResources(_target);
                }
            }
            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;
        }

        private void drawResources()
        {
            EditorGUILayout.LabelField("Resources: ");
            foreach (var resourcePath in target.resourcesPaths)
            {
                EditorGUILayout.BeginVertical(EditorStyles.textField);
                {
                    EditorGUILayout.LabelField(resourcePath);
                    var resource = AssetDatabase.LoadAssetAtPath(resourcePath, typeof(GAFTexturesResource)) as GAFTexturesResource;
                    if (resource != null)
                    {
                        var textures = resource.data.Select(data => data.sharedTexture).ToList();
                        var materials = resource.data.Select(data => data.sharedMaterial).ToList();
                        var invalidData = resource.data.Where(data => !data.isValid).Select(data => data.name).ToList();

                        if (textures.Count > 0)
                        {
                            GUILayout.Space(3f);
                            EditorGUILayout.LabelField("Found textures: ");
                            for (int index = 0; index < textures.Count; ++index)
                            {
                                string path = AssetDatabase.GetAssetPath(textures[index]);

                                EditorGUILayout.LabelField("\t" + (index + 1).ToString() + ". " + path);
                            }
                        }

                        if (invalidData.Count > 0)
                        {
                            GUILayout.Space(3f);
                            EditorGUILayout.LabelField("Missing textures: ");
                            for (int index = 0; index < invalidData.Count; ++index)
                            {
                                EditorGUILayout.LabelField("\t" + (index + 1).ToString() + ". " + invalidData[index]);
                            }
                        }

                        if (materials.Count > 0)
                        {
                            GUILayout.Space(3f);
                            EditorGUILayout.LabelField("Shared materials: ");
                            for (int index = 0; index < materials.Count; ++index)
                            {
                                string path = AssetDatabase.GetAssetPath(materials[index]);

                                EditorGUILayout.LabelField("\t" + (index + 1).ToString() + ". " + path);
                            }
                        }
                    }
                }
                EditorGUILayout.EndVertical();
            }
        }

        private void drawTimelines()
        {
            EditorGUILayout.LabelField("Timelines: ");
            foreach (var timeline in target.getTimelines())
            {
                EditorGUILayout.BeginVertical(EditorStyles.textField);
                {
                    GUILayout.Space(5f);
                    EditorGUILayout.LabelField("ID - " + timeline.id.ToString());
                    EditorGUILayout.LabelField("Linkage name - " + timeline.linkageName);

                    GUILayout.Space(5f);
                    EditorGUILayout.LabelField("Frame size: " + timeline.frameSize.ToString());
                    EditorGUILayout.LabelField("Pivot: " + timeline.pivot.ToString());
                    EditorGUILayout.LabelField("Frames count: " + timeline.framesCount.ToString());

                    GUILayout.Space(5f);
                    EditorGUILayout.LabelField("Available sequences: " + string.Join(",", timeline.sequences.ConvertAll(sequence => sequence.name).ToArray()));

                    GUILayout.Space(5f);
                    EditorGUILayout.LabelField("Objects count: " + timeline.objects.Count.ToString());
                    EditorGUILayout.LabelField("Masks count: " + timeline.masks.Count.ToString());
                }
                EditorGUILayout.EndVertical();
            }
        }

        private void drawCreateClip(bool _WithAnimator)
        {
			var foldout = false;
			var guiStyle = new GUIStyle(EditorStyles.foldout);
			guiStyle.fontSize = 12;
			guiStyle.fontStyle = FontStyle.Bold;

			if (!_WithAnimator)
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(5f);
				m_MovieClipFoldout = EditorGUILayout.Foldout(m_MovieClipFoldout, new GUIContent("Simple movie clip"), guiStyle);
				foldout = m_MovieClipFoldout;
				EditorGUILayout.EndHorizontal();
			}

			else
			{
				var buttonStyle = new GUIStyle();
				var foldoutText = new GUIContent("Clip with animator");
				var foldoutSize = buttonStyle.CalcSize(foldoutText).x + 25f;

				var rect = EditorGUILayout.BeginVertical();
				{
					var texture = GAFEditor.Skin.GAFStyles.instance.textures["gafProFeature"];

					if (GUI.Button(new Rect(rect.x + foldoutSize + 5f, rect.y + texture.height / 2f, texture.width, texture.height), texture, buttonStyle))
					{
						Application.OpenURL(GAFEditor.Interactive.GAFServerConnection.urlGAFPro);

						Tracking.GAFTracking.sendGAFProADPressed();
					}

					EditorGUILayout.BeginHorizontal();
					{
						GUILayout.Space(5f);
						m_AnimatorFoldout = EditorGUILayout.Foldout(m_AnimatorFoldout, foldoutText, guiStyle);
					}
					EditorGUILayout.EndHorizontal();

					foldout = m_AnimatorFoldout;
				}
				EditorGUILayout.EndVertical();
			}

			if (foldout)
			{
				GUI.enabled = !_WithAnimator;

				var rect = EditorGUILayout.BeginVertical();
				{
					EditorGUILayout.BeginHorizontal();
					{
						if (GUILayout.Button("Add to scene"))
						{
							foreach (var _target in targets)
								addToScene(_target);
						}

						GUILayout.Space(5f);
						if (GUILayout.Button("Create prefab"))
						{
							foreach (var _target in targets)
								createPrefab(_target);
						}

						GUILayout.Space(5f);
						if (GUILayout.Button("Prefab+instance"))
						{
							foreach (var _target in targets)
								createPrefabPlusInstance(_target);
						}
					}
					EditorGUILayout.EndHorizontal();

					GUI.enabled = true;

					if (_WithAnimator)
						if (GUI.Button(new Rect(rect.x, rect.y, rect.width, rect.height), "", new GUIStyle(EditorStyles.label)))
						{
							Application.OpenURL(GAFEditor.Interactive.GAFServerConnection.urlGAFPro);

							Tracking.GAFTracking.sendGAFProADPressed();
						}
				}
				EditorGUILayout.EndVertical();
			}
        }

        private void drawAnimationColor()
        {
            EditorGUILayout.BeginVertical();
            {
                var color = serializedObject.FindProperty("m_AnimationColor");

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(color, new GUIContent("Animation color: "));
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();

                    foreach (var _target in targets)
                    {
                        foreach (var resourcePath in _target.resourcesPaths)
                        {
                            var resource = AssetDatabase.LoadAssetAtPath(resourcePath, typeof(GAFTexturesResource)) as GAFTexturesResource;
                            var materials = resource.data.Select((item) => item.sharedMaterial).ToList();

                            foreach (var material in materials)
                            {
                                material.SetColor("_CustomColor", color.colorValue);
                            }
                        }
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void drawAssetData()
        {
            EditorGUILayout.BeginVertical(EditorStyles.textField);
            {
                GUILayout.Space(2f);
                EditorGUILayout.LabelField("GAF version: " + target.majorDataVersion.ToString() + "." + target.minorDataVersion.ToString());
                EditorGUILayout.LabelField("Asset version: " + target.assetVersion.ToString());

                GUILayout.Space(5f);
                EditorGUILayout.LabelField("Available atlas scales: " + string.Join(",", target.scales.ConvertAll(scale => scale.ToString()).ToArray()));
                EditorGUILayout.LabelField("Available content scale factors: " + string.Join(",", target.csfs.ConvertAll(csf => csf.ToString()).ToArray()));
            }
            EditorGUILayout.EndVertical();
        }

        private void addToScene(GAFAnimationAsset _Asset)
        {
            createClip(_Asset);
        }

        private void createPrefab(GAFAnimationAsset _Asset)
        {
            var path = AssetDatabase.GetAssetPath(target);
            path = path.Substring(0, path.Length - name.Length - ".asset".Length);

            var prefabPath = path + name + ".prefab";
            var existingPrefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
            if (existingPrefab == null)
            {
                var clipObject = createClip(_Asset);
                var prefab = PrefabUtility.CreateEmptyPrefab(prefabPath);
                prefab = PrefabUtility.ReplacePrefab(clipObject, prefab, ReplacePrefabOptions.ConnectToPrefab);
                GameObject.DestroyImmediate(clipObject);
            }
        }

        private void createPrefabPlusInstance(GAFAnimationAsset _Asset)
        {
            var path = AssetDatabase.GetAssetPath(target);
            path = path.Substring(0, path.Length - name.Length - ".asset".Length);

            var prefabPath = path + name + ".prefab";
            var existingPrefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
            if (existingPrefab == null)
            {
                var clipObject = createClip(_Asset);
                var prefab = PrefabUtility.CreateEmptyPrefab(prefabPath);
                prefab = PrefabUtility.ReplacePrefab(clipObject, prefab, ReplacePrefabOptions.ConnectToPrefab);
            }
            else
            {
                PrefabUtility.InstantiatePrefab(existingPrefab);
            }
        }

        private GameObject createClip(GAFAnimationAsset _Asset)
        {
            var clipObject = new GameObject(_Asset.name);

            var clip = clipObject.AddComponent<GAFMovieClip>();
            clip.initialize(_Asset);
            clip.reload();

            return clipObject;
        }
    }
}