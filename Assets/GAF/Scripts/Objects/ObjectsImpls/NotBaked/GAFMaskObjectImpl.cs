/*
 * File:			GAFMaskObjectImpl.cs
 * Version:			1.0
 * Last changed:	2014/12/15 8:15
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
	public class GAFMaskObjectImpl : GAFObjectImpl, IGAFMaskObjectImpl
	{
		#region Members

		private int							m_StencilID		= 0;
        private Material                    m_MaskMaterial  = null;
		private List<IGAFMaskedObjectImpl>	m_MaskedObjects = new List<IGAFMaskedObjectImpl>();

		#endregion // Members

		#region GAFObjectImpl

		public GAFMaskObjectImpl(
			  GameObject _ThisObject
			, GAFObjectData _Data
			, Renderer _Renderer
			, MeshFilter _Filter)
			: base(_ThisObject, _Data, _Renderer, _Filter)
		{
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

            if (Application.isPlaying)
                Object.Destroy(m_MaskMaterial);
            else
                Object.DestroyImmediate(m_MaskMaterial);

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

		public int getStencilID()
		{
			return m_StencilID;
		}

		#endregion // IGAFMaskObjectImpl

		#region Implementation

		protected override void resetRenderer()
		{
			var clip = serializedProperties.clip;

            m_MaskMaterial = new Material(Shader.Find("GAF/GAFMaskObject"));
            m_MaskMaterial.mainTexture = texture;
            m_MaskMaterial.renderQueue = 3000;

            material = m_MaskMaterial;

			m_StencilID = GAFStencilMaskManager.registerMask(serializedProperties.clip.GetInstanceID(), serializedProperties.objectID, this);
			material.SetFloat("_StencilID", m_StencilID);

			renderer.sharedMaterial		= material;
			renderer.castShadows		= false;
			renderer.receiveShadows		= false;
			renderer.sortingLayerName	= clip.settings.spriteLayerName;
			renderer.sortingOrder		= clip.settings.spriteLayerValue;
		}

		#endregion // Implementation
	}
}