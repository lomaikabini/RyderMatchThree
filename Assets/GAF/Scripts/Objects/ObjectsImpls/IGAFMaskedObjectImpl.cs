/*
 * File:			igafmaskedobjectimpl.cs
 * Version:			3.9
 * Last changed:	2014/10/13 18:22
 * Author:			Alexey_Nikitin
 * Copyright:		© Catalyst Apps
 * Project:			UnityVS.UnityProject.CSharp
 */

using UnityEngine;
using System.Collections;

namespace GAF.Objects
{
	public interface IGAFMaskedObjectImpl : System.IEquatable<IGAFMaskedObjectImpl>
	{
		void enableMasking();

		void disableMasking();

		uint getObjectID();
	}
}
