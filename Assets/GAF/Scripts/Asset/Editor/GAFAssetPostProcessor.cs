/*
 * File:			GAFAssetPostProcessor.cs
 * Version:			1.0
 * Last changed:	2014/12/12 13:22
 * Author:			Alexey_Nikitin
 * Copyright:		© GAF Media
 * Project:			UnityVS.UnityProject.CSharp.Editor
 */

using UnityEditor;
using UnityEngine;

using System.IO;
using System.Linq;

using GAF.Assets;
using GAF.Reader;

using GAFEditor.Tracking;
using GAFEditor.Utils;
using GAFEditor.ExternalEditor;

namespace GAFEditor.Assets
{
	public class GAFAssetPostProcessor : AssetPostprocessor
	{
		public void OnPreprocessTexture()
		{
			GAFResourceManager.preProcessTexture((TextureImporter)assetImporter);
		}

		public void OnPostprocessTexture(Texture2D _Texture)
		{
			GAFResourceManager.postProcessTexture(assetPath, (TextureImporter)assetImporter);
		}

		public override uint GetVersion()
		{
			return (uint)1;
		}

		public static void OnPostprocessAllAssets(
			  string[] importedAssets
			, string[] deletedAssets
			, string[] movedAssets
			, string[] movedFromAssetPaths)
		{
			foreach (string assetName in importedAssets)
			{
				if (assetName.EndsWith(".gaf"))
				{
					byte[] fileBytes = null;
					using (BinaryReader freader = new BinaryReader(File.OpenRead(assetName)))
					{
						fileBytes = freader.ReadBytes((int)freader.BaseStream.Length);
					}

					if (fileBytes.Length > sizeof(int))
					{
						int header = System.BitConverter.ToInt32(fileBytes.Take(4).ToArray(), 0);
						if (GAFHeader.isCorrectHeader((GAFHeader.CompressionType)header))
						{
							var path = Path.GetDirectoryName(assetName) + "/" + Path.GetFileNameWithoutExtension(assetName) + ".asset";

							var asset = AssetDatabase.LoadAssetAtPath(path, typeof(GAFAnimationAsset)) as GAFAnimationAsset;
							if (asset == null)
							{
								asset = ScriptableObject.CreateInstance<GAFAnimationAsset>();
								asset = GAFAssetUtils.saveAsset(asset, path);
							}

							asset.name = Path.GetFileNameWithoutExtension(assetName);
							asset.initialize(fileBytes, AssetDatabase.AssetPathToGUID(path));
							EditorUtility.SetDirty(asset);
							GAFResourceManager.createResources(asset);

							GAFTracking.sendAssetCreatedRequest(assetName);
						}
					}
				}
			}
		}
	}
}