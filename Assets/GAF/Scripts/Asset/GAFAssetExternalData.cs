/*
 * File:			GAFAssetExternalData.cs
 * Version:			1.0
 * Last changed:	2014/10/23 17:07
 * Author:			Alexey_Nikitin
 * Copyright:		© GAF Media
 * Project:			UnityVS.UnityProject.CSharp
 */

using UnityEngine;

using System.Collections.Generic;

using GAF.Objects;

namespace GAF.Data
{
	[System.Serializable]
	public class GAFAssetExternalData
	{
		#region Members

		[HideInInspector][SerializeField] private int				m_TimelineID		= -1;
		[HideInInspector][SerializeField] private List<ObjectType>	m_ObjectTypeFlags	= new List<ObjectType>();

		#endregion // Members

		#region Interface

		public GAFAssetExternalData(
			  int				_TimelineID
			, List<ObjectType>	_ObjectTypeFlags)
		{
			m_TimelineID		= _TimelineID;
			m_ObjectTypeFlags	= _ObjectTypeFlags;
		}

		#endregion // Interface

		#region Properties

		public int timelineID
		{
			get
			{
				return m_TimelineID;
			}
		}

		public List<ObjectType> objectTypeFlags
		{
			get
			{
				return m_ObjectTypeFlags;
			}
		}

		#endregion // Properties
	}
}
