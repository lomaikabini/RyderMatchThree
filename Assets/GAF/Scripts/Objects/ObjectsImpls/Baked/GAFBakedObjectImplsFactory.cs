/*
 * File:			GAFBakedObjectImplsFactory.cs
 * Version:			1.0
 * Last changed:	2014/12/2 12:05
 * Author:			Alexey_Nikitin
 * Copyright:		© GAF Media
 * Project:			UnityVS.UnityProject.CSharp
 */

using UnityEngine;

namespace GAF.Objects
{
	public static class GAFBakedObjectImplsFactory
	{
		public static GAFBakedObjectImpl getImpl(GAFObjectData _Data, GAFRenderProcessor _Processor, GAFBakedObjectController _Controller)
		{
			GAFBakedObjectImpl impl = null;

			switch (_Data.type)
			{
				case ObjectType.Simple:		impl = new GAFBakedObjectImpl(_Data, _Processor, _Controller); break;
				case ObjectType.Masked:		impl = new GAFBakedMaskedObjectImpl(_Data, _Processor, _Controller); break;
				case ObjectType.Mask:		impl = new GAFBakedMaskObjectImpl(_Data, _Processor, _Controller); break;
				case ObjectType.Filtered:	impl = new GAFBakedObjectImpl(_Data, _Processor, _Controller); break;
				case ObjectType.Complex:	impl = new GAFBakedMaskedObjectImpl(_Data, _Processor, _Controller); break;
			}

			return impl;
		}
	}
}
