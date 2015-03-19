/*
 * File:			GAFBaseMovieClip.cs
 * Version:			1.0
 * Last changed:	2014/12/11 14:16
 * Author:			Alexey_Nikitin
 * Copyright:		© GAF Media
 * Project:			UnityVS.UnityProject.CSharp
 */

using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using GAF.Assets;
using GAF.Data;
using GAF.Objects;
using GAF.Utils;


namespace GAF.Core
{
	[System.Serializable]
	[RequireComponent(typeof(GAFObjectsManager))]
	[DisallowMultipleComponent]
	public abstract class GAFBaseMovieClip : GAFBehaviour
	{
		#region Members

		[HideInInspector][SerializeField] protected GAFObjectsManager				m_ObjectsManager		= null;
		[HideInInspector][SerializeField] protected GAFAnimationAsset				m_GAFAsset				= null;
		[HideInInspector][SerializeField] protected int								m_TimelineID			= 0;
		[HideInInspector][SerializeField] protected int 							m_CurrentFrameNumber 	= 1;
		[HideInInspector][SerializeField] protected bool 							m_IsInitialized			= false;
		[HideInInspector][SerializeField] protected GAFAnimationPlayerSettings		m_Settings				= new GAFAnimationPlayerSettings();
		[HideInInspector][SerializeField] protected int 							m_SequenceIndex			= 0;

		protected bool			m_IsLoaded				= false;
		protected Material[]	m_IndividualMaterials	= null;

		#endregion

		#region Properties

		public GAFAnimationAsset asset
		{
			get
			{
				return m_GAFAsset;
			}
		}

		public int timelineID
		{
			get
			{
				return m_TimelineID;
			}
		}

		public GAFObjectsManager manager
		{
			get
			{
				if (m_ObjectsManager == null)
				{
					m_ObjectsManager = GetComponent<GAFObjectsManager>();

					if (m_ObjectsManager == null)
					{
						m_ObjectsManager = gameObject.AddComponent<GAFObjectsManager>();
					}
				}

				return m_ObjectsManager;
			}
		}

		public bool isInitialized
		{
			get
			{
				return m_IsInitialized;
			}
		}

		public GAFAnimationPlayerSettings settings
		{
			get
			{
				return m_Settings;
			}
		}

		public GAFTexturesResource resource
		{
			get;
			set;
		}

		public Material[] individualMaterials
		{
			get
			{
				return m_IndividualMaterials;
			}
			set
			{
				m_IndividualMaterials = value;
			}
		}

		#endregion // Properties

		#region Interface

		public uint getCurrentFrameNumber()
		{
			return (uint)m_CurrentFrameNumber;
		}

		public Material getSharedMaterial(string _Name)
		{
			if (settings.hasIndividualMaterial)
			{
				return m_IndividualMaterials.First((material) => material.name == _Name);
			}
			else
			{
				return resource.getSharedMaterial(_Name);
			}
		}

		public void setMaterialColor(Color _Color)
		{
			for (int i = 0; i < m_IndividualMaterials.Length; i++)
			{
				m_IndividualMaterials[i].SetColor("_CustomColor", _Color);
			}
		}

		public virtual IGAFObject getObject(uint _ID)
		{
			return manager.objectsDict.ContainsKey(_ID) ? manager.objectsDict[_ID] : null;
		}

		public virtual void clear(bool destroyChildren)
		{
			if (m_ObjectsManager != null)
				m_ObjectsManager.clear(destroyChildren);

			if (cachedFilter.sharedMesh != null)
				cachedFilter.sharedMesh.Clear();

			cachedRenderer.sharedMaterials = new Material[0];

			m_GAFAsset = null;
			resource = null;
			m_Settings = new GAFAnimationPlayerSettings();
			m_SequenceIndex = 0;
			m_CurrentFrameNumber = 1;

			m_IsInitialized = false;
		}

		public virtual void initialize(GAFAnimationAsset _Asset)
		{
			initialize(_Asset, 0, true);
		}

		public virtual void initialize(GAFAnimationAsset _Asset, int _TimelineID)
		{
			initialize(_Asset, _TimelineID, true);
		}

		public abstract void initialize(GAFAnimationAsset _Asset, int _TimelineID, bool _BakeObjects);

		public abstract void reload();

		#endregion // Interface

		#region Implementation

		protected virtual void initResource(GAFAnimationAsset _Asset)
		{
			resource = _Asset.getResource(settings.scale, settings.csf);
		}

		protected virtual List<GAFObjectStateData> getStates(uint _FrameNumber, bool _RefreshStates)
		{
			if (!_RefreshStates)
			{
				_RefreshStates = _FrameNumber < getCurrentFrameNumber();
			}

			if (_RefreshStates)
			{
				var frame = new GAFFrameData(_FrameNumber);
				var objects = asset.getObjects(timelineID);
				var frames = asset.getFrames(timelineID);

				foreach (var _obj in objects)
				{
					frame.addState(new GAFObjectStateData(_obj.id));
				}

				foreach (var _frame in frames)
				{
					if (_frame.Key > _FrameNumber)
						break;

					foreach (var _state in _frame.Value.states)
					{
						frame.states[_state.Key] = _state.Value;
					}
				}

				return frame.states.Values.ToList();
			}
			else
			{
				var frames = asset.getFrames(timelineID);
				if (_FrameNumber - getCurrentFrameNumber() == 1)
				{
					if (frames.ContainsKey(_FrameNumber))
					{
						return frames[_FrameNumber].states.Values.ToList();
					}
				}
				else
				{
					var frame = new GAFFrameData(_FrameNumber);
					foreach (var _frame in frames)
					{
						if (_frame.Key > _FrameNumber)
							break;

						if (_frame.Key < getCurrentFrameNumber())
							continue;

						foreach (var _state in _frame.Value.states)
						{
							frame.states[_state.Key] = _state.Value;
						}
					}

					return frame.states.Values.ToList();
				}

				return null;
			}
		}

		protected virtual void setIndividualMaterials()
		{
			if (settings.hasIndividualMaterial)
			{
				var materialsCount = resource.data.Count;

				m_IndividualMaterials = new Material[materialsCount];
				for (int i = 0; i < materialsCount; i++)
				{
					m_IndividualMaterials[i] = new Material(resource.data[i].sharedMaterial);
				}
			}
			else
			{
				m_IndividualMaterials = null;
			}
		}

		#endregion // Implementation

		#region MonoBehaviour

		protected void OnEnable()
		{
			reload();
		}

		#endregion // MonoBehaviour
	}
}
