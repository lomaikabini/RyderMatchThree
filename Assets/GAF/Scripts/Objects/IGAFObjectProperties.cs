/*
 * File:			IGAFObjectProperties.cs
 * Version:			3.9
 * Last changed:	2014/10/2 9:28
 * Author:			Alexey_Nikitin
 * Copyright:		© Catalyst Apps
 * Project:			UnityVS.UnityProject.CSharp
 */

using UnityEngine;

using GAF.Core;
using GAF.Data;

namespace GAF.Objects
{
	public interface IGAFObjectProperties
	{
		#region Base Data

		Renderer renderer { get; }

		MeshFilter filter { get; }

		Material currentMaterial { get; }

		Texture2D texture { get; }

		Material material { get; }

		GAFAtlasData atlasData { get; }

		GAFAtlasElementData atlasElementData { get; }

		GAFTexturesData texturesData { get; }

		GAFObjectStateData currentState { get; }

		int zOrder { get; }

		Color32[] colors { get; }

		Vector4[] colorsShift { get; }

		Vector3[] initialVertices { get; }

		Vector3[] currentVertices { get; }

		Vector2[] uvs { get; }

		#endregion // Base Data
	}
}
