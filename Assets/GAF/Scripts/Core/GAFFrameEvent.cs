/*
 * File:			gafframeevent.cs
 * Version:			3.9
 * Last changed:	2014/9/8 16:30
 * Author:			Alexey_Nikitin
 * Copyright:		© Catalyst Apps
 * Project:			UnityVS.UnityProject.CSharp
 */

namespace GAF.Core
{
	public class GAFFrameEvent
	{
		#region Members

		private System.Action<IGAFMovieClip> m_Callback = null;
		private string m_ID = string.Empty;

		#endregion Members

		#region Interface

		public GAFFrameEvent(System.Action<IGAFMovieClip> _Callback)
		{
			m_Callback = _Callback;
			m_ID = System.Guid.NewGuid().ToString();
		}

		public string id
		{
			get
			{
				return m_ID;
			}
		}

		public void trigger(GAFMovieClip _Clip)
		{
			m_Callback(_Clip);
		}

		#endregion // Interface
	}
}