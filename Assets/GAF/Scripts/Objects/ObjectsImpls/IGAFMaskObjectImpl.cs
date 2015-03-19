/*
 * File:			igafmaskobjectimpl.cs
 * Version:			3.9
 * Last changed:	2014/10/13 18:07
 * Author:			Alexey_Nikitin
 * Copyright:		© Catalyst Apps
 * Project:			UnityVS.UnityProject.CSharp
 */

namespace GAF.Objects
{
	public interface IGAFMaskObjectImpl
	{
		void enableMask();

		void disableMask();

		void registerMaskedObject(IGAFMaskedObjectImpl _Masked);

		void unregisterMaskedObject(IGAFMaskedObjectImpl _Masked);

		int getStencilID();
	}
}
