/*
 * File:			GAFStencilMaskManager.cs
 * Version:			1.0
 * Last changed:	2014/10/21 15:35
 * Author:			Alexey_Nikitin
 * Copyright:		© GAF Media
 * Project:			UnityVS.UnityProject.CSharp
 */

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;

namespace GAF.Objects
{
#if UNITY_EDITOR
	[InitializeOnLoad]
#endif // UNITY_EDITOR
	public static class GAFStencilMaskManager
	{
		#region Members

		private static Dictionary<int, Dictionary<uint, IGAFMaskObjectImpl>> stencilMasks
		{
			get;
			set;
		}

		private static List<int> availableIDs
		{
			get;
			set;
		}

		#endregion // Members

		#region Interface

		static GAFStencilMaskManager()
		{
			stencilMasks = new Dictionary<int, Dictionary<uint, IGAFMaskObjectImpl>>();
			availableIDs = Enumerable.Range(1, 255).ToList();
		}

		public static int registerMask(int _ClipInstanceID, uint _ObjectID, IGAFMaskObjectImpl _Mask)
		{
			int stencilID = 0;
			if (stencilMasks.ContainsKey(_ClipInstanceID))
			{
				if (stencilMasks[_ClipInstanceID].ContainsKey(_ObjectID))
				{
					stencilID = stencilMasks[_ClipInstanceID][_ObjectID].getStencilID();
				}
				else
				{
					stencilID = availableIDs[0];
					availableIDs.RemoveAt(0);
					stencilMasks[_ClipInstanceID].Add(_ObjectID, _Mask);
				}
			}
			else
			{
				stencilMasks.Add(_ClipInstanceID, new Dictionary<uint, IGAFMaskObjectImpl>());
				stencilID = availableIDs[0];
				availableIDs.RemoveAt(0);
				stencilMasks[_ClipInstanceID].Add(_ObjectID, _Mask);
			}

			return stencilID;
		}

		public static void unregisterMask(int _ClipInstanceID, uint _ObjectID, IGAFMaskObjectImpl _Mask)
		{
			if (stencilMasks.ContainsKey(_ClipInstanceID) &&
				stencilMasks[_ClipInstanceID].ContainsKey(_ObjectID))
			{
				availableIDs.Add(_Mask.getStencilID());
				stencilMasks[_ClipInstanceID].Remove(_ObjectID);
				if (stencilMasks[_ClipInstanceID].Count == 0)
					stencilMasks.Remove(_ClipInstanceID);
			}
		}

		public static IGAFMaskObjectImpl getMask(int _ClipID, uint _MaskID)
		{
			if (stencilMasks.ContainsKey(_ClipID) &&
				stencilMasks[_ClipID].ContainsKey(_MaskID))
			{
				return stencilMasks[_ClipID][_MaskID];
			}
			else
			{
				return null;
			}
		}

		#endregion // Interface
	}
}
