/*
 * File:			IGAFObjectImpl.cs
 * Version:			1.0
 * Last changed:	2014/11/20 12:09
 * Author:			Alexey_Nikitin
 * Copyright:		© GAF Media
 * Project:			UnityVS.UnityProject.CSharp
 */

using UnityEngine;

using GAF.Core;
using GAF.Data;

namespace GAF.Objects
{
	public abstract class IGAFObjectImpl : IGAFObjectProperties
	{
		#region Static

		protected static readonly Color32		initialColor = new Color32((byte)255, (byte)255, (byte)255, (byte)255);
		protected static readonly Vector3[]		normals = new Vector3[4] { new Vector3(0f, 0f, -1f), new Vector3(0f, 0f, -1f), new Vector3(0f, 0f, -1f), new Vector3(0f, 0f, -1f) };
		protected static readonly int[]			triangles = new int[6] { 2, 0, 1, 3, 0, 2 };

		#endregion // Static

		#region Members

		private IGAFObjectSerializedProperties	m_Data				= null;
		private Renderer						m_Renderer			= null;
		private MeshFilter						m_Filter			= null;

		private GAFObjectStateData				m_CurrentState		= null;
		private GAFAtlasData 					m_AtlasData			= null;
		private GAFAtlasElementData				m_AtlasElementData	= null;
		private GAFTexturesData 				m_TexturesData		= null;
		private Texture2D						m_Texture			= null;
		private Material						m_Material			= null;

		private Vector3[]						m_Vertices			= null;
		private Vector2[]						m_UV				= null;
		private Color32[]						m_Colors			= null;
		private Vector4[]						m_ColorShift		= null;
		private Vector3[]						m_CurrentVertices	= new Vector3[4];

		#endregion // Members

		#region Base Methods

		public IGAFObjectImpl(
			  IGAFObjectSerializedProperties	_Data
			, Renderer							_Renderer
			, MeshFilter						_Filter)
		{
			m_Data		= _Data;
			m_Renderer	= _Renderer;
			m_Filter	= _Filter;

			initializeBaseData();
		}

		public abstract void updateToState(
			  GAFObjectStateData	_State
			, bool					_Refresh);

		public virtual void cleanUp()
		{ 
		}

		#endregion // Base Methods

		#region IGAFObjectProperties

		public virtual Renderer renderer 
		{
			get
			{
				return m_Renderer;
			}
		}

		public virtual MeshFilter filter 
		{
			get
			{
				return m_Filter;
			}
		}

		public virtual Material currentMaterial
		{
			get
			{
				return serializedProperties.material != null ? serializedProperties.material : material;
			}
		}

		public virtual Texture2D texture
		{
			get
			{
				return m_Texture;
			}
		}

		public virtual Material material
		{
			get
			{
				return m_Material;
			}

			protected set
			{
				m_Material = value;
			}
		}

		public virtual GAFAtlasData atlasData
		{
			get
			{
				return m_AtlasData;
			}
		}

		public virtual GAFAtlasElementData atlasElementData
		{
			get
			{
				return m_AtlasElementData;
			}
		}

		public virtual GAFTexturesData texturesData
		{
			get
			{
				return m_TexturesData;
			}
		}

		public virtual GAFObjectStateData currentState
		{
			get
			{
				return m_CurrentState;
			}

			protected set
			{
				m_CurrentState = value;
			}
		}

		public virtual int zOrder
		{
			get
			{
				return -currentState.zOrder;
			}
		}

		public virtual Color32[] colors
		{
			get
			{
				return m_Colors;
			}

			protected set
			{
				m_Colors = value;
			}
		}

		public virtual Vector4[] colorsShift
		{
			get
			{
				return m_ColorShift;
			}

			protected set
			{
				m_ColorShift = value;
			}
		}

		public virtual Vector3[] initialVertices
		{
			get
			{
				return m_Vertices;
			}
		}

		public virtual Vector3[] currentVertices
		{
			get
			{
				return m_CurrentVertices;
			}

			protected set
			{
				m_CurrentVertices = value;
			}
		}

		public virtual Vector2[] uvs
		{
			get
			{
				return m_UV;
			}

			protected set
			{
				m_UV = value;
			}
		}

		#endregion // Base Data

