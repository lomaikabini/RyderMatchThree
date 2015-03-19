/*
 * File:			GAFObjectImplsFactory.cs
 * Version:			1.0
 * Last changed:	2014/12/2 12:06
 * Author:			Alexey_Nikitin
 * Copyright:		© GAF Media
 * Project:			UnityVS.UnityProject.CSharp
 */

using UnityEngine;

namespace GAF.Objects
{
	public static class GAFObjectImplsFactory
	{
		public static GAFObjectImpl getImpl(GameObject _Object, GAFObjectData _Data, Renderer _Renderer, MeshFilter _Filter)
		{
			GAFObjectImpl impl = null;
			switch (_Data.type)
			{
				case ObjectType.Simple:     impl = new GAFObjectImpl(_Object, _Data, _Renderer, _Filter); break;
				case ObjectType.Masked:     impl = new GAFMaskedObjectImpl(_Object, _Data, _Renderer, _Filter); break;
				case ObjectType.Mask:	    impl = new GAFMaskObjectImpl(_Object, _Data, _Renderer, _Filter); break;
				case ObjectType.Filtered:   impl = new GAFObjectImpl(_Object, _Data, _Renderer, _Filter); break;
				case ObjectType.Complex:    impl = new GAFMaskedObjectImpl(_Object, _Data, _Renderer, _Filter); break;
			}

			return impl;
		}
	}
}
