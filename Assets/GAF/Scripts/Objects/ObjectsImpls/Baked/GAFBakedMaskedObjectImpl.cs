/*
 * File:			GAFBakedMaskedObjectImpl.cs
 * Version:			1.0
 * Last changed:	2014/12/2 14:12
 * Author:			Alexey_Nikitin
 * Copyright:		© GAF Media
 * Project:			UnityVS.UnityProject.CSharp
 */

using UnityEngine;

using GAF.Core;
using GAF.Data;

namespace GAF.Objects
{
	public class GAFBakedMaskedObjectImpl : GAFBakedObjectImpl, IGAFMaskedObjectImpl
	{
		#region Members

		private int	m_StencilID	= 0;

		private Material m_SharedMaterial = null;
		private Material m_MaskedMaterial = null;

		#endregion // Members

		#region Interface

		public GAFBakedMaskedObjectImpl(
			  IGAFObjectSerializedProperties	_Data
			, GAFRenderProcessor				_Processor
			, GAFBakedObjectController			_Controller)
			: base(_Data, _Processor, _Controller)
		{
			m_MaskedMaterial = new Material(Shader.Find("GAF/GAFMaskedObject"));
			m_MaskedMaterial.mainTexture = texture;
			m_MaskedMaterial.renderQueue = 3000;

			var clip = serializedProperties.clip;
			m_SharedMaterial = clip.getSharedMaterial(System.IO.Path.GetFileNameWithoutExtension(texturesData.getFileName(clip.settings.csf)));
		}

		public GAFBakedMaskedObjectImpl(
			  Material							_MaskedMaterial
			, IGAFObjectSerializedProperties	_Data
			, GAFRenderProcessor				_Processor
			, GAFBakedObjectController			_Controller)
			: base(_Data, _Processor, _Controller)
		{
			m_MaskedMaterial = _MaskedMaterial;

			var clip = serializedProperties.clip;
			m_SharedMaterial = clip.getSharedMaterial(System.IO.Path.GetFileNameWithoutExtension(texturesData.getFileName(clip.settings.csf)));
		}

		public override void updateToState(GAFObjectStateData _State, bool _Refresh)
		{
			updateMasking(_State, _Refresh);

			if (material != m_SharedMaterial)
				updateMaterialColor(_State, _Refresh);
			else
				updateMeshColor(_State, _Refresh);

			updateTransform(_State, _Refresh);
		}

		public virtual void updateMasking(GAFObjectStateData _State, bool _Refresh)
		{
			if (_State.maskID != currentState.maskID ||
				_Refresh)
			{
				if (currentState.maskID >= 0)
				{
					var oldMask = GAFStencilMaskManager.getMask(serializedProperties.clip.GetInstanceID(), (uint)currentState.maskID);
					m_StencilID = 0;
					oldMask.unregisterMaskedObject(this);
					disableMasking();
				}

				if (_State.maskID >= 0)
				{
					var newMask = GAFStencilMaskManager.getMask(serializedProperties.clip.GetInstanceID(), (uint)_State.maskID);
					m_StencilID = newMask.getStencilID();
					newMask.registerMaskedObject(this);
					enableMasking();
				}

				currentState.maskID = _State.maskID;
			}
		}

		public override void cleanUp()
		{
			base.cleanUp();

			m_MaskedMaterial = null;
			m_SharedMaterial = null;
		}

		#endregion // Interface

		#region IGAFMaskedObjectImpl

		public void enableMasking()
		{
			m_MaskedMaterial.SetFloat("_StencilID", m_StencilID);
			material = m_MaskedMaterial;
		}

		public void disableMasking()
		{
			material = m_SharedMaterial;
		}

		public uint getObjectID()
		{
			return serializedProperties.objectID;
		}

		#endregion // IGAFMaskedObjectImpl

		#region IEquatable

		#endregion // IEquatable

		public bool Equals(IGAFMaskedObjectImpl _Other)
		{
			return getObjectID() == _Other.getObjectID();
		}
	}
}
