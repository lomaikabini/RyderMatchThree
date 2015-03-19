/*
 * File:			GAFBakedMaskObjectImpl.cs
 * Version:			1.0
 * Last changed:	2014/12/2 14:13
 * Author:			Alexey_Nikitin
 * Copyright:		© GAF Media
 * Project:			UnityVS.UnityProject.CSharp
 */

using UnityEngine;

using System.Collections.Generic;
using System.Linq;

using GAF.Core;
using GAF.Data;

namespace GAF.Objects
{
	public class GAFBakedMaskObjectImpl : GAFBakedObjectImpl, IGAFMaskObjectImpl
	{
		#region Members

		private int							m_StencilID		= 0;
        private Material                    m_MaskMaterial  = null;
		private List<IGAFMaskedObjectImpl>	m_MaskedObjects = new List<IGAFMaskedObjectImpl>();

		#endregion // Members

		#region GAFObjectImpl

		public GAFBakedMaskObjectImpl(
			  IGAFObjectSerializedProperties	_Data
			, GAFRenderProcessor				_Processor
			, GAFBakedObjectController			_Controller)
			: base(_Data, _Processor, _Controller)
		{
            m_MaskMaterial = new Material(Shader.Find("GAF/GAFMaskObject"));
            m_MaskMaterial.mainTexture = texture;
            m_MaskMaterial.renderQueue = 3000;

			m_StencilID = GAFStencilMaskManager.registerMask(serializedProperties.clip.GetInstanceID(), serializedProperties.objectID, this);
            m_MaskMaterial.SetFloat("_StencilID", m_StencilID);

            material = m_MaskMaterial;
		}

		protected override void updateMeshColor(GAFObjectStateData _State, bool _Refresh)
		{
			if (currentState.alpha != _State.alpha ||
				_Refresh)
			{
				if (_State.alpha == 0f || !serializedProperties.visible)
				{
					disableMask();
				}
				else
				{
					enableMask();
				}
			}

			base.updateMeshColor(_State, _Refresh);
		}

		public override void cleanUp()
		{
            base.cleanUp();

            GAFStencilMaskManager.unregisterMask(serializedProperties.clip.GetInstanceID(), serializedProperties.objectID, this);

            m_MaskMaterial = null;
		}

		#endregion // GAFObjectImpl

		#region IGAFMaskObjectImpl

		public void enableMask()
		{
			foreach (var masked in m_MaskedObjects)
				masked.enableMasking();
		}

		public void disableMask()
		{
			foreach (var masked in m_MaskedObjects)
				masked.disableMasking();
		}

		public void registerMaskedObject(IGAFMaskedObjectImpl _Masked)
		{
			if (!m_MaskedObjects.Contains(_Masked))
				m_MaskedObjects.Add(_Masked);
		}

		public void unregisterMaskedObject(IGAFMaskedObjectImpl _Masked)
		{
			m_MaskedObjects.Remove(_Masked);
		}

		public void setStencilID(int _StencilID)
		{
			m_StencilID = _StencilID;
		}

		public int getStencilID()
		{
			return m_StencilID;
		}

		#endregion // IGAFMaskObjectImpl
	}
}