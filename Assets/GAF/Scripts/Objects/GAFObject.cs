/*
 * File:			GAFObject.cs
 * Version:			1.0
 * Last changed:	2014/12/11 14:26
 * Author:			Alexey_Nikitin
 * Copyright:		© GAF Media
 * Project:			UnityVS.UnityProject.CSharp
 */

using UnityEngine;

using GAF.Core;
using GAF.Data;

namespace GAF.Objects
{
	[AddComponentMenu("")]
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	public class GAFObject : GAFBehaviour, IGAFObject
	{
		#region Events

		public event System.Action onWillRenderObject;
		public event System.Action onLateUpdate;

		#endregion // Events

		#region Members

		[HideInInspector][SerializeField] private GAFObjectData	m_Data = null;

		[HideInInspector]
		[System.NonSerialized]
		private GAFObjectImpl m_Impl = null;
	
		#endregion // Members

		#region Base Methods Impl

		public void initialize(string _Name, ObjectType _Type, GAFBaseMovieClip _Clip, GAFObjectsManager _Manager, uint _ObjectID, uint _AtlasElementID)
		{
			m_Data = new GAFObjectData(_Name, _Type, _Clip, _Manager, _ObjectID, _AtlasElementID);
		}

		public void reload(GAFRenderProcessor _Processor)
		{
			cachedFilter.hideFlags		= HideFlags.NotEditable;
			cachedRenderer.hideFlags	= HideFlags.NotEditable;

			if (m_Impl != null)
				m_Impl.cleanUp();

			m_Impl = GAFObjectImplsFactory.getImpl(gameObject, m_Data, cachedRenderer, cachedFilter);
		}

		public void updateToState(GAFObjectStateData _State, bool _Refresh)
		{
			gameObject.SetActive(_State.alpha > 0);

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

		#region IComparable

		public int CompareTo(object other)
		{
			return properties.zOrder.CompareTo(((IGAFObject)other).properties.zOrder);
		}

		#endregion // IComparable

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
	}
}