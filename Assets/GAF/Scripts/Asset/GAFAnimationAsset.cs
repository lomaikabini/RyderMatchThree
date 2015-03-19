/*
 * File:			GAFAnimationAsset.cs
 * Version:			1.0
 * Last changed:	2014/12/11 11:05
 * Author:			Alexey_Nikitin
 * Copyright:		© GAF Media
 * Project:			UnityVS.UnityProject.CSharp
 */

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using LegacySystem.IO;

using GAF.Utils;
using GAF.Data;
using GAF.Reader;

namespace GAF.Assets
{
	[System.Serializable]
	public class GAFAnimationAsset : ScriptableObject
	{
		#region Members

		[HideInInspector][SerializeField]private int		m_AssetVersion	= 0;
		[HideInInspector][SerializeField]private string		m_GUID			= string.Empty;
		[HideInInspector][SerializeField]private byte[]		m_AssetData		= null;

		[HideInInspector][SerializeField]private bool						m_IsExternalDataCollected	= false;
		[HideInInspector][SerializeField]private List<GAFAssetExternalData>	m_ExternalData				= new List<GAFAssetExternalData>();

		[HideInInspector][SerializeField]private Color m_AnimationColor = new Color(1, 1, 1, 1);

		private Dictionary<KeyValuePair<float, float>, GAFTexturesResource> m_LoadedResources = new Dictionary<KeyValuePair<float, float>, GAFTexturesResource>();

		private GAFAnimationData m_SharedData = null;

		private Object m_Locker = new Object();

		#endregion // Members

		#region Interface

		public void initialize(byte[] _GAFData, string _GUID)
		{
			m_AssetData		= _GAFData;
			m_SharedData	= null;
			m_GUID			= _GUID;

			m_AssetVersion = GAFSystem.AssetVersion;

			load();
		}

		public void resetGUID(string _GUID)
		{
			m_GUID = _GUID;
		}

		public void load()
		{
			lock (m_Locker)
			{
#if UNITY_EDITOR
				if (m_AssetVersion < GAFSystem.AssetVersion &&
					!EditorApplication.isPlayingOrWillChangePlaymode)
				{
					upgrade();
				}
#endif // UNITY_EDITOR

				if (m_AssetVersion == GAFSystem.AssetVersion)
				{
					if (!isLoaded &&
						 m_AssetData != null)
					{
						GAFReader reader = new GAFReader();
						try
						{
							reader.Load(m_AssetData, ref m_SharedData);
						}
						catch (GAFException _Exception)
						{
							GAFUtils.Error(_Exception.Message);

							m_SharedData = null;
						}

						if (isLoaded &&
							!m_IsExternalDataCollected)
						{
							collectExternalData();

#if UNITY_EDITOR
							if (!EditorApplication.isPlayingOrWillChangePlaymode)
								EditorUtility.SetDirty(this);
#endif // UNITY_EDITOR
						}
					}
				}
				else
				{
					GAFUtils.Log("Asset \"" + name + "\" was not upgraged!", string.Empty);
				}
			}
		}

		public GAFTexturesResource getResource(float _Scale, float _CSF)
		{
			GAFTexturesResource resource = null;

			var key = new KeyValuePair<float, float>(_Scale, _CSF);
			if (m_LoadedResources.ContainsKey(key) &&
				m_LoadedResources[key] != null)
			{
				resource = m_LoadedResources[key];
			}
			else
			{
				string resourcePath = "Cache/" + getResourceName(_Scale, _CSF);
				resource = Resources.Load<GAFTexturesResource>(resourcePath);
				if (resource != null)
					m_LoadedResources[key] = resource;
			}

			return resource;
		}

		public string getResourceName(float _Scale, float _CSF)
		{
			return "[" + name + "]" + m_GUID + "_" + _Scale.ToString() + "_" + _CSF.ToString();
		}

		public List<GAFTimelineData> getTimelines()
		{
			return isLoaded ? m_SharedData.timelines.Values.ToList() : null;
		}

		public List<GAFAtlasData> getAtlases(int _TimelineID)
		{
			if (isLoaded &&
				m_SharedData.timelines.ContainsKey(_TimelineID))
			{
				return m_SharedData.timelines[_TimelineID].atlases;
			}
			else
			{
				return null;
			}
		}

		public List<GAFObjectData> getObjects(int _TimelineID)
		{
			if (isLoaded &&
				m_SharedData.timelines.ContainsKey(_TimelineID))
			{
				return m_SharedData.timelines[_TimelineID].objects;
			}
			else
			{
				return null;
			}
		}

		public List<GAFObjectData> getMasks(int _TimelineID)
		{
			if (isLoaded &&
				m_SharedData.timelines.ContainsKey(_TimelineID))
			{
				return m_SharedData.timelines[_TimelineID].masks;
			}
			else
			{
				return null;
			}
		}

		public Dictionary<uint, GAFFrameData> getFrames(int _TimelineID)
		{
			if (isLoaded &&
				m_SharedData.timelines.ContainsKey(_TimelineID))
			{
				return m_SharedData.timelines[_TimelineID].frames;
			}
			else
			{
				return null;
			}
		}

