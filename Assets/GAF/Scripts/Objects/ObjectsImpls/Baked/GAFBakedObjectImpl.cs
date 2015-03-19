/*
 * File:			GAFBakedObjectImpl.cs
 * Version:			1.0
 * Last changed:	2014/12/9 18:00
 * Author:			Alexey_Nikitin
 * Copyright:		© GAF Media
 * Project:			UnityVS.UnityProject.CSharp
 */

using UnityEngine;

using GAF.Core;
using GAF.Data;

namespace GAF.Objects
{
	public class GAFBakedObjectImpl : IGAFObjectImpl
	{
		#region Members

		private GAFRenderProcessor			m_RenderProcessor	= null;
		private GAFBakedObjectController	m_Controller		= null;

		#endregion // Members

		#region Interface

		public GAFBakedObjectImpl(
			  IGAFObjectSerializedProperties	_Data
			, GAFRenderProcessor				_Processor
			, GAFBakedObjectController			_Controller)
			: base(_Data, _Processor.renderer, _Processor.filter)
		{
			m_RenderProcessor	= _Processor;
			m_Controller		= _Controller;
		}

		public override void updateToState(GAFObjectStateData _State, bool _Refresh)
		{
			updateMeshColor(_State, _Refresh);
			updateTransform(_State, _Refresh);
		}

		public override void cleanUp()
		{
			base.cleanUp();
		}

		#endregion // Interface

		#region Implementation

		protected GAFRenderProcessor renderProcessor
		{
			get
			{
				return m_RenderProcessor;
			}
		}

		protected GAFBakedObjectController controller
		{
			get
			{
				return m_Controller;
			}
		}

		protected virtual void updateMeshColor(GAFObjectStateData _State, bool _Refresh)
		{
			if (_Refresh ||
				currentState.alpha != _State.alpha)
			{
				if (!serializedProperties.visible)
				{
					if (renderProcessor.contains(serializedProperties.objectID))
						renderProcessor.remove(serializedProperties.objectID);
				}
				else if (_State.alpha == 0f)
				{
					renderProcessor.remove(serializedProperties.objectID);
				}
				else
				{
					for (int i = 0; i < colors.Length; ++i)
						colors[i].a = (byte)(_State.alpha * 255f);

					if (!renderProcessor.contains(serializedProperties.objectID))
						renderProcessor.add(serializedProperties.clip.getObject(serializedProperties.objectID));
				}

				currentState.alpha = _State.alpha;
			}

			if (_Refresh ||
				currentState.colorTransformData != _State.colorTransformData)
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

				currentState.colorTransformData = _State.colorTransformData;
			}

			if (serializedProperties.clip.settings.hasIndividualMaterial)
				material.SetColor("_CustomColor", serializedProperties.clip.settings.animationColor);
		}

		protected virtual void updateMaterialColor(GAFObjectStateData _State, bool _Refresh)
		{
			if (_Refresh ||
				currentState.alpha != _State.alpha)
			{
				if (!serializedProperties.visible)
				{
					if (renderProcessor.contains(serializedProperties.objectID))
						renderProcessor.remove(serializedProperties.objectID);
				}
				else if (_State.alpha == 0f)
				{
					renderProcessor.remove(serializedProperties.objectID);
				}
				else
				{
					for (int i = 0; i < colors.Length; ++i)
						colors[i].a = (byte)(_State.alpha * 255f);

					if (!renderProcessor.contains(serializedProperties.objectID))
						renderProcessor.add(serializedProperties.clip.getObject(serializedProperties.objectID));

					var color = new Color(
						  (float)colors[0].r / 255f
						, (float)colors[0].g / 255f
						, (float)colors[0].b / 255f
						, (float)colors[0].a / 255f);

					material.SetColor("_TintColor", color);
				}

				currentState.alpha = _State.alpha;
			}

			if (_Refresh ||
				currentState.colorTransformData != _State.colorTransformData)
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

				if (serializedProperties.visible)
				{
					var color = new Color(
						  (float)colors[0].r / 255f
						, (float)colors[0].g / 255f
						, (float)colors[0].b / 255f
						, (float)colors[0].a / 255f);

					material.SetColor("_TintColor", color);
					material.SetVector("_TintColorOffset", colorsShift[0]);
				}

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
					currentState.a != _State.a ||
					currentState.b != _State.b ||
					currentState.c != _State.c ||
					currentState.d != _State.d ||
					currentState.tX != _State.tX ||
					currentState.tY != _State.tY ||
					currentState.zOrder != _State.zOrder)
				{
					var		clip		= serializedProperties.clip;
					var		pivotOffset = clip.settings.pivotOffset;
					float	scale		= clip.settings.pixelsPerUnit / clip.settings.scale;

					Matrix4x4 _transform = Matrix4x4.identity;
					_transform[0, 0] =  _State.a;
					_transform[0, 1] = -_State.c;
					_transform[1, 0] = -_State.b;
					_transform[1, 1] =  _State.d;
					_transform[0, 3] =  _State.tX / scale + serializedProperties.offset.x + pivotOffset.x;
					_transform[1, 3] = -_State.tY / scale + serializedProperties.offset.y + pivotOffset.y;
					_transform[2, 3] = _State.zOrder / scale * clip.settings.zLayerScale;

					serializedProperties.statePosition = new Vector3(_State.tX / scale, -_State.tY / scale, _State.zOrder / scale * clip.settings.zLayerScale);

					for (int i = 0; i < initialVertices.Length; i++)
					{
						currentVertices[i] = _transform.MultiplyPoint3x4(initialVertices[i]);
					}

					if (currentState.zOrder != _State.zOrder)
						renderProcessor.pushSortRequest();

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

		#endregion // Implementation
	}
}
