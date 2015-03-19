/*
 * File:			GAFObjectsManager.cs
 * Version:			1.0
 * Last changed:	2014/12/11 15:31
 * Author:			Alexey_Nikitin
 * Copyright:		© GAF Media
 * Project:			UnityVS.UnityProject.CSharp
 */

using UnityEngine;

using System.Collections.Generic;
using System.Linq;

using GAF.Objects;
using GAF.Utils;

namespace GAF.Core
{
	[System.Serializable]
	[AddComponentMenu("")]
	[ExecuteInEditMode]
	[RequireComponent(typeof(MeshRenderer))]
	[RequireComponent(typeof(MeshFilter))]
	[DisallowMultipleComponent]
	public class GAFObjectsManager : GAFBehaviour
	{
		#region Events

		public event System.Action onWillRenderObject;
		public event System.Action onLateUpdate;

		#endregion // Events

		#region Members

		[HideInInspector][SerializeField] private GAFBaseMovieClip		m_MovieClip		= null;
		[HideInInspector][SerializeField] private List<GAFBakedObject>	m_BakedObjects	= new List<GAFBakedObject>();
		[HideInInspector][SerializeField] private List<GAFObject>		m_Objects		= new List<GAFObject>();
		[HideInInspector][SerializeField] private bool					m_OldMode		= false;

		private Dictionary<uint, IGAFObject>	m_AllObjects		= new Dictionary<uint, IGAFObject>();
		private GAFRenderProcessor				m_RenderProcessor	= null;
		 
		#endregion // Members

		#region Properties

		public IEnumerable<IGAFObject> objects
		{
			get
			{
				return oldMode ? m_Objects.Cast<IGAFObject>() : m_BakedObjects.Cast<IGAFObject>();
			}
		}

		public GAFBaseMovieClip movieClip 
		{
			get
			{
				return m_MovieClip;
			}
		}

		public bool oldMode
		{
			get
			{
				return m_OldMode;
			}
		}

		public Dictionary<uint, IGAFObject> objectsDict
		{
			get
			{
				if (m_AllObjects == null || m_AllObjects.Count == 0)
				{
					m_AllObjects = new Dictionary<uint, IGAFObject>();
					foreach (var _object in m_Objects)
					{
						m_AllObjects[_object.serializedProperties.objectID] = _object;
					}

					foreach (var _object in m_BakedObjects)
					{
						m_AllObjects[_object.serializedProperties.objectID] = _object;
					}
				}

				return m_AllObjects;
			}
		}

		#endregion // Properties

		#region Interface

		public void initialize(GAFBaseMovieClip _Player, bool _BakeObjects)
		{
			m_MovieClip = _Player;

			if (_BakeObjects)
			{
				createNewModeObjects();
			}
			else
			{
				createOldModeObjects();
			}
		}

		public void regroupInOldMode()
		{
			if (!m_OldMode)
			{
				if (m_RenderProcessor != null)
					m_RenderProcessor.clear();

				m_RenderProcessor = null;

				m_AllObjects = null;

				cachedRenderer.materials = new Material[0];
				cachedRenderer.enabled = false;

				bakedObjectsCleanUp();
				createOldModeObjects();
			}
		}

		public void regroupInNewMode()
		{
			if (m_OldMode)
			{
				if (m_RenderProcessor != null)
					m_RenderProcessor.clear();

				m_RenderProcessor = null;

				cachedRenderer.materials = new Material[0];
				cachedRenderer.enabled = true;

				m_AllObjects = null;

				objectsCleanUp();
				createNewModeObjects();
			}
		}

		public void reload()
		{
			if (!m_OldMode)
			{
				if (m_RenderProcessor == null)
					m_RenderProcessor = new GAFRenderProcessor();
				else
					m_RenderProcessor.clear();

				m_RenderProcessor.init(m_MovieClip, cachedFilter, cachedRenderer);
			}

			foreach (var obj in objectsDict.Values)
			{
				obj.reload(m_RenderProcessor);
			}
		}

		public bool hasController(uint _ID)
		{
			return oldMode ? false : m_BakedObjects.Find(baked => baked.serializedProperties.objectID == _ID).hasController();
		}

		public void addControllerToObject(uint _ID)
		{
			if (!oldMode)
			{
				m_BakedObjects.Find(obj => obj.serializedProperties.objectID == _ID).addController();
			}
		}

		public void removeControllerFromObject(uint _ID)
		{
			if (!oldMode)
			{
				m_BakedObjects.Find(obj => obj.serializedProperties.objectID == _ID).removeController();
			}
		}

		public void clear(bool _DestroyChildren)
		{
			m_OldMode = false;

			if (m_AllObjects != null)
			{
				m_AllObjects.Clear();
				m_AllObjects = null;
			}

			foreach (var obj in m_Objects)
				obj.onDestroy();
			m_Objects.Clear();

			foreach (var obj in m_BakedObjects)
				obj.onDestroy();
			m_BakedObjects.Clear();

			if (_DestroyChildren)
			{
				List<GameObject> children = new List<GameObject>();
				foreach (Transform child in cachedTransform)
					children.Add(child.gameObject);

				foreach (var child in children)
				{
					if (Application.isPlaying)
						Destroy(child);
					else
						DestroyImmediate(child, true);
				}
			}
			else
			{
				foreach (var obj in m_Objects)
				{
					if (Application.isPlaying)
						Destroy(obj);
					else
						DestroyImmediate(obj, true);
				}
			}
		}

