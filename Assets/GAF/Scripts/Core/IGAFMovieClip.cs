/*
 * File:			IGAFMovieClip.cs
 * Version:			1.0
 * Last changed:	2014/12/3 11:04
 * Author:			Alexey_Nikitin
 * Copyright:		© GAF Media
 * Project:			UnityVS.UnityProject.CSharp
 */

using GAF.Assets;
using GAF.Objects;

namespace GAF.Core
{
	public enum GAFWrapMode
	{
		Once
	  , Loop
	}

	public interface IGAFMovieClip
	{
		void play();
		bool isPlaying();
		void pause();
		void stop();

		void gotoAndStop(uint _FrameNumber);
		void gotoAndPlay(uint _FrameNumber);

		void setSequence(string _SequenceName, bool _PlayImmediately = false);
		void setDefaultSequence(bool _PlayImmediately = false);
		string sequenceIndexToName(uint _Index);
		uint sequenceNameToIndex(string _Name);
		uint getCurrentSequenceIndex();

		uint getCurrentFrameNumber();
		uint getFramesCount();

		GAFWrapMode getAnimationWrapMode();
		void setAnimationWrapMode(GAFWrapMode _Mode);

		float duration();

		string addTrigger(System.Action<IGAFMovieClip> _Callback, uint _FrameNumber);
		void removeTrigger(string _ID);
		void removeAllTriggers(uint _FrameNumber);
		void removeAllTriggers();

		IGAFObject getObject(uint _ID);
		IGAFObject getObject(string _PartName);
		string objectIDToPartName(uint _ID);
		uint partNameToObjectID(string _PartName);
	}
}