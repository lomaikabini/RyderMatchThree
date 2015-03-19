/*
 * File:			GAFMovieClip.cs
 * Version:			1.0
 * Last changed:	2014/12/11 16:21
 * Author:			Alexey_Nikitin
 * Copyright:		© GAF Media
 * Project:			UnityVS.UnityProject.CSharp
 */

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;

using GAF.Assets;
using GAF.Data;
using GAF.Objects;
using GAF.Utils;

namespace GAF.Core
{
	[AddComponentMenu("GAF/GAFMovieClip")]
	[ExecuteInEditMode]
	public class GAFMovieClip :
		  GAFBaseMovieClip
		, IGAFMovieClip
	{
		#region Movie clip interface

		public void play()
		{
			m_ContiniousPlaying = true;
			setPlaying(true);
		}

		public void pause()
		{
			m_ContiniousPlaying = false;
			setPlaying(false);
		}

		public void stop()
		{
			updateToFrame(currentSequence.startFrame, true);
			m_ContiniousPlaying = false;
			setPlaying(false);
		}

		public void gotoAndStop(uint _FrameNumber)
		{
			_FrameNumber = (uint)Mathf.Clamp(
				  (int)_FrameNumber
				, (int)currentSequence.startFrame
				, (int)currentSequence.endFrame);

			updateToFrame(_FrameNumber, true);

			if (on_goto != null)
				on_goto(this);

			m_ContiniousPlaying = false;
			setPlaying(false);
		}

		public void gotoAndPlay(uint _FrameNumber)
		{
			_FrameNumber = (uint)Mathf.Clamp(
				  (int)_FrameNumber
				, (int)currentSequence.startFrame
				, (int)currentSequence.endFrame);

			updateToFrame(_FrameNumber, true);

			if (on_goto != null)
				on_goto(this);

			m_ContiniousPlaying = true;
			setPlaying(true);
		}

		public string sequenceIndexToName(uint _Index)
		{
			if (asset != null && asset.isLoaded)
			{
				var sequences = asset.getSequences(timelineID);
				return sequences[Mathf.Clamp((int)_Index, 0, sequences.Count - 1)].name;
			}
			else
			{
				return string.Empty;
			}
		}

		public uint sequenceNameToIndex(string _Name)
		{
			if (asset != null && asset.isLoaded)
			{
				int index = asset.getSequences(timelineID).FindIndex(__sequence => __sequence.name == _Name);
				return index < 0 ? uint.MaxValue : (uint)index;
			}
			else
			{
				return uint.MaxValue;
			}
		}

		public void setSequence(string _SequenceName, bool _PlayImmediately = false)
		{
			uint sequenceIndex = sequenceNameToIndex(_SequenceName);
			if (sequenceIndex != uint.MaxValue &&
				m_SequenceIndex != sequenceIndex)
			{
				m_SequenceIndex = (int)sequenceIndex;

				updateToFrame(currentSequence.startFrame, true);

				if (on_sequence_change != null)
					on_sequence_change(this);

				m_ContiniousPlaying = _PlayImmediately;
				setPlaying(_PlayImmediately);
			}
		}

		public void setDefaultSequence(bool _PlayImmediately = false)
		{
			setSequence("Default", _PlayImmediately);
		}

		public uint getCurrentSequenceIndex()
		{
			return (uint)m_SequenceIndex;
		}

		public uint getFramesCount()
		{
			return asset.getFramesCount(timelineID);
		}

		public GAFWrapMode getAnimationWrapMode()
		{
			return settings.wrapMode;
		}

		public void setAnimationWrapMode(GAFWrapMode _Mode)
		{
			settings.wrapMode = _Mode;
		}

		public bool isPlaying()
		{
			return m_IsPlaying;
		}

		public float duration()
		{
			return (currentSequence.endFrame - currentSequence.startFrame) * settings.targetSPF;
		}

		public string addTrigger(System.Action<IGAFMovieClip> _Callback, uint _FrameNumber)
		{
			if (_FrameNumber < getFramesCount())
			{
				GAFFrameEvent triggerEvent = new GAFFrameEvent(_Callback);
				if (m_FrameEvents.ContainsKey(_FrameNumber))
				{
					m_FrameEvents[_FrameNumber].Add(triggerEvent);
				}
				else
				{
					m_FrameEvents.Add(_FrameNumber, new List<GAFFrameEvent>());
					m_FrameEvents[_FrameNumber].Add(triggerEvent);
				}

				return triggerEvent.id;
			}

			return string.Empty;
		}

		public void removeTrigger(string _ID)
		{
			foreach (KeyValuePair<uint, List<GAFFrameEvent>> pair in m_FrameEvents)
			{
				pair.Value.RemoveAll(delegate(GAFFrameEvent _event)
				{
					return _event.id == _ID;
				});
			}
		}

		public void removeAllTriggers(uint _FrameNumber)
		{
			if (_FrameNumber < getFramesCount())
			{
				if (m_FrameEvents.ContainsKey(_FrameNumber))
				{
					m_FrameEvents[_FrameNumber].Clear();
				}
			}
		}

		public void removeAllTriggers()
		{
			m_FrameEvents.Clear();
		}

		public IGAFObject getObject(string _PartName)
		{
			return getObject(partNameToObjectID(_PartName));
		}

		public string objectIDToPartName(uint _ID)
		{
			if (asset != null &&
				asset.isLoaded)
			{
				var data = asset.getNamedParts(timelineID).Find(part => part.objectID == _ID);
				return data != null ? data.name : string.Empty;
			}

			return string.Empty;
		}

		public uint partNameToObjectID(string _PartName)
		{
			if (asset != null &&
				asset.isLoaded)
			{
				var data = asset.getNamedParts(timelineID).Find(part => part.name == _PartName);
				return data != null ? data.objectID : uint.MaxValue;
			}

			return uint.MaxValue;
		}

		#endregion // Movie clip interface

		#region Events

		public event System.Action<IGAFMovieClip> on_start_play;
		public event System.Action<IGAFMovieClip> on_stop_play;
		public event System.Action<IGAFMovieClip> on_goto;
		public event System.Action<IGAFMovieClip> on_sequence_change;
		public event System.Action<IGAFMovieClip> on_clear;

		#endregion // Events

		#region Behavior interface

		public override void initialize(GAFAnimationAsset _Asset, int _TimelineID, bool _BakeObjects)
		{
			if (!isInitialized)
			{
				settings.init(_Asset);

				m_IsInitialized = true;

				m_MovieClipVersion = GAFSystem.MovieClipVersion;
				m_GAFAsset = _Asset;
				m_TimelineID = _TimelineID;

				manager.initialize(this, _BakeObjects);
			}
		}

		public override void clear(bool destroyChildren = false)
		{
			if (on_clear != null)
				on_clear(this);

			base.clear(destroyChildren);

			m_FrameEvents.Clear();

			m_Stopwatch = 0.0f;
		}

		public override void reload()
		{			
			if (!System.Object.Equals(asset, null) &&
				isInitialized)
			{
				if (!asset.isLoaded)
					asset.load();

				if (asset.isLoaded)
				{
#if UNITY_EDITOR
					if (m_MovieClipVersion < GAFSystem.MovieClipVersion &&
						!EditorApplication.isPlayingOrWillChangePlaymode)
					{
						upgrade();
						EditorUtility.SetDirty(this);
					}
#endif // UNITY_EDITOR

					initResource(asset);

					if (resource != null &&
						resource.isReady)
					{
						if (m_MovieClipVersion == GAFSystem.MovieClipVersion)
						{
							m_IsLoaded = true;

							setIndividualMaterials();

							manager.reload();

							updateToFrame(getCurrentFrameNumber(), true);
						}
					}
				}
			}
		}

		#endregion // Behavior interface

		#region Properties

		public GAFSequenceData currentSequence
		{
			get
			{
				if (asset != null &&
					asset.isLoaded)
				{
					return asset.getSequences(timelineID)[(int)getCurrentSequenceIndex()];
				}

				return null;
			}
		}

		#endregion // Properties

		#region MonoBehaviour

		private void FixedUpdate()
		{
			if (asset != null &&
				asset.isLoaded &&
				m_IsLoaded &&
				isPlaying() &&
			   !settings.ignoreTimeScale)
			{
				OnUpdate(Time.deltaTime);
			}
		}

		private void Update()
		{
			if (asset != null &&
				asset.isLoaded &&
				m_IsLoaded &&
				isPlaying() &&
				settings.ignoreTimeScale)
			{
				OnUpdate(Mathf.Clamp(Time.realtimeSinceStartup - m_PreviouseUpdateTime, 0f, Time.maximumDeltaTime));
				m_PreviouseUpdateTime = Time.realtimeSinceStartup;
			}
		}

		private void Start()
		{
			if (Application.isPlaying &&
				m_IsLoaded)
			{
				m_ContiniousPlaying = settings.playAutomatically;
				setPlaying(settings.playAutomatically);
			}
		}

		private void OnDestroy()
		{
			clear(true);
		}

		private void OnApplicationFocus(bool _FocusStatus)
		{
			if ( m_IsLoaded &&
				!settings.playInBackground)
			{
				setPlaying(_FocusStatus && m_ContiniousPlaying);
			}
		}

		private void OnApplicationPause(bool _PauseStatus)
		{
			if ( m_IsLoaded &&
				!settings.playInBackground)
			{
				setPlaying(_PauseStatus);
			}
		}

		#endregion // MonoBehaviour

		#region Implementation

		private void EditorUpdate()
		{
			if (asset != null &&
				asset.isLoaded &&
				m_IsLoaded &&
				isPlaying())
			{
				OnUpdate(Mathf.Clamp(Time.realtimeSinceStartup - m_PreviouseUpdateTime, 0f, Time.maximumDeltaTime));
				m_PreviouseUpdateTime = Time.realtimeSinceStartup;
			}
		}

		private void OnUpdate(float _TimeDelta)
		{
			m_Stopwatch += _TimeDelta;

			if (m_Stopwatch >= settings.targetSPF)
			{
				int framesCount = 1;
				if (settings.perfectTiming)
				{
					m_StoredTime += m_Stopwatch - settings.targetSPF;
					if (m_StoredTime > settings.targetSPF)
					{
						int additionalFrames = Mathf.FloorToInt(m_StoredTime / settings.targetSPF);
						m_StoredTime = m_StoredTime - (additionalFrames * settings.targetSPF);
						framesCount += additionalFrames;
					}
				}

				m_Stopwatch = 0f;

				if (getCurrentFrameNumber() + framesCount > currentSequence.endFrame)
				{
					switch (settings.wrapMode)
					{
						case GAFWrapMode.Once:
							updateToFrame(currentSequence.endFrame, true);
							m_ContiniousPlaying = false;
							setPlaying(false);
							return;

						case GAFWrapMode.Loop:
							updateToFrame(currentSequence.startFrame, true);

							if (on_stop_play != null)
								on_stop_play(this);

							if (on_start_play != null)
								on_start_play(this);

							return;

						default:
							m_ContiniousPlaying = false;
							setPlaying(false);
							return;
					}
				}

				for (uint i = getCurrentFrameNumber() + 1; i <= getCurrentFrameNumber() + (uint)framesCount; i++)
				{
					if (m_FrameEvents.ContainsKey(i))
					{
						foreach (GAFFrameEvent _event in m_FrameEvents[i])
						{
							_event.trigger(this);
						}
					}
				}
				updateToFrame(getCurrentFrameNumber() + (uint)framesCount, false);
			}
		}

		private void updateToFrame(uint _FrameNumber, bool _RefreshStates)
		{
			if (m_IsInitialized && m_IsLoaded)
			{
				if (getCurrentFrameNumber() != _FrameNumber || _RefreshStates)
				{
					var states = getStates(_FrameNumber, _RefreshStates);
					if (states != null)
					{
						manager.updateToFrame(states, _RefreshStates);
					}

					m_CurrentFrameNumber = (int)_FrameNumber;
				}
			}
		}

		private void setPlaying(bool _IsPlay) 
		{
			if (m_IsPlaying != _IsPlay)
			{
				m_IsPlaying = _IsPlay;

				if (m_IsPlaying)
				{
					if (on_start_play != null)
						on_start_play(this);

					m_Stopwatch = 0.0f;
					m_PreviouseUpdateTime = 0f;
				}
				else
				{
					if (on_stop_play != null)
						on_stop_play(this);

					m_Stopwatch = 0.0f;
					m_PreviouseUpdateTime = 0f;
				}
			}
		}

		protected override void initResource(GAFAnimationAsset _Asset)
		{
			resource = _Asset.getResource(settings.scale, settings.csf);
		}

#if UNITY_EDITOR
		private void upgrade()
		{
			var _asset				= asset;
			var _timelineID			= timelineID;
			var _settings			= settings;
			var _sequenceIndex		= m_SequenceIndex;
			var _currentFrameNumber = m_CurrentFrameNumber;
			var _isOldMode			= manager.oldMode;

			clear(true);

			m_Settings				= _settings;
			m_SequenceIndex			= _sequenceIndex;
			m_CurrentFrameNumber	= _currentFrameNumber;

			initialize(_asset, _timelineID, !_isOldMode);
		}
#endif // UNITY_EDITOR

		#endregion // Implementation

		#region Members

		[HideInInspector][SerializeField] private int	m_MovieClipVersion	= 0;


		private Dictionary<uint, List<GAFFrameEvent>> m_FrameEvents = new Dictionary<uint, List<GAFFrameEvent>>();

		private bool m_IsPlaying = false;
		private float m_Stopwatch = 0f;
		private float m_StoredTime = 0f;

		private float m_PreviouseUpdateTime = 0f;

		private bool m_ContiniousPlaying = false;

		#endregion // Members

		#region Properties

		#endregion
	}
}