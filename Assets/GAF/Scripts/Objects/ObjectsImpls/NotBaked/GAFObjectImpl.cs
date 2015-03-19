/*
 * File:			GAFObjectImpl.cs
 * Version:			1.0
 * Last changed:	2014/12/15 8:15
 * Author:			Alexey_Nikitin
 * Copyright:		© GAF Media
 * Project:			UnityVS.UnityProject.CSharp
 */

using UnityEngine;

using GAF.Core;
using GAF.Data;

namespace GAF.Objects
{
	public class GAFObjectImpl : IGAFObjectImpl
	{
		#region Members

		private GameObject m_Object = null;

		#endregion // Members

		#region Interface

		public GAFObjectImpl(
			  GameObject		_ThisObject
			, GAFObjectData		_Data
			, Renderer			_Renderer
			, MeshFilter		_Filter) : base(_Data, _Renderer, _Filter)
		{
			m_Object = _ThisObject;

			resetRenderer();
			resetMesh();
		}

		public override void updateToState(GAFObjectStateData _State, bool _Refresh)
		{
			updateMeshColor(_State, _Refresh);
			updateTransform(_State, _Refresh);
		}

		public override void cleanUp()
		{
			if (filter != null && filter.sharedMesh != null)
			{
				filter.sharedMesh.Clear();

				if (Application.isPlaying)
					Object.Destroy(filter.sharedMesh);
				else
					Object.DestroyImmediate(filter.sharedMesh);
			}
		}

		#endregion // Interface

		#region Protected properties

		protected GameObject thisObject
		{
			get
			{
				return m_Object;
			}
		}

		#endregion // Protected properties

		#region Implementation

		protected virtual void updateMeshColor(GAFObjectStateData _State, bool _Refresh)
		{
			var setColors		= false;
			var setColorsShift	= false;

			if (currentState.alpha != _State.alpha ||
				_Refresh)
			{
				if (_State.alpha == 0f)
				{
					renderer.enabled = false;
				}
				else
				{
					renderer.enabled = serializedProperties.visible;

					for (int i = 0; i < colors.Length; ++i)
						colors[i].a = (byte)(_State.alpha * 255f);
				}

				setColors = true;

				currentState.alpha = _State.alpha;
			}

			if (currentState.colorTransformData != _State.colorTransformData ||
				_Refresh)
			{
				if (_State.colorTransformData != null)
				{
					for (int i = 0; i < colors.Length; ++i)
						colors[i] = _State.colorTransformData.multipliers;

					for (int i = 0; i < colorsShift.Length; ++i)
						colorsShift[i] = _State.colorTransformData.offsets;
				}
				else
				{
					for (int i = 0; i < colors.Length; ++i)
					{
						colors[i].r = (byte)255;
						colors[i].g = (byte)255;
						colors[i].b = (byte)255;
					}

					var offset = new Vector4(0f, 0f, 0f, 0f);
					for (int i = 0; i < colorsShift.Length; ++i)
						colorsShift[i] = offset;
				}

				setColors = true;
				setColorsShift = true;

				currentState.colorTransformData = _State.colorTransformData;
			}

			if (setColors)
				filter.sharedMesh.colors32 = colors;

			if (setColorsShift)
				filter.sharedMesh.tangents = colorsShift;

			if (serializedProperties.clip.settings.hasIndividualMaterial)
				material.SetColor("_CustomColor", serializedProperties.clip.settings.animationColor);
		}

