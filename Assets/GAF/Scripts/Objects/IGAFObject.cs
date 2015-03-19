/*
 * File:			IGAFObject.cs
 * Version:			1.0
 * Last changed:	2014/11/19 15:51
 * Author:			Alexey_Nikitin
 * Copyright:		© GAF Media
 * Project:			UnityVS.UnityProject.CSharp
 */

using UnityEngine;

using GAF.Core;
using GAF.Data;

namespace GAF.Objects
{
	[System.Flags]
	public enum ObjectType
	{
		  Simple	= 0
		, Mask		= 1
		, Masked	= 2
		, Filtered	= 4
		, Complex	= Masked | Filtered
	}

	public interface IGAFObject : System.IComparable
	{
		#region Base Methods

		void initialize(string _Name, ObjectType _Type, GAFBaseMovieClip _Player, GAFObjectsManager _Manager, uint _ObjectID, uint _AtlasElementID);

		void reload(GAFRenderProcessor _Processor);

		void updateToState(GAFObjectStateData _State, bool Refresh);

		void onDestroy();

		#endregion // Base Methods

		#region Properties

		IGAFObjectProperties properties { get; }

		IGAFObjectSerializedProperties serializedProperties { get; }

		#endregion // Properties
	}
}