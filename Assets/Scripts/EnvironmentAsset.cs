using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class EnvironmentAsset : MonoBehaviour {
	
	public List<cellParticlesStruct> cellsParticlesDamage;
	public List<cellParticlesStruct> cellParticles;

	public ParticleSystem separatorParticles;
	public ParticleSystem separatorParticlesDamage;

	public Cell.Sprites[] cellSprites;
	public Separator.Sprites[] separatorSprites;

	[Serializable]
	public struct cellParticlesStruct
	{
		public Cell.Type cellType;
		public ParticleSystem particles;
	}

}