		protected virtual void updateMaterialColor(GAFObjectStateData _State, bool _Refresh)
		{
			if (currentState.alpha != _State.alpha ||
				_Refresh)
			{
				renderer.enabled = _State.alpha > 0f && serializedProperties.visible;

				for (int i = 0; i < colors.Length; ++i)
					colors[i].a = (byte)(_State.alpha * 255f);

				var color = new Color(
					  (float)colors[0].r / 255f
					, (float)colors[0].g / 255f
					, (float)colors[0].b / 255f
					, (float)colors[0].a / 255f);

				material.SetColor("_TintColor", color);

				currentState.alpha = _State.alpha;
			}

			if (currentState.colorTransformData != _State.colorTransformData ||
				_Refresh)
			{
				if (_State.colorTransformData != null)
				{
					for (int i = 0; i < colors.Length; ++i)
						colors[i] = _State.colorTransformData.multipliers;

					for (int i = 0; i < colorsShift.Length; ++i)
						colorsShift[i] = _State.colorTransformData.offsets;
				}
				else
				{
					for (int i = 0; i < colors.Length; ++i)
					{
						colors[i].r = (byte)255;
						colors[i].g = (byte)255;
						colors[i].b = (byte)255;
					}

					var offset = new Vector4(0f, 0f, 0f, 0f);
					for (int i = 0; i < colorsShift.Length; ++i)
						colorsShift[i] = offset;
				}

				var color = new Color(
						  (float)colors[0].r / 255f
						, (float)colors[0].g / 255f
						, (float)colors[0].b / 255f
						, (float)colors[0].a / 255f);

				material.SetColor("_TintColor", color);
				material.SetVector("_TintColorOffset", colorsShift[0]);

				currentState.colorTransformData = _State.colorTransformData;
			}

			if (serializedProperties.clip.settings.hasIndividualMaterial)
				material.SetColor("_CustomColor", serializedProperties.clip.settings.animationColor);
		}

		protected virtual void updateTransform(GAFObjectStateData _State, bool _Refresh)
		{
			if (currentState.alpha > 0)
			{
				if (_Refresh ||
					currentState.zOrder != _State.zOrder ||
					currentState.tX != _State.tX ||
					currentState.tY != _State.tY)
				{
					var	clip  = serializedProperties.clip;
					var	scale = clip.settings.pixelsPerUnit / clip.settings.scale;

					serializedProperties.statePosition = new Vector3(_State.tX / scale, -_State.tY / scale, _State.zOrder / scale * clip.settings.zLayerScale);
					m_Object.transform.localPosition = serializedProperties.statePosition + serializedProperties.offset + (Vector3)clip.settings.pivotOffset;
					currentState.zOrder = _State.zOrder;
				}

				if (_Refresh ||
					currentState.a != _State.a ||
					currentState.b != _State.b ||
					currentState.c != _State.c ||
					currentState.d != _State.d)
				{
					Matrix4x4 _transform = Matrix4x4.identity;
					_transform[0, 0] =  _State.a;
					_transform[0, 1] = -_State.c;
					_transform[1, 0] = -_State.b;
					_transform[1, 1] =  _State.d;
					_transform[2, 3] =  0;

					for (int i = 0; i < initialVertices.Length; i++)
					{
						currentVertices[i] = _transform.MultiplyPoint3x4(initialVertices[i]);
					}

					filter.sharedMesh.vertices = currentVertices;
                    filter.sharedMesh.RecalculateBounds();

					currentState.tX		= _State.tY;
					currentState.tY		= _State.tY;
					currentState.zOrder = _State.zOrder;
					currentState.a		= _State.a;
					currentState.b		= _State.b;
					currentState.c		= _State.c;
					currentState.d		= _State.d;
				}
			}
		}

		protected virtual void resetRenderer()
		{
			var clip = serializedProperties.clip;

			renderer.sharedMaterial		= currentMaterial;
			renderer.castShadows		= false;
			renderer.receiveShadows		= false;
			renderer.sortingLayerName	= clip.settings.spriteLayerName;
			renderer.sortingOrder		= clip.settings.spriteLayerValue;
		}

		protected virtual void resetMesh()
		{
			Mesh mesh = new Mesh();
			mesh.name = serializedProperties.name;

			mesh.vertices	= initialVertices;
			mesh.uv			= uvs;
			mesh.triangles	= triangles;
			mesh.normals	= normals;
			mesh.colors32	= colors;

			filter.sharedMesh = mesh;
		}

		#endregion // Implementation
	}
}
