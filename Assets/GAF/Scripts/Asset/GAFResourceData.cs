/*
 * File:			gafresourcedata.cs
 * Version:			3.9
 * Last changed:	2014/9/8 16:30
 * Author:			Alexey_Nikitin
 * Copyright:		© Catalyst Apps
 * Project:			UnityVS.UnityProject.CSharp
 */

using UnityEngine;

namespace GAF.Assets
{
	[System.Serializable]
	public class GAFResourceData
	{
		#region Members
		
		[HideInInspector][SerializeField] private string		m_SearchName		= string.Empty;
		[HideInInspector][SerializeField] private Texture2D		m_SharedTexture		= null;
		[HideInInspector][SerializeField] private Material		m_SharedMaterial	= null;
		
		#endregion // Members

		#region Interface

		public GAFResourceData(string _Name)
		{
			m_SearchName = _Name;
		}

		public void set(Texture2D _Texture, Material _Material)
		{
			m_SharedTexture = _Texture;
			m_SharedMaterial = _Material;
		}

		public bool isValid
		{
			get
			{
				return m_SharedTexture != null && m_SharedMaterial != null && m_SharedMaterial.mainTexture == m_SharedTexture;
			}
		}

		public string name
		{
			get
			{
				return m_SearchName;
			}
		}

		public Texture2D sharedTexture
		{
			get
			{
				return m_SharedTexture;
			}
		}

		public Material sharedMaterial
		{
			get
			{
				return m_SharedMaterial;
			}
		}

		#endregion // Interface
	}
}