		public List<GAFSequenceData> getSequences(int _TimelineID)
		{
			if (isLoaded &&
				m_SharedData.timelines.ContainsKey(_TimelineID))
			{
				return m_SharedData.timelines[_TimelineID].sequences;
			}
			else
			{
				return null;
			}
		}

		public List<string> getSequenceIDs(int _TimelineID)
		{
			if (isLoaded &&
				m_SharedData.timelines.ContainsKey(_TimelineID))
			{
				return m_SharedData.timelines[_TimelineID].sequences.Select(sequence => sequence.name).ToList();
			}
			else
			{
				return null;
			}
		}

		public List<GAFNamedPartData> getNamedParts(int _TimelineID)
		{
			if (isLoaded &&
				m_SharedData.timelines.ContainsKey(_TimelineID))
			{
				return m_SharedData.timelines[_TimelineID].namedParts;
			}
			else
			{
				return null;
			}
		}

		public uint getFramesCount(int _TimelineID)
		{
			if (isLoaded &&
				m_SharedData.timelines.ContainsKey(_TimelineID))
			{
				return m_SharedData.timelines[_TimelineID].framesCount;
			}
			else
			{
				return (uint)0;
			}
		}

		public Rect getFrameSize(int _TimelineID)
		{
			if (isLoaded &&
				m_SharedData.timelines.ContainsKey(_TimelineID))
			{
				return m_SharedData.timelines[_TimelineID].frameSize;
			}
			else
			{
				return new Rect(0, 0, 0, 0);
			}
		}

		public Vector2 getPivot(int _TimelineID)
		{
			if (isLoaded &&
				m_SharedData.timelines.ContainsKey(_TimelineID))
			{
				return m_SharedData.timelines[_TimelineID].pivot;
			}
			else
			{
				return Vector2.zero;
			}
		}

		public GAFAssetExternalData getExternalData(int _TimelineID)
		{
			GAFAssetExternalData externalData = null;
			if (m_IsExternalDataCollected)
			{
				externalData = m_ExternalData.Find(data => data.timelineID == _TimelineID);
			}

			return externalData;
		}

		#endregion // Interface

		#region Properties

		public bool isLoaded
		{
			get
			{
				return m_SharedData != null;
			}
		}

		public bool isResourcesAvailable
		{
			get
			{
				string assetsPath = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length);
				foreach (var path in resourcesPaths)
				{
                    if (!LegacySystem.IO.WinRTFile.Exists(assetsPath + path))
					{
						return false;
					}
				}

				return true;
			}
		}

		public int assetVersion
		{
			get
			{
				return m_AssetVersion;
			}
		}

		public ushort majorDataVersion
		{
			get
			{
				return isLoaded ? m_SharedData.majorVersion : (ushort)0;
			}
		}

		public ushort minorDataVersion
		{
			get
			{
				return isLoaded ? m_SharedData.minorVersion : (ushort)0;
			}
		}

		public List<string> resourcesPaths
		{
			get
			{
				List<string> paths = new List<string>();
				foreach (var scale in scales)
				{
					foreach (var csf in csfs)
					{
						var resourceName = getResourceName(scale, csf) + ".asset";
						paths.Add("Assets/GAF/Resources/Cache/" + resourceName);
					}
				}
				return paths;
			}
		}

		public List<float> scales
		{
			get
			{
				return isLoaded ? m_SharedData.scales : null;
			}
		}

		public List<float> csfs
		{
			get
			{
				return isLoaded ? m_SharedData.csfs : null;
			}
		}

		public Color32 animationColor
		{
			get
			{
				return m_AnimationColor;
			}
		}

		#endregion // Properties

		#region ScriptableObject

		private void OnEnable()
		{
			load();
		}

		#endregion // ScriptableObject

		#region Implementation

#if UNITY_EDITOR
		private void upgrade()
		{
			m_AssetVersion = GAFSystem.AssetVersion;

			m_IsExternalDataCollected = false;

			EditorUtility.SetDirty(this);
		}
#endif // UNITY_EDITOR

		private void collectExternalData()
		{
			if (isLoaded &&
				!m_IsExternalDataCollected)
			{
				m_IsExternalDataCollected = true;
				m_ExternalData.Clear();

				foreach (var timeline in m_SharedData.timelines.Values)
				{
					var objectTypeFlags = new List<GAF.Objects.ObjectType>();
					foreach (var obj in timeline.objects)
					{
						var objectTypeFlag = GAF.Objects.ObjectType.Simple;
						foreach (var frame in timeline.frames.Values)
						{
							if (frame.states.ContainsKey(obj.id))
							{
								var state = frame.states[obj.id];

								if (state.maskID > 0)
								{
									objectTypeFlag |= GAF.Objects.ObjectType.Masked;
								}

								if (state.filterData != null)
								{
									objectTypeFlag |= GAF.Objects.ObjectType.Filtered;
								}
							}
						}

						objectTypeFlags.Add(objectTypeFlag);
					}

					m_ExternalData.Add(new GAFAssetExternalData((int)timeline.id, objectTypeFlags));
				}
			}
		}

		#endregion // Implementation
	}
}
