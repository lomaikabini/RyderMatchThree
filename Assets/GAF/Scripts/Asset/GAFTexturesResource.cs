/*
 * File:			GAFTexturesResource.cs
 * Version:			1.0
 * Last changed:	2014/11/17 19:11
 * Author:			Alexey_Nikitin
 * Copyright:		© GAF Media
 * Project:			UnityVS.UnityProject.CSharp
 */

using UnityEngine;

using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace GAF.Assets
{
	[System.Serializable]
	public class GAFTexturesResource : ScriptableObject
	{
		#region Members

		[HideInInspector][SerializeField] private GAFAnimationAsset		m_Asset		= null;		
		[HideInInspector][SerializeField] private float 				m_Scale		= 1f;
		[HideInInspector][SerializeField] private float 				m_CSF		= 1f;

		[HideInInspector][SerializeField] private string 				m_CurrentDataPath	= string.Empty;
		[HideInInspector][SerializeField] private List<GAFResourceData> m_Data				= new List<GAFResourceData>();

		#endregion // Members

		#region Interface

		public void initialize(GAFAnimationAsset _Asset, List<string> _Names, float _Scale, float _CSF, string _DataPath)
		{
			m_Asset			= _Asset;
			m_Scale			= _Scale;
			m_CSF			= _CSF;
			currentDataPath	= _DataPath;

			foreach (var name in _Names)
				m_Data.Add(new GAFResourceData(name));
		}

		public void setData(string _SearchName, Texture2D _SharedTexture, Material _SharedMaterial)
		{
			var data = m_Data.Find((_data) => _data.name == _SearchName);
			if (data != null)
			{
				data.set(_SharedTexture, _SharedMaterial);
			}
		}

		public bool isDataValid(string _SearchName)
		{
			var data = m_Data.Find((_data) => _data.name == _SearchName);
			return data != null ? data.isValid : false;
		}

		public Texture2D getTexture(string _Name)
		{
			var data = m_Data.Find((_data) => _data.name == _Name);
			return data != null && data.isValid ? data.sharedTexture : null;
		}

		public Material getSharedMaterial(string _Name)
		{
			Material material = null;

			var data = m_Data.Find((_data) => _data.name == _Name);
			if (data != null && data.isValid)
				material = data.sharedMaterial;

			material.EnableKeyword("GAF_VERTICES_TRANSFORM_ON");
			material.DisableKeyword("GAF_VERTICES_TRANSFORM_OFF");
			material.renderQueue = 3000;

			return material;
		}

		#endregion // Interface

		#region Properties

		public GAFAnimationAsset asset
		{
			get
			{
				return m_Asset;
			}
		}

		public bool isValid
		{
			get
			{
				return m_Asset != null;
			}
		}

		public bool isReady
		{
			get
			{
				return isValid && m_Data.All(data => data.isValid);
			}
		}

		public float scale
		{
			get
			{
				return m_Scale;
			}
		}

		public float csf
		{
			get
			{
				return m_CSF;
			}
		}

		public List<GAFResourceData> data
		{
			get
			{
				return m_Data.Where(data => data.isValid).Select(data => data).ToList();
			}
		}

		public List<GAFResourceData> missingData
		{
			get
			{
				return m_Data.Where(data => !data.isValid).Select(data => data).ToList();
			}
		}

		public string currentDataPath
		{
			get
			{
				return m_CurrentDataPath;
			}
			set
			{
				m_CurrentDataPath = value;
			}
		}

		#endregion // Properties
	}
}