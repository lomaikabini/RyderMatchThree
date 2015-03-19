/*
 * File:			GAFMaskedObjectImpl_Pro.cs
 * Version:			1.0
 * Last changed:	2014/12/2 12:08
 * Author:			Alexey_Nikitin
 * Copyright:		© GAF Media
 * Project:			UnityVS.UnityProject.CSharp
 */

using UnityEngine;

using GAF.Core;
using GAF.Data;

namespace GAF.Objects
{
	public class GAFMaskedObjectImpl : GAFObjectImpl, IGAFMaskedObjectImpl
	{
		#region Members

		private int	m_StencilID	= 0;

		private Material	m_SharedMaterial	= null;
		private Material	m_MaskedMaterial	= null;

		#endregion // Members

		#region Interface

		public GAFMaskedObjectImpl(
			  GameObject _ThisObject
			, GAFObjectData _Data
			, Renderer _Renderer
			, MeshFilter _Filter)
			: base(_ThisObject, _Data, _Renderer, _Filter)
		{
			m_MaskedMaterial = new Material(Shader.Find("GAF/GAFMaskedObject"));
			m_MaskedMaterial.mainTexture = texture;
			m_MaskedMaterial.renderQueue = 3000;

			m_SharedMaterial = material;
		}

		public GAFMaskedObjectImpl(
			  Material		_MaskedMaterial
			, GameObject	_ThisObject
			, GAFObjectData	_Data
			, Renderer		_Renderer
			, MeshFilter	_Filter)
			: base(_ThisObject, _Data, _Renderer, _Filter)
		{
			m_MaskedMaterial = _MaskedMaterial;
			m_SharedMaterial = material;
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

		public void updateMasking(GAFObjectStateData _State, bool _Refresh)
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

            if (Application.isPlaying)
                Object.Destroy(m_MaskedMaterial);
            else
                Object.DestroyImmediate(m_MaskedMaterial);

            m_MaskedMaterial = null;
        }

		#endregion // Interface

		#region IGAFMaskedObjectImpl

		public void enableMasking()
		{
			m_MaskedMaterial.SetFloat("_StencilID", m_StencilID);
			material = m_MaskedMaterial;

			renderer.sharedMaterial = currentMaterial;
		}

		public void disableMasking()
		{
			material = m_SharedMaterial;

			renderer.sharedMaterial = currentMaterial;
		}

		public uint getObjectID()
		{
			return serializedProperties.objectID;
		}

		#endregion // IGAFMaskedObjectImpl

		#region IEquatable

        public bool Equals(IGAFMaskedObjectImpl _Other)
        {
            return getObjectID() == _Other.getObjectID();
        }

		#endregion // IEquatable
	}
}
