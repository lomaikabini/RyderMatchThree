/*
 * File:			IGAFObjectSerializedProperties.cs
 * Version:			1.0
 * Last changed:	2014/11/20 12:07
 * Author:			Alexey_Nikitin
 * Copyright:		© GAF Media
 * Project:			UnityVS.UnityProject.CSharp
 */

using UnityEngine;

using GAF.Core;
using GAF.Data;

namespace GAF.Objects
{
	public interface IGAFObjectSerializedProperties
	{
		string name { get; }

		ObjectType type { get; }

		uint objectID { get; }

		uint atlasElementID { get; }

		GAFBaseMovieClip clip { get; }

		GAFObjectsManager manager { get; }

		Vector3 localPosition { get; }

		Vector3 statePosition { get; set; }

		bool visible { get; set; }

		Vector3 offset { get; set; }

		Material material { get; set; }

		bool useCustomTextureRect { get; set; }

		Rect atlasTextureRect { get; set; }

		Vector2 meshSizeMultiplier { get; set; }
	}
}