		public void updateToFrame(List<GAF.Data.GAFObjectStateData> _States, bool _Refresh)
		{
			foreach (var state in _States)
			{
				if (objectsDict.ContainsKey(state.id))
				{
					objectsDict[state.id].updateToState(state, _Refresh);
				}
			}

			if (!m_OldMode)
			{
				if (_Refresh)
				{
					m_RenderProcessor.pushSortRequest();
					m_RenderProcessor.pushSetupRequest();
				}
				else if (_States.Count > 0)
				{
					m_RenderProcessor.pushSetupRequest();
				}

				m_RenderProcessor.process();
			}
		}

		#endregion // Interface

		#region MonoBehaviour

		private void OnWillRenderObject()
		{
			if (onWillRenderObject != null)
				onWillRenderObject();
		}

		private void LateUpdate()
		{
			if (onLateUpdate != null)
				onLateUpdate();
		}

		#endregion // MonoBehaviour

		#region Implementation
		private void createNewModeObjects()
		{
			m_BakedObjects = new List<GAFBakedObject>();

			var objects = movieClip.asset.getObjects(movieClip.timelineID);
			var masks = movieClip.asset.getMasks(movieClip.timelineID);

			for (int i = 0; i < objects.Count; ++i)
			{
				var _objectData = objects[i];
				var _name		= getObjectName(_objectData);
				var _type		= movieClip.asset.getExternalData(movieClip.timelineID).objectTypeFlags[i];

				m_BakedObjects.Add(createBakedObject(_name, _type, _objectData));
			}

			if (masks != null)
			{
				for (int i = 0; i < masks.Count; i++)
				{
					var _maskData	= masks[i];
					var _name		= getObjectName(_maskData) + "_mask";

					m_BakedObjects.Add(createBakedObject(_name, ObjectType.Mask, _maskData));
				}
			}

			m_OldMode = false;
		}

		private void createOldModeObjects()
		{
			var objects = movieClip.asset.getObjects(movieClip.timelineID);
			var masks	= movieClip.asset.getMasks(movieClip.timelineID);
			for (int i = 0; i < objects.Count; ++i)
			{
				var _objectData = objects[i];
				var _name		= getObjectName(_objectData);
				var _type		= movieClip.asset.getExternalData(movieClip.timelineID).objectTypeFlags[i];

				m_Objects.Add(createOldModeObject(_name, _type, _objectData));
			}

			if (masks != null)
			{
				for (int i = 0; i < masks.Count; ++i)
				{
					var _maskData	= masks[i];
					var _name		= getObjectName(_maskData) + "_mask";

					m_Objects.Add(createOldModeObject(_name, ObjectType.Mask, _maskData));
				}
			}

			m_OldMode = true;
		}

		private string getObjectName(GAF.Data.GAFObjectData _Object)
		{
			var namedParts = movieClip.asset.getNamedParts(movieClip.timelineID);
			var part = namedParts.Find((partData) => partData.objectID == _Object.id);

			return part == null ? _Object.atlasElementID.ToString() + "_" + _Object.id.ToString() : part.name;
		}

		private GAFBakedObject createBakedObject(string _Name, ObjectType _Type, GAF.Data.GAFObjectData _Data)
		{
			GAFBakedObject bakedObject = new GAFBakedObject();
			bakedObject.initialize(_Name, _Type, movieClip, this, _Data.id, _Data.atlasElementID);

			return bakedObject;
		}

		private GAFObject createOldModeObject(string _Name, ObjectType _Type, GAF.Data.GAFObjectData _Data)
		{
			var gameObj = new GameObject { name = _Name };
			gameObj.transform.parent		= this.transform;
			gameObj.transform.localScale	= Vector3.one;
			gameObj.transform.localPosition	= Vector3.zero;

			var component = gameObj.AddComponent<GAFObject>();
			component.initialize(_Name, _Type, movieClip, this, _Data.id, _Data.atlasElementID);

			return component;
		}

		private void bakedObjectsCleanUp()
		{
			for (int i = 0; i < m_BakedObjects.Count; i++)
			{
				if (m_BakedObjects[i].hasController())
					m_BakedObjects[i].removeController();
			}

			foreach (var obj in m_BakedObjects)
				obj.onDestroy();

			m_BakedObjects.Clear();
		}

		private void objectsCleanUp()
		{
			foreach (var obj in m_Objects)
				obj.onDestroy();

			for (int i = 0; i < m_Objects.Count; i++)
			{
				if (!Application.isPlaying)
					DestroyImmediate(m_Objects[i].gameObject);
				else
					Destroy(m_Objects[i].gameObject);
			}

			m_Objects.Clear();
		}

		#endregion // Implementation
	}
}