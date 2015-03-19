/*
 * File:			GAFBehaviour.cs
 * Version:			1.0
 * Last changed:	2014/11/20 9:16
 * Author:			Alexey_Nikitin
 * Copyright:		© GAF Media
 * Project:			UnityVS.UnityProject.CSharp
 */

using UnityEngine;

namespace GAF.Core
{
	[AddComponentMenu("")]
	public class GAFBehaviour : MonoBehaviour
	{
		private Transform	m_CachedTransform	= null;
		private Renderer	m_CachedRenderer	= null;
		private MeshFilter	m_CachedFilter		= null;

		public Transform cachedTransform
		{
			get
			{
				if (!m_CachedTransform)
				{
					m_CachedTransform = base.GetComponent<Transform>();
				}

				return m_CachedTransform;
			}
		}

		public Renderer cachedRenderer
		{
			get
			{
 				if (!m_CachedRenderer)
				{
					m_CachedRenderer = base.GetComponent<Renderer>();
				}

				return m_CachedRenderer;
			}
		}

		public MeshFilter cachedFilter
		{
			get
			{
				if (!m_CachedFilter)
				{
					m_CachedFilter = base.GetComponent<MeshFilter>();
				}

				return m_CachedFilter;
			}
		}
	}
}
