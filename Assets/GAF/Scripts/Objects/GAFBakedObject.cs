/*
 * File:			GAFBakedObject.cs
 * Version:			1.0
 * Last changed:	2014/12/3 12:23
 * Author:			Alexey_Nikitin
 * Copyright:		© GAF Media
 * Project:			UnityVS.UnityProject.CSharp
 */

using UnityEngine;

using GAF.Core;
using GAF.Data;

namespace GAF.Objects
{
	[System.Serializable]
	public class GAFBakedObject : IGAFObject
	{
		#region Members
		
		[HideInInspector][SerializeField] private GAFObjectData				m_Data			= null;
		[HideInInspector][SerializeField] private GAFBakedObjectController	m_Controller	= null;

		[HideInInspector]
		[System.NonSerialized]
		private GAFBakedObjectImpl m_Impl = null;

		#endregion // Members

		#region Base Methods Impl

		public void initialize(string _Name, ObjectType _Type, GAFBaseMovieClip _Clip, GAFObjectsManager _Manager, uint _ObjectID, uint _AtlasElementID)
		{
			m_Data = new GAFObjectData(_Name, _Type, _Clip, _Manager, _ObjectID, _AtlasElementID);
		}

		public void reload(GAFRenderProcessor _Processor)
		{
			if (hasController())
				m_Controller.registerObject(this);

			if (m_Impl != null)
				m_Impl.cleanUp();
			
			m_Impl = GAFBakedObjectImplsFactory.getImpl(m_Data, _Processor, m_Controller);
		}

		public void updateToState(GAFObjectStateData _State, bool _Refresh)
		{
			m_Impl.updateToState(_State, _Refresh);
		}

		public void onDestroy()
		{
			if (m_Impl != null)
				m_Impl.cleanUp();
		}

		#endregion // Base Methods Impl

		#region Properties

		public IGAFObjectProperties properties
		{
			get
			{
				return m_Impl;
			}
		}

		public IGAFObjectSerializedProperties serializedProperties
		{
			get
			{
				return m_Data;
			}
		}

		#endregion // Properties

		#region Baked Object Interface

		public bool hasController()
		{
			return m_Controller != null;
		}

		public void addController()
		{
			if (!hasController())
			{
				var gameObj = new GameObject { name = serializedProperties.name };
				gameObj.transform.parent = serializedProperties.clip.transform;
				gameObj.transform.localScale = Vector3.one;
				gameObj.transform.localRotation = Quaternion.identity;
				gameObj.transform.localPosition = serializedProperties.offset;

				m_Controller = gameObj.AddComponent<GAFBakedObjectController>();
			}
		}

		public void removeController()
		{
			if (hasController())
			{
				if (!Application.isPlaying)
					Object.DestroyImmediate(m_Controller.gameObject);
				else
					Object.Destroy(m_Controller.gameObject);

				m_Controller = null;
			}
		}

		#endregion // Baked Object Interface

		#region IComparable

		public int CompareTo(object other)
		{
			return properties.zOrder.CompareTo(((IGAFObject)other).properties.zOrder);
		}

		#endregion // IComparable
	}
}