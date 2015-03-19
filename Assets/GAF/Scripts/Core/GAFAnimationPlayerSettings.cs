/*
 * File:			GAFAnimationPlayerSettings.cs
 * Version:			1.0
 * Last changed:	2014/12/15 8:14
 * Author:			Alexey_Nikitin
 * Copyright:		© GAF Media
 * Project:			UnityVS.UnityProject.CSharp
 */

using UnityEngine;

using GAF.Assets;

namespace GAF.Core
{
	[System.Serializable]
	public class GAFAnimationPlayerSettings
	{
		#region Members

		[HideInInspector][SerializeField] private float 		m_Scale					= 1f;
		[HideInInspector][SerializeField] private float 		m_CSF					= 1f;
		[HideInInspector][SerializeField] private float 		m_PixelsPerUnit			= 1f;
		[HideInInspector][SerializeField] private bool 			m_PlayAutomatically		= true;
		[HideInInspector][SerializeField] private bool 			m_IgnoreTimeScale 		= false;
		[HideInInspector][SerializeField] private bool 			m_PerfectTiming			= true;
		[HideInInspector][SerializeField] private bool 			m_PlayInBackground		= true;
		[HideInInspector][SerializeField] private GAFWrapMode 	m_WrapMode 				= GAFWrapMode.Loop;
		[HideInInspector][SerializeField] private int 			m_TargetFPS 			= 30;
		[HideInInspector][SerializeField] private int 			m_SpriteLayerID			= 0;
		[HideInInspector][SerializeField] private string 		m_SpriteLayerName		= "Default";
		[HideInInspector][SerializeField] private int 			m_SpriteLayerValue		= 0;
		[HideInInspector][SerializeField] private Vector2		m_PivotOffset			= new Vector2();
		[HideInInspector][SerializeField] private bool 			m_HasIndividualMaterial	= false;	
		[HideInInspector][SerializeField] private Color 		m_AnimationColor		= new Color(1, 1, 1, 1);	
		[HideInInspector][SerializeField] private float			m_ZLayerScale			= 1f;

		#endregion // Members

		#region Properties

		public float scale
		{
			get
			{
				return m_Scale;
			}

			set
			{
				m_Scale = value;
			}
		}

		public float csf
		{
			get
			{
				return m_CSF;
			}

			set
			{
				m_CSF = value;
			}
		}

		public float pixelsPerUnit
		{
			get
			{
				return m_PixelsPerUnit;
			}

			set
			{
				m_PixelsPerUnit = value;
			}
		}

		public bool playAutomatically
		{
			get
			{
				return m_PlayAutomatically;
			}

			set
			{
				m_PlayAutomatically = value;
			}
		}

		public bool ignoreTimeScale
		{
			get
			{
				return m_IgnoreTimeScale;
			}

			set
			{
				m_IgnoreTimeScale = value;
			}
		}

		public bool perfectTiming
		{
			get
			{
				return m_PerfectTiming;
			}

			set
			{
				m_PerfectTiming = value;
			}
		}

		public bool playInBackground
		{
			get
			{
				return m_PlayInBackground;
			}

			set
			{
				m_PlayInBackground = value;
			}
		}

		public GAFWrapMode wrapMode
		{
			get
			{
				return m_WrapMode;
			}

			set
			{
				m_WrapMode = value;
			}
		}

		public uint targetFPS
		{
			get
			{
				return (uint)m_TargetFPS;
			}

			set
			{
				m_TargetFPS = (int)value;
			}
		}

		public float targetSPF
		{
			get
			{
				return 1f / m_TargetFPS;
			}
		}

		public int spriteLayerID
		{
			get
			{
				return m_SpriteLayerID;
			}

			set
			{
				m_SpriteLayerID = value;
			}
		}

		public string spriteLayerName
		{
			get
			{
				return m_SpriteLayerName;
			}

			set
			{
				m_SpriteLayerName = value;
			}
		}

		public int spriteLayerValue
		{
			get
			{
				return m_SpriteLayerValue;
			}

			set
			{
				m_SpriteLayerValue = value;
			}
		}

		public Vector2 pivotOffset
		{
			get
			{
				return m_PivotOffset;
			}

			set
			{
				m_PivotOffset = value;
			}
		}

		public float zLayerScale
		{
			get
			{
				return m_ZLayerScale;
			}

			set
			{
				m_ZLayerScale = value;
			}
		}

		public bool hasIndividualMaterial
		{
			get
			{
				return m_HasIndividualMaterial;
			}
		}

		public Color animationColor
		{
			get
			{
				return m_AnimationColor;
			}
		}

		#endregion // Properties

		#region Interface

		public void init(GAFAnimationAsset _Asset)
		{
			scale = _Asset.scales[0];
			csf = _Asset.csfs[0];
		}

		#endregion
	}
}