		#region Protected properties

		protected IGAFObjectSerializedProperties serializedProperties
		{
			get
			{
				return m_Data;
			}
		}

		protected bool hasCustomTexture
		{
			get
			{
				return serializedProperties.material != null;
			}
		}

		#endregion // Protected properties

		#region Implementation

		protected virtual void initializeBaseData()
		{
			var clip = serializedProperties.clip;

			m_AtlasData			= clip.asset.getAtlases(clip.timelineID).Find(atlas => atlas.scale == clip.settings.scale);
			m_AtlasElementData	= m_AtlasData.getElement(serializedProperties.atlasElementID);
			m_TexturesData		= m_AtlasData.getAtlas(m_AtlasElementData.atlasID);

			m_Texture	= hasCustomTexture ? serializedProperties.material.mainTexture as Texture2D :
							clip.resource.getTexture(System.IO.Path.GetFileNameWithoutExtension(texturesData.getFileName(clip.settings.csf)));
			m_Material	= hasCustomTexture ? serializedProperties.material :
							clip.getSharedMaterial(System.IO.Path.GetFileNameWithoutExtension(texturesData.getFileName(clip.settings.csf)));
			m_Material.renderQueue = 3000;

			m_CurrentState		= new GAFObjectStateData(serializedProperties.objectID);
			m_Colors			= new Color32[4] { initialColor, initialColor, initialColor, initialColor };
			m_ColorShift		= new Vector4[4];

			calcInitialVertices();
			calcUV();
		}

		private void calcInitialVertices()
		{
			float scale			= atlasElementData.scale * serializedProperties.clip.settings.pixelsPerUnit;
			float scaledPivotX	= atlasElementData.pivotX / scale;
			float scaledPivotY	= atlasElementData.pivotY / scale;
			float scaledWidth	= atlasElementData.width  / scale * serializedProperties.meshSizeMultiplier.x;
			float scaledHeight	= atlasElementData.height / scale * serializedProperties.meshSizeMultiplier.y;

			m_Vertices = new Vector3[4];
			m_Vertices[0] = new Vector3(-scaledPivotX, scaledPivotY - scaledHeight, 0f);
			m_Vertices[1] = new Vector3(-scaledPivotX, scaledPivotY, 0f);
			m_Vertices[2] = new Vector3(-scaledPivotX + scaledWidth, scaledPivotY, 0f);
			m_Vertices[3] = new Vector3(-scaledPivotX + scaledWidth, scaledPivotY - scaledHeight, 0f);				
		}

		protected virtual void calcUV()
		{
			var clip = serializedProperties.clip;

			var atlasTexture = hasCustomTexture ? serializedProperties.material.mainTexture as Texture2D :
								clip.resource.getTexture(System.IO.Path.GetFileNameWithoutExtension(texturesData.getFileName(clip.settings.csf)));

			var x		= atlasElementData.x;
			var y		= atlasElementData.y;
			var width	= atlasElementData.width;
			var height	= atlasElementData.height;

			if (serializedProperties.useCustomTextureRect)
			{
				x		= serializedProperties.atlasTextureRect.x;
				y		= serializedProperties.atlasTextureRect.y;
				width	= serializedProperties.atlasTextureRect.width;
				height	= serializedProperties.atlasTextureRect.height;
			}
			else
			{
				serializedProperties.atlasTextureRect = new Rect(x, y, width, height);
			}

			float scaledElementLeftX	= x * clip.settings.csf / atlasTexture.width;
			float scaledElementRightX	= (x + width) * clip.settings.csf / atlasTexture.width;
			float scaledElementTopY		= (atlasTexture.height - y * clip.settings.csf - height * clip.settings.csf) / atlasTexture.height;
			float scaledElementBottomY	= (atlasTexture.height - y * clip.settings.csf) / atlasTexture.height;

			m_UV = new Vector2[4];
			m_UV[0] = new Vector2(scaledElementLeftX, scaledElementTopY);
			m_UV[1] = new Vector2(scaledElementLeftX, scaledElementBottomY);
			m_UV[2] = new Vector2(scaledElementRightX, scaledElementBottomY);
			m_UV[3] = new Vector2(scaledElementRightX, scaledElementTopY);
		}

		#endregion // Implementation
	}
}
