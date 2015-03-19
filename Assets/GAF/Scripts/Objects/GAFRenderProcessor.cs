/*
 * File:			gafrenderprocessor.cs
 * Version:			1.0
 * Last changed:	2014/12/15 8:15
 * Author:			Alexey_Nikitin
 * Copyright:		© GAF Media
 * Project:			UnityVS.UnityProject.CSharp
 */

using UnityEngine;

using GAF.Core;

using System.Collections.Generic;
using System.Linq;

namespace GAF.Objects
{
	public class GAFRenderProcessor
	{
		#region Enums

		[System.Flags]
		private enum MeshState
		{
			  Null	= 0
			, Setup	= 1
			, Sort	= 2
		}

		#endregion // Enums

		#region Members

		private static readonly Vector3 normalVector = new Vector3(0, 0, -1f);

		private MeshFilter			m_Filter	= null;
		private Renderer			m_Renderer	= null;
		private GAFBaseMovieClip	m_Clip		= null;

		private MeshState						m_State				= MeshState.Null;			
		private Dictionary<uint, IGAFObject>	m_Objects			= new Dictionary<uint, IGAFObject>();
		private List<IGAFObject>				m_SortedObjects		= null;

		#endregion // Members

		#region Interface

		public void init(GAFBaseMovieClip _Clip, MeshFilter _Filter, Renderer _Renderer)
		{
			m_Filter	= _Filter;
			m_Renderer	= _Renderer;
			m_Clip		= _Clip;

			m_Filter.hideFlags		= HideFlags.NotEditable;
			m_Renderer.hideFlags	= HideFlags.NotEditable;

			renderer.castShadows		= false;
			renderer.receiveShadows		= false;
			renderer.sortingLayerName	= m_Clip.settings.spriteLayerName;
			renderer.sortingOrder		= m_Clip.settings.spriteLayerValue;
		}

		public void process()
		{
			if (isStateSet(MeshState.Sort))
				sort();

			if (isStateSet(MeshState.Setup))
				setupMesh();

			m_State = MeshState.Null;
		}

		public void clear()
		{
			m_State				= MeshState.Null;
			m_Objects			= new Dictionary<uint,IGAFObject>();
			m_SortedObjects		= null;
		}

		public bool contains(uint _ID)
		{
			return m_Objects.ContainsKey(_ID);
		}

		public void add(IGAFObject _Object)
		{
			m_Objects.Add(_Object.serializedProperties.objectID, _Object);
			pushSortRequest();
		}

		public void remove(uint _ID)
		{
			m_Objects.Remove(_ID);
			pushSortRequest();
		}

		public void pushSortRequest()
		{
			m_State |= MeshState.Sort;
		}

		public void pushSetupRequest()
		{
			m_State |= MeshState.Setup;
		}

		#endregion // Interface

		#region Properties

		public MeshFilter filter
		{
			get
			{
				return m_Filter;
			}
		}

		public Renderer renderer
		{
			get
			{
				return m_Renderer;
			}
		}

		#endregion // Properties

		#region Implementation

		private bool isStateSet(MeshState _State)
		{
			return ((m_State & _State) == _State);
		}

		private void sort()
		{
			m_SortedObjects = m_Objects.Values.ToList();
			m_SortedObjects.Sort();
		}

		private void setupMesh()
		{
			if (m_Filter.sharedMesh != null)
			{
				m_Filter.sharedMesh.Clear();
			}
			else
			{
				m_Filter.sharedMesh = new Mesh();
				m_Filter.sharedMesh.name = m_Clip.name;
			}

			if (m_SortedObjects == null)
				m_SortedObjects = m_Objects.Values.ToList();

			int capacity = m_SortedObjects.Count;

			Vector3[] vertices = new Vector3[capacity * 4];
			Vector2[] uvs = new Vector2[capacity * 4];
			Color32[] colors = new Color32[capacity * 4];
			Vector4[] tangents = new Vector4[capacity * 4];
			List<int[]> triangles = new List<int[]>();
			Material[] materials = new Material[capacity];
			Vector3[] normals = new Vector3[capacity * 4];

			m_Filter.sharedMesh.subMeshCount = capacity;

			int index = 0;
			int materialIndex = 0;
			foreach (var obj in m_SortedObjects)
			{
				obj.properties.currentVertices.CopyTo(vertices, index);
				obj.properties.uvs.CopyTo(uvs, index);
				obj.properties.colors.CopyTo(colors, index);
				obj.properties.colorsShift.CopyTo(tangents, index);

				materials[materialIndex++] = obj.properties.currentMaterial;

				normals[index + 0] = normalVector;
				normals[index + 1] = normalVector;
				normals[index + 2] = normalVector;
				normals[index + 3] = normalVector;

				triangles.Add(new int[]
				{
					  2 + index
					, 0 + index
					, 1 + index
					, 3 + index
					, 0 + index
					, 2 + index
				});

				index += 4;
			}

			m_Filter.sharedMesh.vertices = vertices;
			m_Filter.sharedMesh.uv = uvs;
			m_Filter.sharedMesh.normals = normals;
			m_Filter.sharedMesh.colors32 = colors;
			m_Filter.sharedMesh.tangents = tangents;

			for (int i = 0; i < triangles.Count; i++)
			{
				m_Filter.sharedMesh.SetTriangles(triangles[i], i);
                m_Filter.sharedMesh.RecalculateBounds();
			}

			m_Renderer.sharedMaterials = materials;
		}

		#endregion // Implementation
	}
}