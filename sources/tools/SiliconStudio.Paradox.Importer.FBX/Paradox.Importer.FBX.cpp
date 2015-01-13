// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#include "stdafx.h"
#include "../SiliconStudio.Paradox.Importer.Common/ImporterUtils.h"

#include <algorithm>
#include <string>
#include <map>

using namespace System;
using namespace System::IO;
using namespace System::Collections::Generic;
using namespace System::Runtime::InteropServices;
using namespace SiliconStudio::BuildEngine;
using namespace SiliconStudio::Core::Diagnostics;
using namespace SiliconStudio::Core::IO;
using namespace SiliconStudio::Core::Mathematics;
using namespace SiliconStudio::Core::Serialization;
using namespace SiliconStudio::Core::Serialization::Assets;
using namespace SiliconStudio::Core::Serialization::Contents;
using namespace SiliconStudio::Paradox::Assets::Materials;
using namespace SiliconStudio::Paradox::Assets::Materials::Nodes;
using namespace SiliconStudio::Paradox::DataModel;
using namespace SiliconStudio::Paradox::EntityModel;
using namespace SiliconStudio::Paradox::EntityModel::Data;
using namespace SiliconStudio::Paradox::Effects;
using namespace SiliconStudio::Paradox::Effects::Data;
using namespace SiliconStudio::Paradox::Engine;
using namespace SiliconStudio::Paradox::Engine::Data;
using namespace SiliconStudio::Paradox::Extensions;
using namespace SiliconStudio::Paradox::Graphics;
using namespace SiliconStudio::Paradox::Graphics::Data;
using namespace SiliconStudio::Paradox::Shaders;

using namespace SiliconStudio::Paradox::Importer::Common;

namespace SiliconStudio { namespace Paradox { namespace Importer { namespace FBX {
	
public ref class MaterialInstances
{
public:
	MaterialInstances()
	{
		Instances = gcnew List<MaterialInstanciation^>();
	}

	FbxSurfaceMaterial* SourceMaterial;
	List<MaterialInstanciation^>^ Instances;
	String^ MaterialsName;
};

public ref class MeshConverter
{
public:
	property bool InverseNormals;
	property bool AllowUnsignedBlendIndices;

    property Vector3 ViewDirectionForTransparentZSort;

	property TagSymbol^ TextureTagSymbol;

	Logger^ logger;

internal:
	FbxManager* lSdkManager;
	FbxImporter* lImporter;
	FbxScene* scene;
	bool polygonSwap;
	bool swapHandedness;
	bool exportedFromMaya;

	String^ inputFilename;
	String^ vfsOutputFilename;
	String^ inputPath;

	ModelData^ modelData;

	Dictionary<IntPtr, int> nodeMapping;
	List<ModelNodeDefinition> nodes;
	
	static array<Byte>^ currentBuffer;

public:
	MeshConverter(Logger^ Logger)
	{
		if(logger == nullptr)
			logger = Core::Diagnostics::GlobalLogger::GetLogger("Importer FBX");

		polygonSwap = false;
		exportedFromMaya = false;
		logger = Logger;
		lSdkManager = NULL;
		lImporter = NULL;
	}

	void Destroy()
	{
		//Marshal::FreeHGlobal((IntPtr)lFilename);
		currentBuffer = nullptr;

		// The file has been imported; we can get rid of the importer.
		lImporter->Destroy();

		// Destroy the sdk manager and all other objects it was handling.
		lSdkManager->Destroy();

		// -----------------------------------------------------
		// TODO: Workaround with FBX SDK not being multithreaded. 
		// We protect the whole usage of this class with a monitor
		//
		// Lock the whole class between Initialize/Destroy
		// -----------------------------------------------------
		System::Threading::Monitor::Exit( globalLock );
		// -----------------------------------------------------
	}

	static bool WeightGreater(const std::pair<short, float>& elem1, const std::pair<short, float>& elem2)
	{
	   return elem1.second > elem2.second;
	}

	void ProcessMesh(FbxMesh* pMesh, std::map<FbxMesh*, std::string> meshNames)
	{
		FbxVector4* controlPoints = pMesh->GetControlPoints();
		FbxGeometryElementNormal* normalElement = pMesh->GetElementNormal();

		// UV set name mapping
		std::map<std::string, int> uvElementMapping;
		std::vector<FbxGeometryElementUV*> uvElements;

		for (int i = 0; i < pMesh->GetElementUVCount(); ++i)
		{
			uvElements.push_back(pMesh->GetElementUV(i));
		}

		bool hasSkinningPosition = false;
		bool hasSkinningNormal = false;
		int totalClusterCount = 0;
		std::vector<std::vector<std::pair<short, float> > > controlPointWeights;

		List<MeshBoneDefinition>^ bones = nullptr;

		// Dump skinning information
		if (pMesh->GetDeformerCount() > 0)
		{
			if (pMesh->GetDeformerCount() != 1)
			{
				logger->Error("Multiple mesh deformers are not supported yet. Mesh '{0}' will not be properly deformed.", gcnew String(meshNames[pMesh].c_str()));
			}

			FbxDeformer* deformer = pMesh->GetDeformer(0);
			FbxDeformer::EDeformerType deformerType = deformer->GetDeformerType();
			if (deformerType == FbxDeformer::eSkin)
			{
				auto lPose = scene->GetPose(0);

				FbxSkin* skin = FbxCast<FbxSkin>(deformer);

				FbxSkin::EType lSkinningType = skin->GetSkinningType();

				controlPointWeights.resize(pMesh->GetControlPointsCount());

				bones = gcnew List<MeshBoneDefinition>();

				totalClusterCount = skin->GetClusterCount();
				for (int clusterIndex = 0 ; clusterIndex < totalClusterCount; ++clusterIndex)
				{
					FbxCluster* cluster = skin->GetCluster(clusterIndex);
					FbxNode* link = cluster->GetLink();
					FbxCluster::ELinkMode lClusterMode = cluster->GetLinkMode();
					const char* boneName = link->GetName();

					int indexCount = cluster->GetControlPointIndicesCount();
					int *indices = cluster->GetControlPointIndices();
					double *weights = cluster->GetControlPointWeights();

					FbxAMatrix lReferenceGlobalInitPosition;
					FbxAMatrix lClusterGlobalInitPosition;
					FbxAMatrix lClusterRelativeInitPosition;

					cluster->GetTransformLinkMatrix(lClusterGlobalInitPosition);
					cluster->GetTransformMatrix(lReferenceGlobalInitPosition);
					//lReferenceGlobalInitPosition *= GetGeometry(pMesh->GetNode());

					bool test = cluster->IsTransformParentSet();

					auto linkMatrix = FBXMatrixToMatrix(lClusterGlobalInitPosition);
					auto meshMatrix = FBXMatrixToMatrix(lReferenceGlobalInitPosition);

					if (swapHandedness)
					{
						logger->Warning("Bones transformation from left handed to right handed need to be checked.", nullptr, CallerInfo::Get(__FILEW__, __FUNCTIONW__, __LINE__));

						// TODO: Check if still necessary?
						//linkMatrix.M13 = -linkMatrix.M13;
						//linkMatrix.M23 = -linkMatrix.M23;
						//linkMatrix.M43 = -linkMatrix.M43;
						//linkMatrix.M31 = -linkMatrix.M31;
						//linkMatrix.M32 = -linkMatrix.M32;
						//linkMatrix.M34 = -linkMatrix.M34;
						//
						//meshMatrix.M13 = -meshMatrix.M13;
						//meshMatrix.M23 = -meshMatrix.M23;
						//meshMatrix.M43 = -meshMatrix.M43;
						//meshMatrix.M31 = -meshMatrix.M31;
						//meshMatrix.M32 = -meshMatrix.M32;
						//meshMatrix.M34 = -meshMatrix.M34;
					}

					auto nodeIndex = nodeMapping[(IntPtr)link];

					MeshBoneDefinition bone;
					bone.NodeIndex = nodeMapping[(IntPtr)link];
					bone.LinkToMeshMatrix = meshMatrix * Matrix::Invert(linkMatrix);
					bones->Add(bone);

					for (int j = 0 ; j < indexCount; j++)
					{
						int controlPointIndex = indices[j];
						controlPointWeights[controlPointIndex].push_back(std::pair<short, float>((short)clusterIndex, (float)weights[j]));
					}
				}

				// look for position/normals skinning
				if (pMesh->GetControlPointsCount() > 0)
				{
					hasSkinningPosition = true;
					hasSkinningNormal = (pMesh->GetElementNormal() != NULL);
				}

				for (int i = 0 ; i < pMesh->GetControlPointsCount(); i++)
				{
					std::sort(controlPointWeights[i].begin(), controlPointWeights[i].end(), WeightGreater);
					controlPointWeights[i].resize(4, std::pair<short, float>(0, 0.0f));
					float totalWeight = 0.0f;
					for (int j = 0; j < 4; ++j)
						totalWeight += controlPointWeights[i][j].second;
					if (totalWeight == 0.0f)
					{
						for (int j = 0; j < 4; ++j)
							controlPointWeights[i][j].second = (j == 0) ? 1.0f : 0.0f;
					}
					else
					{
						totalWeight = 1.0f / totalWeight;
						for (int j = 0; j < 4; ++j)
							controlPointWeights[i][j].second *= totalWeight;
					}
				}
			}
		}

		// Build the vertex declaration
		auto vertexElements = gcnew List<VertexElement>();

		int vertexStride = 0;
		int positionOffset = vertexStride;
		vertexElements->Add(VertexElement::Position<Vector3>(0, vertexStride));
		vertexStride += 12;

		int normalOffset = vertexStride;
		if (normalElement != NULL)
		{
			vertexElements->Add(VertexElement::Normal<Vector3>(0, vertexStride));
			vertexStride += 12;
		}

		std::vector<int> uvOffsets;
		for (int i = 0; i < (int)uvElements.size(); ++i)
		{
			uvOffsets.push_back(vertexStride);
			vertexElements->Add(VertexElement::TextureCoordinate<Vector2>(i, vertexStride));
			vertexStride += 8;
			uvElementMapping[pMesh->GetElementUV(i)->GetName()] = i;
		}

		int blendIndicesOffset = vertexStride;
		bool controlPointIndices16 = (AllowUnsignedBlendIndices && totalClusterCount > 256) || (!AllowUnsignedBlendIndices && totalClusterCount > 128);
		if (!controlPointWeights.empty())
		{
			if (controlPointIndices16)
			{
				if (AllowUnsignedBlendIndices)
				{
					vertexElements->Add(VertexElement("BLENDINDICES", 0, PixelFormat::R16G16B16A16_UInt, vertexStride));
					vertexStride += sizeof(unsigned short) * 4;
				}
				else
				{
					vertexElements->Add(VertexElement("BLENDINDICES", 0, PixelFormat::R16G16B16A16_SInt, vertexStride));
					vertexStride += sizeof(short) * 4;
				}
			}
			else
			{
				if (AllowUnsignedBlendIndices)
				{
					vertexElements->Add(VertexElement("BLENDINDICES", 0, PixelFormat::R8G8B8A8_UInt, vertexStride));
					vertexStride += sizeof(unsigned char) * 4;
				}
				else
				{
					vertexElements->Add(VertexElement("BLENDINDICES", 0, PixelFormat::R8G8B8A8_SInt, vertexStride));
					vertexStride += sizeof(char) * 4;
				}
			}
		}

		int blendWeightOffset = vertexStride;
		if (!controlPointWeights.empty())
		{
			vertexElements->Add(VertexElement("BLENDWEIGHT", 0, PixelFormat::R32G32B32A32_Float, vertexStride));
			vertexStride += sizeof(float) * 4;
		}

		int polygonCount = pMesh->GetPolygonCount();

		FbxGeometryElement::EMappingMode materialMappingMode = FbxGeometryElement::eNone;
		FbxLayerElementArrayTemplate<int>* materialIndices = NULL;

		if (pMesh->GetElementMaterial())
		{
			materialMappingMode = pMesh->GetElementMaterial()->GetMappingMode();
			materialIndices = &pMesh->GetElementMaterial()->GetIndexArray();
		}

		auto buildMeshes = gcnew List<BuildMesh^>();

		// Count polygon per materials
		for (int i = 0; i < polygonCount; i++)
		{
			int materialIndex = 0;
			if (materialMappingMode == FbxGeometryElement::eByPolygon)
			{
				materialIndex = materialIndices->GetAt(i);
			}

			// Equivalent to std::vector::resize()
			while (materialIndex >= buildMeshes->Count)
			{
				buildMeshes->Add(nullptr);
			}

			if (buildMeshes[materialIndex] == nullptr)
				buildMeshes[materialIndex] = gcnew BuildMesh();

			int polygonSize = pMesh->GetPolygonSize(i) - 2;
			if (polygonSize > 0)
				buildMeshes[materialIndex]->polygonCount += polygonSize;
		}

		// Create arrays
		for each(BuildMesh^ buildMesh in buildMeshes)
		{
			if (buildMesh == nullptr)
				continue;

			buildMesh->buffer = gcnew array<Byte>(vertexStride * buildMesh->polygonCount * 3);
		}

		// Build polygons
		int polygonVertexStartIndex = 0;
		for (int i = 0; i < polygonCount; i++)
		{
			int materialIndex = 0;
			if (materialMappingMode == FbxGeometryElement::eByPolygon)
			{
				materialIndex = materialIndices->GetAt(i);
			}

			auto buildMesh = buildMeshes[materialIndex];
			auto buffer = buildMesh->buffer;

			int polygonSize = pMesh->GetPolygonSize(i);

			for (int polygonFanIndex = 2; polygonFanIndex < polygonSize; ++polygonFanIndex)
			{
				pin_ptr<Byte> vbPointer = &buffer[buildMesh->bufferOffset];
				buildMesh->bufferOffset += vertexStride * 3;

				int vertexInPolygon[3] = { 0, polygonFanIndex - 1, polygonFanIndex };
				if (polygonSwap)
				{
					int temp = vertexInPolygon[1];
					vertexInPolygon[1] = vertexInPolygon[2];
					vertexInPolygon[2] = temp;
				}
				int controlPointIndices[3] = { pMesh->GetPolygonVertex(i, vertexInPolygon[0]), pMesh->GetPolygonVertex(i, vertexInPolygon[1]), pMesh->GetPolygonVertex(i, vertexInPolygon[2]) };

				for (int polygonFanVertex = 0; polygonFanVertex < 3; ++polygonFanVertex)
				{
					int j = vertexInPolygon[polygonFanVertex];
					int vertexIndex = polygonVertexStartIndex + j;
					int controlPointIndex = controlPointIndices[polygonFanVertex];

					FbxVector4 controlPoint = controlPoints[controlPointIndex];

					if (swapHandedness)
						controlPoint[2] = -controlPoint[2];

					((float*)(vbPointer + positionOffset))[0] = (float)controlPoint[0];
					((float*)(vbPointer + positionOffset))[1] = (float)controlPoint[1];
					((float*)(vbPointer + positionOffset))[2] = (float)controlPoint[2];

					if (normalElement != NULL)
					{
						FbxVector4 normal;
						if (normalElement->GetMappingMode() == FbxLayerElement::eByControlPoint)
						{
							int normalIndex = (normalElement->GetReferenceMode() == FbxLayerElement::eIndexToDirect)
								? normalElement->GetIndexArray().GetAt(controlPointIndex)
								: controlPointIndex;
							normal = normalElement->GetDirectArray().GetAt(normalIndex);
						}
						else if (normalElement->GetMappingMode() == FbxLayerElement::eByPolygonVertex)
						{
							int normalIndex = (normalElement->GetReferenceMode() == FbxLayerElement::eIndexToDirect)
								? normalElement->GetIndexArray().GetAt(vertexIndex)
								: vertexIndex;
							normal = normalElement->GetDirectArray().GetAt(normalIndex);
						}

						if (swapHandedness)
							normal[2] = -normal[2];
						if(InverseNormals)
							normal = - normal;

						((float*)(vbPointer + normalOffset))[0] = (float)normal[0];
						((float*)(vbPointer + normalOffset))[1] = (float)normal[1];
						((float*)(vbPointer + normalOffset))[2] = (float)normal[2];
					}

					for (int uvGroupIndex = 0; uvGroupIndex < (int)uvElements.size(); ++uvGroupIndex)
					{
						auto uvElement = uvElements[uvGroupIndex];
						FbxVector2 uv;
						if (uvElement->GetMappingMode() == FbxLayerElement::eByControlPoint)
						{
							int uvIndex = (uvElement->GetReferenceMode() == FbxLayerElement::eIndexToDirect)
								? uvElement->GetIndexArray().GetAt(controlPointIndex)
								: controlPointIndex;
							uv = uvElement->GetDirectArray().GetAt(uvIndex);
						}
						else if (uvElement->GetMappingMode() == FbxLayerElement::eByPolygonVertex)
						{
							int uvIndex = (uvElement->GetReferenceMode() == FbxLayerElement::eIndexToDirect)
								? uvElement->GetIndexArray().GetAt(vertexIndex)
								: vertexIndex;
							uv = uvElement->GetDirectArray().GetAt(uvIndex);
						}
						else
						{
							logger->Error("The texture mapping mode '{0}' is not supported yet by the FBX importer "
								+ "(currently only mapping by control point and by polygon vertex are supported). "
								+ "Texture mapping will not be correct for mesh '{1}'.", gcnew Int32(uvElement->GetMappingMode()), gcnew String(meshNames[pMesh].c_str()));
						}

						((float*)(vbPointer + uvOffsets[uvGroupIndex]))[0] = (float)uv[0];
						((float*)(vbPointer + uvOffsets[uvGroupIndex]))[1] = 1.0f - (float)uv[1];
					}

					if (!controlPointWeights.empty())
					{
						const auto& blendWeights = controlPointWeights[controlPointIndex];
						for (int i = 0; i < 4; ++i)
						{
							if (controlPointIndices16)
							{
								if (AllowUnsignedBlendIndices)
									((unsigned short*)(vbPointer + blendIndicesOffset))[i] = (unsigned short)blendWeights[i].first;
								else
									((short*)(vbPointer + blendIndicesOffset))[i] = (short)blendWeights[i].first;
							}
							else
							{
								if (AllowUnsignedBlendIndices)
									((unsigned char*)(vbPointer + blendIndicesOffset))[i] = (unsigned char)blendWeights[i].first;
								else
									((char*)(vbPointer + blendIndicesOffset))[i] = (char)blendWeights[i].first;
							}
							((float*)(vbPointer + blendWeightOffset))[i] = blendWeights[i].second;
						}
					}

					vbPointer += vertexStride;
				}
			}

			polygonVertexStartIndex += polygonSize;
		}


		// Create submeshes
		for (int i = 0; i < buildMeshes->Count; ++i)
		{
			auto buildMesh = buildMeshes[i];
			if (buildMesh == nullptr)
				continue;

			auto buffer = buildMesh->buffer;
			auto vertexBufferBinding = gcnew VertexBufferBindingData(ContentReference::Create(gcnew BufferData(BufferFlags::VertexBuffer, buffer)), gcnew VertexDeclaration(vertexElements->ToArray()), buildMesh->polygonCount * 3, 0, 0);
			
			auto drawData = gcnew MeshDrawData();
			auto vbb = gcnew List<VertexBufferBindingData^>();
			vbb->Add(vertexBufferBinding);
			drawData->VertexBuffers = vbb->ToArray();
			drawData->PrimitiveType = PrimitiveType::TriangleList;
			drawData->DrawCount = buildMesh->polygonCount * 3;

			// Generate index buffer
			// For now, if user requests 16 bits indices but it doesn't fit, it
			// won't generate an index buffer, but ideally it should just split it in multiple render calls
			IndexExtensions::GenerateIndexBuffer(drawData);
			/*if (drawData->DrawCount < 65536)
			{
				IndexExtensions::GenerateIndexBuffer(drawData);
			}
			else
			{
				logger->Warning("The index buffer could not be generated with --force-compact-indices because it would use more than 16 bits per index.", nullptr, CallerInfo::Get(__FILEW__, __FUNCTIONW__, __LINE__));
			}*/

			auto lMaterial = pMesh->GetNode()->GetMaterial(i);
		
			// Generate TNB
			if (normalElement != NULL && uvElements.size() > 0)
				TNBExtensions::GenerateTangentBinormal(drawData);

			auto meshData = gcnew MeshData();
			meshData->NodeIndex = nodeMapping[(IntPtr)pMesh->GetNode()];
			meshData->Draw = drawData;
			if (!controlPointWeights.empty())
			{
				meshData->Skinning = gcnew MeshSkinningDefinition();
				meshData->Skinning->Bones = bones->ToArray();
			}

			// Dump materials/textures
			FbxGeometryElementMaterial* lMaterialElement = pMesh->GetElementMaterial();
			if (lMaterialElement != NULL && lMaterial != NULL)
			{
				auto isTransparent = IsTransparent(lMaterial);
				bool sortTransparentMeshes = true;	// TODO transform into importer parameter
				if (isTransparent && sortTransparentMeshes)
				{
					PolySortExtensions::SortMeshPolygons(drawData, ViewDirectionForTransparentZSort);
				}
			}

			auto meshName = meshNames[pMesh];
			if (buildMeshes->Count > 1)
				meshName = meshName + "_" + std::to_string(i + 1);
			meshData->Name = gcnew String(meshName.c_str());
			
			if (hasSkinningPosition || hasSkinningNormal || totalClusterCount > 0)
			{
				meshData->Parameters = gcnew ParameterCollectionData();

				if (hasSkinningPosition)
					meshData->Parameters->Set(MaterialParameters::HasSkinningPosition, true);
				if (hasSkinningNormal)
					meshData->Parameters->Set(MaterialParameters::HasSkinningNormal, true);
				if (totalClusterCount > 0)
					meshData->Parameters->Set(MaterialParameters::SkinningBones, totalClusterCount);
			}
			modelData->Meshes->Add(meshData);
		}
	}

	// return a boolean indicating whether the built material is transparent or not
	MaterialDescription^ ProcessMeshMaterialAsset(FbxSurfaceMaterial* lMaterial, std::map<std::string, int>& uvElementMapping)
	{
		auto uvEltMappingOverride = uvElementMapping;
		std::map<FbxFileTexture*, std::string> textureMap;
		std::map<std::string, int> textureNameCount;

		auto finalMaterial = gcnew SiliconStudio::Paradox::Assets::Materials::MaterialDescription();
		
		auto phongSurface = FbxCast<FbxSurfacePhong>(lMaterial);
		auto lambertSurface = FbxCast<FbxSurfaceLambert>(lMaterial);

		{   // The diffuse color
			auto diffuseTree = GenerateSurfaceTextureTree(lMaterial, uvEltMappingOverride, textureMap, textureNameCount, FbxSurfaceMaterial::sDiffuse, FbxSurfaceMaterial::sDiffuseFactor, finalMaterial);
			if(lambertSurface || diffuseTree != nullptr)
			{
				if(diffuseTree == nullptr)	
				{
					auto diffuseColor = lambertSurface->Diffuse.Get();
					auto diffuseFactor = lambertSurface->DiffuseFactor.Get();
					auto diffuseColorValue = diffuseFactor * diffuseColor;

					// Create diffuse value even if the color is black
					diffuseTree = gcnew MaterialColorNode(FbxDouble3ToColor4(diffuseColorValue));
					((MaterialColorNode^)diffuseTree)->Key = MaterialKeys::DiffuseColorValue;
					((MaterialColorNode^)diffuseTree)->AutoAssignKey = false;
					((MaterialColorNode^)diffuseTree)->IsReducible = false;
				}

				if(diffuseTree != nullptr)
					finalMaterial->AddColorNode(MaterialParameters::AlbedoDiffuse, "diffuse", diffuseTree);
			}
		}
		{   // The emissive color
			auto emissiveTree = GenerateSurfaceTextureTree(lMaterial, uvEltMappingOverride, textureMap, textureNameCount, FbxSurfaceMaterial::sEmissive, FbxSurfaceMaterial::sEmissiveFactor, finalMaterial);
			if(lambertSurface || emissiveTree != nullptr)
			{
				if(emissiveTree == nullptr)	
				{
					auto emissiveColor = lambertSurface->Emissive.Get();
					auto emissiveFactor = lambertSurface->EmissiveFactor.Get();
					auto emissiveColorValue = emissiveFactor * emissiveColor;

					// Do not create the node if the value has not been explicitly specified by the user.
					if(emissiveColorValue != FbxDouble3(0))
					{
						emissiveTree = gcnew MaterialColorNode(FbxDouble3ToColor4(emissiveColorValue));
						((MaterialColorNode^)emissiveTree)->Key = MaterialKeys::EmissiveColorValue;
						((MaterialColorNode^)emissiveTree)->AutoAssignKey = false;
						((MaterialColorNode^)emissiveTree)->IsReducible = false;
					}
				}

				if(emissiveTree != nullptr)
					finalMaterial->AddColorNode(MaterialParameters::EmissiveMap, "emissive", emissiveTree);
			}
		}
		{   // The ambient color
			auto ambientTree = GenerateSurfaceTextureTree(lMaterial, uvEltMappingOverride, textureMap, textureNameCount, FbxSurfaceMaterial::sAmbient, FbxSurfaceMaterial::sAmbientFactor, finalMaterial);
			if(lambertSurface || ambientTree != nullptr)
			{
				if(ambientTree == nullptr)	
				{
					auto ambientColor = lambertSurface->Emissive.Get();
					auto ambientFactor = lambertSurface->EmissiveFactor.Get();
					auto ambientColorValue = ambientFactor * ambientColor;

					// Do not create the node if the value has not been explicitly specified by the user.
					if(ambientColorValue != FbxDouble3(0))
					{
						ambientTree = gcnew MaterialColorNode(FbxDouble3ToColor4(ambientColorValue));
						((MaterialColorNode^)ambientTree)->Key = MaterialKeys::AmbientColorValue;
						((MaterialColorNode^)ambientTree)->AutoAssignKey = false;
						((MaterialColorNode^)ambientTree)->IsReducible = false;
					}
				}

				if(ambientTree != nullptr)
					finalMaterial->AddColorNode(MaterialParameters::AmbientMap, "ambient", ambientTree);
			}
		}
		{   // The normal map
			auto normalMapTree = GenerateSurfaceTextureTree(lMaterial, uvEltMappingOverride, textureMap, textureNameCount, FbxSurfaceMaterial::sNormalMap, NULL, finalMaterial);
			if(lambertSurface || normalMapTree != nullptr)
			{
				if(normalMapTree == nullptr)	
				{
					auto normalMapValue = lambertSurface->NormalMap.Get();

					// Do not create the node if the value has not been explicitly specified by the user.
					if(normalMapValue != FbxDouble3(0))
					{
						normalMapTree = gcnew MaterialFloat4Node(FbxDouble3ToVector4(normalMapValue));
						((MaterialFloat4Node^)normalMapTree)->Key = MaterialKeys::NormalMapValue;
						((MaterialFloat4Node^)normalMapTree)->AutoAssignKey = false;
						((MaterialFloat4Node^)normalMapTree)->IsReducible = false;
					}
				}
				
				if(normalMapTree != nullptr)
					finalMaterial->AddColorNode(MaterialParameters::NormalMap, "normalMap", normalMapTree);
			}
		}
		{   // The bump map
			auto bumpMapTree = GenerateSurfaceTextureTree(lMaterial, uvEltMappingOverride, textureMap, textureNameCount, FbxSurfaceMaterial::sBump, FbxSurfaceMaterial::sBumpFactor, finalMaterial);
			if(lambertSurface || bumpMapTree != nullptr)
			{
				if(bumpMapTree == nullptr)	
				{
					auto bumpValue = lambertSurface->Bump.Get();
					auto bumpFactor = lambertSurface->BumpFactor.Get();
					auto bumpMapValue = bumpFactor * bumpValue;

					// Do not create the node if the value has not been explicitly specified by the user.
					if(bumpMapValue != FbxDouble3(0))
					{
						bumpMapTree = gcnew MaterialFloat4Node(FbxDouble3ToVector4(bumpMapValue));
						((MaterialFloat4Node^)bumpMapTree)->Key = MaterialKeys::BumpValue;
						((MaterialFloat4Node^)bumpMapTree)->AutoAssignKey = false;
						((MaterialFloat4Node^)bumpMapTree)->IsReducible = false;
					}
				}
				
				if(bumpMapTree != nullptr)
					finalMaterial->AddColorNode(MaterialParameters::BumpMap, "bumpMap", bumpMapTree);
			}
		}
		{   // The transparency
			auto transparencyTree = GenerateSurfaceTextureTree(lMaterial, uvEltMappingOverride, textureMap, textureNameCount, FbxSurfaceMaterial::sTransparentColor, FbxSurfaceMaterial::sTransparencyFactor, finalMaterial);
			if(lambertSurface || transparencyTree != nullptr)
			{
				if(transparencyTree == nullptr)	
				{
					auto transparencyColor = lambertSurface->TransparentColor.Get();
					auto transparencyFactor = lambertSurface->TransparencyFactor.Get();
					auto transparencyValue = transparencyFactor * transparencyColor;
					auto opacityValue = std::min(1.0f, std::max(0.0f, 1-(float)transparencyValue[0]));

					// Do not create the node if the value has not been explicitly specified by the user.
					if(opacityValue < 1)
					{
						transparencyTree = gcnew MaterialFloatNode(opacityValue);
						((MaterialFloatNode^)transparencyTree)->Key = MaterialKeys::TransparencyValue;
						((MaterialFloatNode^)transparencyTree)->AutoAssignKey = false;
						((MaterialFloatNode^)transparencyTree)->IsReducible = false;
					}
				}

				if(transparencyTree != nullptr)
					finalMaterial->AddColorNode(MaterialParameters::TransparencyMap, "transparencyMap", transparencyTree);
			}
		}
		{   // The displacement map
			auto displacementColorTree = GenerateSurfaceTextureTree(lMaterial, uvEltMappingOverride, textureMap, textureNameCount, FbxSurfaceMaterial::sDisplacementColor, FbxSurfaceMaterial::sDisplacementFactor, finalMaterial);
			if(lambertSurface || displacementColorTree != nullptr)
			{
				if(displacementColorTree == nullptr)	
				{
					auto displacementColor = lambertSurface->DisplacementColor.Get();
					auto displacementFactor = lambertSurface->DisplacementFactor.Get();
					auto displacementValue = displacementFactor * displacementColor;

					// Do not create the node if the value has not been explicitly specified by the user.
					if(displacementValue != FbxDouble3(0))
					{
						displacementColorTree = gcnew MaterialFloat4Node(FbxDouble3ToVector4(displacementValue));
						((MaterialFloat4Node^)displacementColorTree)->Key = MaterialKeys::DisplacementValue;
						((MaterialFloat4Node^)displacementColorTree)->AutoAssignKey = false;
						((MaterialFloat4Node^)displacementColorTree)->IsReducible = false;
					}
				}
				
				if(displacementColorTree != nullptr)
					finalMaterial->AddColorNode(MaterialParameters::DisplacementMap, "displacementMap", displacementColorTree);
			}
		}
		{	// The specular color
			auto specularTree = GenerateSurfaceTextureTree(lMaterial, uvEltMappingOverride, textureMap, textureNameCount, FbxSurfaceMaterial::sSpecular, NULL, finalMaterial);
			if(phongSurface || specularTree != nullptr)
			{
				if(specularTree == nullptr)	
				{
					auto specularColor = phongSurface->Specular.Get();
		
					// Do not create the node if the value has not been explicitly specified by the user.
					if(specularColor != FbxDouble3(0))
					{
						specularTree = gcnew MaterialColorNode(FbxDouble3ToColor4(specularColor));
						((MaterialColorNode^)specularTree)->Key = MaterialKeys::SpecularColorValue;
						((MaterialColorNode^)specularTree)->AutoAssignKey = false;
						((MaterialColorNode^)specularTree)->IsReducible = false;
					}
				}
						
				if(specularTree != nullptr)	
					finalMaterial->AddColorNode(MaterialParameters::AlbedoSpecular, "specular", specularTree);
			}
		}
		{	// The specular intensity map
			auto specularIntensityTree = GenerateSurfaceTextureTree(lMaterial, uvEltMappingOverride, textureMap, textureNameCount, FbxSurfaceMaterial::sSpecularFactor, NULL, finalMaterial);
			if(phongSurface || specularIntensityTree != nullptr)
			{
				if(specularIntensityTree == nullptr)	
				{
					auto specularIntensity = phongSurface->SpecularFactor.Get();
		
					// Do not create the node if the value has not been explicitly specified by the user.
					if(specularIntensity > 0)
					{
						specularIntensityTree = gcnew MaterialFloatNode((float)specularIntensity);
						((MaterialFloatNode^)specularIntensityTree)->Key = MaterialKeys::SpecularIntensity;
						((MaterialFloatNode^)specularIntensityTree)->AutoAssignKey = false;
						((MaterialFloatNode^)specularIntensityTree)->IsReducible = false;
					}
				}
						
				if(specularIntensityTree != nullptr)		
					finalMaterial->AddColorNode(MaterialParameters::SpecularIntensityMap, "specularIntensity", specularIntensityTree);
			}
		}
		{	// The specular power map
			auto specularPowerTree = GenerateSurfaceTextureTree(lMaterial, uvEltMappingOverride, textureMap, textureNameCount, FbxSurfaceMaterial::sShininess, NULL, finalMaterial);
			if(phongSurface || specularPowerTree != nullptr)
			{
				if(specularPowerTree == nullptr)	
				{
					auto specularPower = phongSurface->Shininess.Get();
		
					// Do not create the node if the value has not been explicitly specified by the user.
					if(specularPower > 0)
					{
						specularPowerTree = gcnew MaterialFloatNode((float)specularPower);
						((MaterialFloatNode^)specularPowerTree)->Key = MaterialKeys::SpecularPower;
						((MaterialFloatNode^)specularPowerTree)->AutoAssignKey = false;
						((MaterialFloatNode^)specularPowerTree)->IsReducible = false;
					}
				}
						
				if(specularPowerTree != nullptr)		
					finalMaterial->AddColorNode(MaterialParameters::SpecularPowerMap, "specularPower", specularPowerTree);
			}
		}
		{   // The reflection map
			auto reflectionMapTree = GenerateSurfaceTextureTree(lMaterial, uvEltMappingOverride, textureMap, textureNameCount, FbxSurfaceMaterial::sReflection, FbxSurfaceMaterial::sReflectionFactor, finalMaterial);
			if(phongSurface || reflectionMapTree != nullptr)
			{
				if(reflectionMapTree == nullptr)	
				{
					auto reflectionColor = lambertSurface->DisplacementColor.Get();
					auto reflectionFactor = lambertSurface->DisplacementFactor.Get();
					auto reflectionValue = reflectionFactor * reflectionColor;

					// Do not create the node if the value has not been explicitly specified by the user.
					if(reflectionValue != FbxDouble3(0))
					{
						reflectionMapTree = gcnew MaterialColorNode(FbxDouble3ToColor4(reflectionValue));
						((MaterialColorNode^)reflectionMapTree)->Key = MaterialKeys::ReflectionColorValue;
						((MaterialColorNode^)reflectionMapTree)->AutoAssignKey = false;
						((MaterialColorNode^)reflectionMapTree)->IsReducible = false;
					}
				}
				
				if(reflectionMapTree != nullptr)
					finalMaterial->AddColorNode(MaterialParameters::ReflectionMap, "reflectionMap", reflectionMapTree);
			}
		}
		return finalMaterial;
	}

	bool IsTransparent(FbxSurfaceMaterial* lMaterial)
	{
		for (int i = 0; i < 2; ++i)
		{
			auto propertyName = i == 0 ? FbxSurfaceMaterial::sTransparentColor : FbxSurfaceMaterial::sTransparencyFactor;
			if (propertyName == NULL)
				continue;

			FbxProperty lProperty = lMaterial->FindProperty(propertyName);
			if (lProperty.IsValid())
			{
				const int lTextureCount = lProperty.GetSrcObjectCount<FbxTexture>();
				for (int j = 0; j < lTextureCount; ++j)
				{
					FbxLayeredTexture *lLayeredTexture = FbxCast<FbxLayeredTexture>(lProperty.GetSrcObject<FbxTexture>(j));
					FbxFileTexture *lFileTexture = FbxCast<FbxFileTexture>(lProperty.GetSrcObject<FbxTexture>(j));
					if (lLayeredTexture)
					{
						int lNbTextures = lLayeredTexture->GetSrcObjectCount<FbxFileTexture>();
						if (lNbTextures > 0)
							return true;
					}
					else if (lFileTexture)
						return true;
				}
				if (lTextureCount == 0)
				{
					auto val = FbxDouble3ToVector3(lProperty.Get<FbxDouble3>());
					if (val == Vector3::Zero || val != Vector3::One)
						return true;
				}
			}
		}
		return false;
	}

	IMaterialNode^ GenerateSurfaceTextureTree(	FbxSurfaceMaterial* lMaterial, std::map<std::string, int>& uvElementMapping, std::map<FbxFileTexture*, std::string>& textureMap, 
												std::map<std::string, int>& textureNameCount, char const* surfaceMaterial, char const* surfaceMaterialFactor,
												SiliconStudio::Paradox::Assets::Materials::MaterialDescription^ finalMaterial)
	{
		auto compositionTrees = gcnew cli::array<IMaterialNode^>(2);

		for (int i = 0; i < 2; ++i)
		{
			// Scan first for component name, then its factor (i.e. sDiffuse, then sDiffuseFactor)
			auto propertyName = i == 0 ? surfaceMaterial : surfaceMaterialFactor;
			if (propertyName == NULL)
				continue;

			int compositionCount = 0;
			
			FbxProperty lProperty = lMaterial->FindProperty(propertyName);
			if (lProperty.IsValid())
			{
				IMaterialNode^ previousNode = nullptr;
				const int lTextureCount = lProperty.GetSrcObjectCount<FbxTexture>();
				for (int j = 0; j < lTextureCount; ++j)
				{
					FbxLayeredTexture *lLayeredTexture = FbxCast<FbxLayeredTexture>(lProperty.GetSrcObject<FbxTexture>(j));
					FbxFileTexture *lFileTexture = FbxCast<FbxFileTexture>(lProperty.GetSrcObject<FbxTexture>(j));
					if (lLayeredTexture)
					{
						int lNbTextures = lLayeredTexture->GetSrcObjectCount<FbxFileTexture>();
						for (int k = 0; k < lNbTextures; ++k)
						{
							FbxFileTexture* lSubTexture = FbxCast<FbxFileTexture>(lLayeredTexture->GetSrcObject<FbxFileTexture>(k));

							auto uvName = std::string(lSubTexture->UVSet.Get());
							if (uvElementMapping.find(uvName) == uvElementMapping.end())
								uvElementMapping[uvName] = uvElementMapping.size();

							auto currentMaterialReference = GenerateMaterialTextureNodeFBX(lSubTexture, uvElementMapping, textureMap, textureNameCount, finalMaterial);
							
							if (lNbTextures == 1 || compositionCount == 0)
							{
								if (previousNode == nullptr)
									previousNode = currentMaterialReference;
								else
									previousNode = gcnew MaterialBinaryNode(previousNode, currentMaterialReference, MaterialBinaryOperand::Add); // not sure
							}
							else
							{
								auto newNode = gcnew MaterialBinaryNode(previousNode, currentMaterialReference, MaterialBinaryOperand::Add);
								previousNode = newNode;
								
								FbxLayeredTexture::EBlendMode blendMode;
								lLayeredTexture->GetTextureBlendMode(k, blendMode);
								newNode->Operand = BlendModeToBlendOperand(blendMode);								
							}

							compositionCount++;
						}
					}
					else if (lFileTexture)
					{
						compositionCount++;

						auto newMaterialReference = GenerateMaterialTextureNodeFBX(lFileTexture, uvElementMapping, textureMap, textureNameCount, finalMaterial);
						
						if (previousNode == nullptr)
							previousNode = newMaterialReference;
						else
							previousNode = gcnew MaterialBinaryNode(previousNode, newMaterialReference, MaterialBinaryOperand::Add); // not sure
					}
				}

				compositionTrees[i] = previousNode;
			}
		}

		// If we only have one of either Color or Factor, use directly, otherwise multiply them together
		IMaterialNode^ compositionTree;
		if (compositionTrees[0] == nullptr) // TODO do we want only the factor??? -> delete
		{
			compositionTree = compositionTrees[1];
		}
		else if (compositionTrees[1] == nullptr)
		{
			compositionTree = compositionTrees[0];
		}
		else
		{
			compositionTree = gcnew MaterialBinaryNode(compositionTrees[0], compositionTrees[1], MaterialBinaryOperand::Multiply);
		}

		return compositionTree;
	}

	MaterialBinaryOperand BlendModeToBlendOperand(FbxLayeredTexture::EBlendMode blendMode)
	{
		switch (blendMode)
		{
		case FbxLayeredTexture::eOver:
			return MaterialBinaryOperand::Over;
		case FbxLayeredTexture::eAdditive:
			return MaterialBinaryOperand::Add;
		case FbxLayeredTexture::eModulate:
			return MaterialBinaryOperand::Multiply;
		//case FbxLayeredTexture::eTranslucent:
		//	return MaterialBinaryOperand::Multiply;
		//case FbxLayeredTexture::eModulate2:
		//	return MaterialBinaryOperand::Multiply;
		//case FbxLayeredTexture::eNormal:
		//	return MaterialBinaryOperand::Multiply;
		//case FbxLayeredTexture::eDissolve:
		//	return MaterialBinaryOperand::Multiply;
		case FbxLayeredTexture::eDarken:
			return MaterialBinaryOperand::Darken;
		case FbxLayeredTexture::eColorBurn:
			return MaterialBinaryOperand::ColorBurn;
		case FbxLayeredTexture::eLinearBurn:
			return MaterialBinaryOperand::LinearBurn;
		//case FbxLayeredTexture::eDarkerColor:
		//	return MaterialBinaryOperand::Multiply;
		case FbxLayeredTexture::eLighten:
			return MaterialBinaryOperand::Lighten;
		case FbxLayeredTexture::eScreen:
			return MaterialBinaryOperand::Screen;
		case FbxLayeredTexture::eColorDodge:
			return MaterialBinaryOperand::ColorDodge;
		case FbxLayeredTexture::eLinearDodge:
			return MaterialBinaryOperand::LinearDodge;
		//case FbxLayeredTexture::eLighterColor:
		//	return MaterialBinaryOperand::Multiply;
		case FbxLayeredTexture::eSoftLight:
			return MaterialBinaryOperand::SoftLight;
		case FbxLayeredTexture::eHardLight:
			return MaterialBinaryOperand::HardLight;
		//case FbxLayeredTexture::eVividLight:
		//	return MaterialBinaryOperand::Multiply;
		//case FbxLayeredTexture::eLinearLight:
		//	return MaterialBinaryOperand::Multiply;
		case FbxLayeredTexture::ePinLight:
			return MaterialBinaryOperand::PinLight;
		case FbxLayeredTexture::eHardMix:
			return MaterialBinaryOperand::HardMix;
		case FbxLayeredTexture::eDifference:
			return MaterialBinaryOperand::Difference;
		case FbxLayeredTexture::eExclusion:
			return MaterialBinaryOperand::Exclusion;
		case FbxLayeredTexture::eSubtract:
			return MaterialBinaryOperand::Subtract;
		case FbxLayeredTexture::eDivide:
			return MaterialBinaryOperand::Divide;
		case FbxLayeredTexture::eHue:
			return MaterialBinaryOperand::Hue;
		case FbxLayeredTexture::eSaturation:
			return MaterialBinaryOperand::Saturation;
		//case FbxLayeredTexture::eColor:
		//	return MaterialBinaryOperand::Multiply;
		//case FbxLayeredTexture::eLuminosity:
		//	return MaterialBinaryOperand::Multiply;
		case FbxLayeredTexture::eOverlay:
			return MaterialBinaryOperand::Overlay;
		default:
			logger->Error("Material blending mode '{0}' is not supported yet. Multiplying blending mode will be used instead.", gcnew Int32(blendMode));
			return MaterialBinaryOperand::Multiply;
		}
	}

	ShaderClassSource^ GenerateTextureLayerFBX(FbxFileTexture* lFileTexture, std::map<std::string, int>& uvElementMapping, MeshData^ meshData, int& textureCount, ParameterKey<Texture^>^ surfaceMaterialKey)
	{
		auto texScale = lFileTexture->GetUVScaling();
		auto texturePath = FindFilePath(lFileTexture);

		return TextureLayerGenerator::GenerateTextureLayer(vfsOutputFilename, texturePath, uvElementMapping[std::string(lFileTexture->UVSet.Get())], Vector2((float)texScale[0], (float)texScale[1]) , 
									textureCount, surfaceMaterialKey,
									meshData,
									nullptr);
	}

	String^ FindFilePath(FbxFileTexture* lFileTexture)
	{		
		auto relFileName = gcnew String(lFileTexture->GetRelativeFileName());
		auto absFileName = gcnew String(lFileTexture->GetFileName());

		// First try to get the texture filename by relative path, if not valid then use absolute path
		// (According to FBX doc, resolved first by absolute name, and relative name if absolute name is not valid)
		auto fileNameToUse = Path::Combine(inputPath, relFileName);
		if(fileNameToUse->StartsWith("\\\\"))
		{
			logger->Warning("Importer detected a network address in referenced assets. This may temporary block the build if the file does not exist. [Address='{0}']", fileNameToUse);
		}
		if (!File::Exists(fileNameToUse))
		{
			fileNameToUse = absFileName;
		}

		return fileNameToUse;
	}

	MaterialReferenceNode^ GenerateMaterialTextureNodeFBX(FbxFileTexture* lFileTexture, std::map<std::string, int>& uvElementMapping, std::map<FbxFileTexture*, std::string>& textureMap, std::map<std::string, int>& textureNameCount, SiliconStudio::Paradox::Assets::Materials::MaterialDescription^ finalMaterial)
	{
		auto texScale = lFileTexture->GetUVScaling();		
		auto texturePath = FindFilePath(lFileTexture);
		auto wrapModeU = lFileTexture->GetWrapModeU();
		auto wrapModeV = lFileTexture->GetWrapModeV();
		bool wrapTextureU = (wrapModeU == FbxTexture::EWrapMode::eRepeat);
		bool wrapTextureV = (wrapModeV == FbxTexture::EWrapMode::eRepeat);
		
		auto index = textureMap.find(lFileTexture);
		if (index != textureMap.end())
		{
			auto textureName = textureMap[lFileTexture];
			auto materialReference = gcnew MaterialReferenceNode(gcnew String(textureName.c_str()));
			return materialReference;
		}
		else
		{
			auto textureValue = TextureLayerGenerator::GenerateMaterialTextureNode(vfsOutputFilename, texturePath, uvElementMapping[std::string(lFileTexture->UVSet.Get())], Vector2((float)texScale[0], (float)texScale[1]), wrapTextureU, wrapTextureV, nullptr);

			auto textureNamePtr = Marshal::StringToHGlobalAnsi(textureValue->TextureName);
			std::string textureName = std::string((char*)textureNamePtr.ToPointer());
			Marshal:: FreeHGlobal(textureNamePtr);

			auto textureCount = GetTextureNameCount(textureNameCount, textureName);
			if (textureCount > 1)
				textureName = textureName + "_" + std::to_string(textureCount - 1);

			auto referenceName = gcnew String(textureName.c_str());
			auto materialReference = gcnew MaterialReferenceNode(referenceName);
			finalMaterial->AddNode(referenceName, textureValue);
			textureMap[lFileTexture] = textureName;
			return materialReference;
		}
		
		return nullptr;
	}

	int GetTextureNameCount(std::map<std::string, int>& textureNameCount, std::string textureName)
	{
		auto textureFound = textureNameCount.find(textureName);
		if (textureFound == textureNameCount.end())
			textureNameCount[textureName] = 1;
		else
			textureNameCount[textureName] = textureNameCount[textureName] + 1;
		return textureNameCount[textureName];
	}

	void ProcessCamera(List<CameraInfo^>^ cameras, FbxNode* pNode, FbxCamera* pCamera, std::map<FbxNode*, std::string>& nodeNames)
	{
		auto cameraInfo = gcnew CameraInfo();
		auto cameraData = gcnew CameraComponentData();
		cameraInfo->Data = cameraData;

		cameraInfo->NodeName = gcnew String(nodeNames[pNode].c_str());

		if (pCamera->FilmAspectRatio.IsValid())
		{
			cameraData->AspectRatio = (float)pCamera->FilmAspectRatio.Get();
		}

		// TODO: Check vertical FOV formulas
		if (!exportedFromMaya && pCamera->FieldOfView.IsValid() && pCamera->FilmAspectRatio.IsValid()) // Only Focal Length is valid when exported from maya
		{
			auto aspectRatio = pCamera->FilmAspectRatio.Get();
			auto diagonalFov = pCamera->FieldOfView.Get();
			cameraData->VerticalFieldOfView = (float)(2.0 * Math::Atan(Math::Tan(diagonalFov * Math::PI / 180.0 / 2.0) / Math::Sqrt(1 + aspectRatio * aspectRatio)));
		}
		else if (pCamera->FocalLength.IsValid() && pCamera->FilmHeight.IsValid())
		{
			cameraData->VerticalFieldOfView = (float)FocalLengthToVerticalFov(pCamera->FilmHeight.Get(), pCamera->FocalLength.Get());
		}

		if (pNode->GetTarget() != NULL)
		{
			auto targetNodeIndex = nodeMapping[(IntPtr)pNode->GetTarget()];
			cameraInfo->TargetNodeName = nodes[targetNodeIndex].Name;
		}
		if (pCamera->NearPlane.IsValid())
		{
			cameraData->NearPlane = (float)pCamera->NearPlane.Get();
		}
		if (pCamera->FarPlane.IsValid())
		{
			cameraData->FarPlane = (float)pCamera->FarPlane.Get();
		}

		cameras->Add(cameraInfo);
	}

	void ProcessLight(List<LightInfo^>^ lights, FbxNode* pNode, FbxLight* pLight, std::map<FbxNode*, std::string>& nodeNames)
	{
		auto lightInfo = gcnew LightInfo();
		auto lightData = gcnew LightComponentData();
		lightInfo->Data = lightData;
		
		lightInfo->NodeName = gcnew String(nodeNames[pNode].c_str());

		// A FbxLight points along negative Y axis.
		lightData->LightDirection = Vector3(0.0f, -1.0f, 0.0f);

		switch (pLight->LightType.Get())
		{
		case FbxLight::ePoint:
			lightData->Deferred = true;
			lightData->Type = LightType::Point;
			lightData->DecayStart = (float)pLight->DecayStart.Get();
			// We support only spherical light (radius > 0)
			if (lightData->DecayStart == 0.0)
				return;
			break;
		case FbxLight::eDirectional:
			lightData->Deferred = false;
			lightData->Type = LightType::Directional;
			break;
		default:
			logger->Error("The light type '{0}' is not supported yet. The light will be ignored.", gcnew Int32(pLight->LightType.Get()));
			return;
		}
		auto lightColor = pLight->Color.IsValid() ? pLight->Color.Get() : FbxDouble3(1.0, 1.0, 1.0);
		lightData->Color = Color3((float)lightColor[0], (float)lightColor[1], (float)lightColor[2]);
		lightData->Intensity = (float)(pLight->Intensity.IsValid() ? pLight->Intensity.Get() * 0.01 : 1.0);
		lightData->Layers = RenderLayers::RenderLayerAll;
		lights->Add(lightInfo);
	}

	void ProcessAttribute(FbxNode* pNode, FbxNodeAttribute* pAttribute, std::map<FbxMesh*, std::string> meshNames)
	{
		if(!pAttribute) return;
 
		if (pAttribute->GetAttributeType() == FbxNodeAttribute::eMesh)
		{
			ProcessMesh((FbxMesh*)pAttribute, meshNames);
		}
	}

	void RegisterNode(FbxNode* pNode, int parentIndex, std::map<FbxNode*, std::string>& nodeNames)
	{
		auto resultNode = gcnew EntityData();
		resultNode->Components->Add(TransformationComponent::Key, gcnew TransformationComponentData());

		int currentIndex = nodes.Count;

		nodeMapping[(IntPtr)pNode] = currentIndex;

		// Create node
		ModelNodeDefinition modelNodeDefinition;
		modelNodeDefinition.ParentIndex = parentIndex;
		modelNodeDefinition.Transform.Scaling = Vector3::One;
		modelNodeDefinition.Name = gcnew String(nodeNames[pNode].c_str());
		modelNodeDefinition.Flags = ModelNodeFlags::Default;
		nodes.Add(modelNodeDefinition);

		// Recursively process the children nodes.
		for(int j = 0; j < pNode->GetChildCount(); j++)
		{
			RegisterNode(pNode->GetChild(j), currentIndex, nodeNames);
		}
	}

	void ProcessNode(FbxNode* pNode, std::map<FbxMesh*, std::string> meshNames)
	{
		auto resultNode = nodeMapping[(IntPtr)pNode];
		auto node = &modelData->Hierarchy->Nodes[resultNode];

		auto localTransform = pNode->EvaluateLocalTransform();

		auto translation = FbxDouble4ToVector4(localTransform.GetT());
		auto rotation = FbxDouble3ToVector3(localTransform.GetR()) * (float)Math::PI / 180.0f;
		auto scaling = FbxDouble3ToVector3(localTransform.GetS());

		if (swapHandedness)
		{
			translation.Z = -translation.Z;
			rotation.Y = -rotation.Y;
			rotation.X = -rotation.X;
		}

		Quaternion quatX, quatY, quatZ;

		Quaternion::RotationX(rotation.X, quatX);
		Quaternion::RotationY(rotation.Y, quatY);
		Quaternion::RotationZ(rotation.Z, quatZ);

		auto rotationQuaternion = quatX * quatY * quatZ;

		node->Transform.Translation = (Vector3)translation;
		node->Transform.Rotation = rotationQuaternion;
		node->Transform.Scaling = (Vector3)scaling;

		const char* nodeName = pNode->GetName();

		// Process the node's attributes.
		for(int i = 0; i < pNode->GetNodeAttributeCount(); i++)
			ProcessAttribute(pNode, pNode->GetNodeAttributeByIndex(i), meshNames);

		// Recursively process the children nodes.
		for(int j = 0; j < pNode->GetChildCount(); j++)
		{
			ProcessNode(pNode->GetChild(j), meshNames);
		}
	}

	ref class CurveEvaluator
	{
		FbxAnimCurve* curve;
		int index;

	public:
		CurveEvaluator(FbxAnimCurve* curve)
			: curve(curve), index(0)
		{
		}

		float Evaluate(CompressedTimeSpan time)
		{
			auto fbxTime = FbxTime((long long)time.Ticks * FBXSDK_TIME_ONE_SECOND.Get() / (long long)CompressedTimeSpan::TicksPerSecond);
			int currentIndex = index;
			auto result = curve->Evaluate(fbxTime, &currentIndex);
			index = currentIndex;

			return result;
		}
	};
	
	template <class T>
	AnimationCurve<T>^ ProcessAnimationCurveVector(AnimationClip^ animationClip, int nodeData, String^ name, int numCurves, FbxAnimCurve** curves, float maxErrorThreshold)
	{
		auto keyFrames = ProcessAnimationCurveFloatsHelper<T>(curves, numCurves);
		if (keyFrames == nullptr)
			return nullptr;

		// Add curve
		auto animationCurve = gcnew AnimationCurve<T>();

		// Switch to cubic implicit interpolation mode for Vector3
		animationCurve->InterpolationType = AnimationCurveInterpolationType::Cubic;

		// Create keys
		for (int i = 0; i < keyFrames->Count; ++i)
		{
			animationCurve->KeyFrames->Add(keyFrames[i]);
		}

		animationClip->AddCurve(name, animationCurve);
		
		if (keyFrames->Count > 0)
		{
			auto curveDuration = keyFrames[keyFrames->Count - 1].Time;
			if (animationClip->Duration < curveDuration)
				animationClip->Duration = curveDuration;
		}

		return animationCurve;
	}

	void ProcessAnimationCurveRotation(AnimationClip^ animationClip, int nodeData, String^ name, FbxAnimCurve** curves, float maxErrorThreshold)
	{
		auto keyFrames = ProcessAnimationCurveFloatsHelper<Vector3>(curves, 3);
		if (keyFrames == nullptr)
			return;

		// Convert euler angles to radians
		for (int i = 0; i < keyFrames->Count; ++i)
		{
			auto keyFrame = keyFrames[i];
			keyFrame.Value *= (float)Math::PI / 180.0f;
			keyFrames[i] = keyFrame;
		}

		// Add curve
		auto animationCurve = gcnew AnimationCurve<Quaternion>();
		
		// Create keys
		for (int i = 0; i < keyFrames->Count; ++i)
		{
			auto keyFrame = keyFrames[i];
			if (swapHandedness)
			{
				keyFrame.Value.X = -keyFrame.Value.X;
				keyFrame.Value.Y = -keyFrame.Value.Y;
			}
		
			Quaternion quatX, quatY, quatZ;

			Quaternion::RotationX(keyFrame.Value.X, quatX);
			Quaternion::RotationY(keyFrame.Value.Y, quatY);
			Quaternion::RotationZ(keyFrame.Value.Z, quatZ);

			auto rotationQuaternion = quatX * quatY * quatZ;

			KeyFrameData<Quaternion> newKeyFrame;
			newKeyFrame.Time = keyFrame.Time;
			newKeyFrame.Value = rotationQuaternion;
			animationCurve->KeyFrames->Add(newKeyFrame);
		}

		animationClip->AddCurve(name, animationCurve);

		if (keyFrames->Count > 0)
		{
			auto curveDuration = keyFrames[keyFrames->Count - 1].Time;
			if (animationClip->Duration < curveDuration)
				animationClip->Duration = curveDuration;
		}
	}

	template <typename T>
	List<KeyFrameData<T>>^ ProcessAnimationCurveFloatsHelper(FbxAnimCurve** curves, int numCurves)
	{
		FbxTime startTime = FBXSDK_TIME_INFINITE;
		FbxTime endTime = FBXSDK_TIME_MINUS_INFINITE;
		for (int i = 0; i < numCurves; ++i)
		{
			auto curve = curves[i];

			// If one of the expected channel is null, the group is skipped.
			// Ideally, we would still want to use default values
			// (i.e. in the unlikely situation where X and Y have animation channels but not Z, it should still be processed with default Z values).
			if (curve == NULL)
				return nullptr;

			FbxTimeSpan timeSpan;
			curve->GetTimeInterval(timeSpan);

			if (curve != NULL && curve->KeyGetCount() > 0)
			{
				auto firstKeyTime = curve->KeyGetTime(0);
				auto lastKeyTime = curve->KeyGetTime(curve->KeyGetCount() - 1);
				if (startTime > firstKeyTime)
					startTime = firstKeyTime;
				if (endTime < lastKeyTime)
					endTime = lastKeyTime;
			}
		}

		if (startTime == FBXSDK_TIME_INFINITE
			|| endTime == FBXSDK_TIME_MINUS_INFINITE)
		{
			// No animation
			return nullptr;
		}

		auto keyFrames = gcnew List<KeyFrameData<T>>();

		const float framerate = static_cast<float>(FbxTime::GetFrameRate(scene->GetGlobalSettings().GetTimeMode()));
		auto oneFrame = FbxTime::GetOneFrameValue(scene->GetGlobalSettings().GetTimeMode());

		// Step1: Pregenerate curve with discontinuities
		int index = 0;
		bool discontinuity = false;

		int currentKeyIndices[4];
		int currentEvaluationIndices[4];
		bool isConstant[4];
		bool hasDiscontinuity[4];

		for (int i = 0; i < numCurves; ++i)
		{
			auto curve = curves[i];
			currentKeyIndices[i] = 0;
			currentEvaluationIndices[i] = 0;
			isConstant[i] = false;
			hasDiscontinuity[i] = false;
		}

		//float values[4];
		auto key = KeyFrameData<T>();
		float* values = (float*)&key.Value;

		FbxTime time;
		bool lastFrame = false;
		for (time = startTime; time < endTime || !lastFrame; time += oneFrame)
		{
			// Last frame with time = endTime
			if (time >= endTime)
			{
				lastFrame = true;
				time = endTime;
			}

			key.Time = FBXTimeToTimeSpane(time);

			bool hasDiscontinuity = false;
			bool needUpdate = false;

			for (int i = 0; i < numCurves; ++i)
			{
				auto curve = curves[i];
				int currentIndex = currentKeyIndices[i];

				FbxAnimCurveKey curveKey;

				// Advance to appropriate key that should be active during this frame
				while (curve->KeyGetTime(currentIndex) <= time && currentIndex + 1 < curve->KeyGetCount())
				{
					++currentIndex;

					// If new key over constant, there is a discontinuity
					bool wasConstant = isConstant[i];
					hasDiscontinuity |= wasConstant;

					auto interpolation = curve->KeyGetInterpolation(currentIndex);
					isConstant[i] = interpolation == FbxAnimCurveDef::eInterpolationConstant;
				}

				currentKeyIndices[i] = currentIndex;

				// Update non-constant values
				if (!isConstant[i])
				{
					values[i] = curve->Evaluate(time, &currentEvaluationIndices[i]);
					needUpdate = true;
				}
			}

			// No need to update values, they are same as previous frame
			//if (!needUpdate && !hasDiscontinuity)
			//	continue;

			// If discontinuity, we need to add previous values twice (with updated time), and new values twice (with updated time) to ignore any implicit tangents
			if (hasDiscontinuity)
			{
				keyFrames->Add(key);
				keyFrames->Add(key);
			}

			// Update constant values
			for (int i = 0; i < numCurves; ++i)
			{
				auto curve = curves[i];
				if (isConstant[i])
					values[i] = curve->Evaluate(time, &currentEvaluationIndices[i]);
			}

			keyFrames->Add(key);
			if (hasDiscontinuity)
				keyFrames->Add(key);
		}

		return keyFrames;
	}

	void ConvertDegreeToRadians(AnimationCurve<float>^ channel)
	{
		for (int i = 0; i < channel->KeyFrames->Count; ++i)
		{
			auto keyFrame = channel->KeyFrames[i];
			keyFrame.Value *= (float)Math::PI / 180.0f;
			channel->KeyFrames[i] = keyFrame;
		}
	}

	void ReverseChannelZ(AnimationCurve<Vector3>^ channel)
	{
		// Used for handedness conversion
		for (int i = 0; i < channel->KeyFrames->Count; ++i)
		{
			auto keyFrame = channel->KeyFrames[i];
			keyFrame.Value.Z = -keyFrame.Value.Z;
			channel->KeyFrames[i] = keyFrame;
		}
	}

	void ComputeFovFromFL(AnimationCurve<float>^ channel, FbxCamera* pCamera)
	{
		// Used for handedness conversion
		for (int i = 0; i < channel->KeyFrames->Count; ++i)
		{
			auto keyFrame = channel->KeyFrames[i];
			keyFrame.Value = (float)FocalLengthToVerticalFov(pCamera->FilmHeight.Get(), keyFrame.Value);
			channel->KeyFrames[i] = keyFrame;
		}
	}

	void MultiplyChannel(AnimationCurve<float>^ channel, double factor)
	{
		// Used for handedness conversion
		for (int i = 0; i < channel->KeyFrames->Count; ++i)
		{
			auto keyFrame = channel->KeyFrames[i];
			keyFrame.Value = (float)(factor * keyFrame.Value);
			channel->KeyFrames[i] = keyFrame;
		}
	}

	void ProcessAnimation(AnimationClip^ animationClip, FbxAnimLayer* animLayer, FbxNode* node)
	{
		auto nodeData = nodeMapping[(IntPtr)node];

		// Directly interpolate matrix frame per frame (test)
		/*auto animationChannel = gcnew array<AnimationChannel^>(16);
		for (int i = 0; i < 16; ++i)
		{
			animationChannel[i] = gcnew AnimationChannel();
			animationChannel[i]->TargetNode = nodeData;
			animationChannel[i]->TargetProperty = i.ToString();
			animationData->AnimationChannels->Add(animationChannel[i]);
		}
		for (int i = 0; i < 200; ++i)
		{
			FbxTime evalTime;
			evalTime.SetMilliSeconds(10 * i);
			FbxXMatrix matrix = node->EvaluateLocalTransform(evalTime);
			for (int i = 0; i < 16; ++i)
			{
				auto key2 = KeyFrameData<float>();
				key2.Value = ((double*)matrix)[i];
				double time = (double)evalTime.Get();
				time *= (double)TimeSpan::TicksPerSecond / (double)FBXSDK_TIME_ONE_SECOND.Get();
				key2.Time = TimeSpan((long long)time);

				animationChannel[i]->Add(key2);
			}
		}
		FbxXMatrix localMatrix = node->EvaluateLocalTransform(FbxTime(0));

		FbxVector4 t = localMatrix.GetT();
		FbxVector4 r = localMatrix.GetR();*/

		auto rotationOffset = node->RotationOffset.Get();
		auto rotationPivot = node->RotationPivot.Get();
		auto quatInterpolate = node->QuaternionInterpolate.Get();
		auto rotationOrder = node->RotationOrder.Get();

		FbxAnimCurve* curves[3];

		auto nodeName = nodes[nodeData].Name;

		curves[0] = node->LclTranslation.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_X);
		curves[1] = node->LclTranslation.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_Y);
		curves[2] = node->LclTranslation.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_Z);
		auto translation = ProcessAnimationCurveVector<Vector3>(animationClip, nodeData, String::Format("Transformation.Translation[{0}]", nodeName), 3, curves, 0.005f);

		curves[0] = node->LclRotation.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_X);
		curves[1] = node->LclRotation.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_Y);
		curves[2] = node->LclRotation.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_Z);
		ProcessAnimationCurveRotation(animationClip, nodeData, String::Format("Transformation.Rotation[{0}]", nodeName), curves, 0.01f);

		curves[0] = node->LclScaling.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_X);
		curves[1] = node->LclScaling.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_Y);
		curves[2] = node->LclScaling.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_Z);
		auto scaling = ProcessAnimationCurveVector<Vector3>(animationClip, nodeData, String::Format("Transformation.Scaling[{0}]", nodeName), 3, curves, 0.005f);

		if (swapHandedness)
		{
			if (translation != nullptr)
				ReverseChannelZ(translation);
		}

		// Change Y scaling for "root" nodes, if necessary
		/*if (node == scene->GetRootNode() && scalingY != nullptr && swapHandedness == true)
		{
			ReverseChannelY(scaling);
		}*/

		FbxCamera* camera = node->GetCamera();
		if (camera != NULL)
		{
			if(camera->FieldOfViewY.GetCurve(animLayer))
			{
				curves[0] = camera->FieldOfViewY.GetCurve(animLayer);
				auto FovAnimChannel = ProcessAnimationCurveVector<float>(animationClip, nodeData, "Camera.FieldOfViewVertical", 1, curves, 0.01f);
				ConvertDegreeToRadians(FovAnimChannel);

				if(!exportedFromMaya)
					MultiplyChannel(FovAnimChannel, 0.6); // Random factor to match what we see in 3dsmax, need to check why!
			}

			
			if(camera->FocalLength.GetCurve(animLayer))
			{
				curves[0] = camera->FocalLength.GetCurve(animLayer);
				auto flAnimChannel = ProcessAnimationCurveVector<float>(animationClip, nodeData, "Camera.FieldOfViewVertical", 1, curves, 0.01f);
				ComputeFovFromFL(flAnimChannel, camera);
			}
		}

		for(int i = 0; i < node->GetChildCount(); ++i)
		{
			ProcessAnimation(animationClip, animLayer, node->GetChild(i));
		}
	}

	void SetPivotStateRecursive(FbxNode* node)
	{
		node->SetPivotState(FbxNode::eSourcePivot, FbxNode::ePivotActive);
		node->SetPivotState(FbxNode::eDestinationPivot, FbxNode::ePivotActive);

		for(int i = 0; i < node->GetChildCount(); ++i)
		{
			SetPivotStateRecursive(node->GetChild(i));
		}
	}

	AnimationClip^ ProcessAnimation(FbxScene* scene)
	{
		auto animationClip = gcnew AnimationClip();

		int animStackCount = scene->GetMemberCount<FbxAnimStack>();

		// TODO: We probably don't support more than one anim stack count.
		for (int i = 0; i < animStackCount; ++i)
		{
			FbxAnimStack* animStack = scene->GetMember<FbxAnimStack>(i);
			int animLayerCount = animStack->GetMemberCount<FbxAnimLayer>();
			FbxAnimLayer* animLayer = animStack->GetMember<FbxAnimLayer>(0);

			// From http://www.the-area.com/forum/autodesk-fbx/fbx-sdk/resetpivotsetandconvertanimation-issue/page-1/
			scene->GetRootNode()->ResetPivotSet(FbxNode::eDestinationPivot);
			SetPivotStateRecursive(scene->GetRootNode());
			scene->GetRootNode()->ConvertPivotAnimationRecursive(animStack, FbxNode::eDestinationPivot, 30.0f);

			ProcessAnimation(animationClip, animLayer, scene->GetRootNode());

			scene->GetRootNode()->ResetPivotSet(FbxNode::eSourcePivot);
		}

		if (animationClip->Curves->Count == 0)
			animationClip = nullptr;

		return animationClip;
	}

	ref class BuildMesh
	{
	public:
		array<Byte>^ buffer;
		int bufferOffset;
		int polygonCount;
	};

	ref struct ImportConfiguration
	{
	public:
		property bool ImportTemplates;
		property bool ImportPivots;
		property bool ImportGlobalSettings;
		property bool ImportCharacters;
		property bool ImportConstraints;
		property bool ImportGobos;
		property bool ImportShapes;
		property bool ImportLinks;
		property bool ImportMaterials;
		property bool ImportTextures;
		property bool ImportModels;
		property bool ImportAnimations;
		property bool ExtractEmbeddedData;

	public:
		static ImportConfiguration^ ImportAll()
		{
			auto config = gcnew ImportConfiguration();

			config->ImportTemplates = true;
			config->ImportPivots = true;
			config->ImportGlobalSettings = true;
			config->ImportCharacters = true;
			config->ImportConstraints = true;
			config->ImportGobos = true;
			config->ImportShapes = true;
			config->ImportLinks = true;
			config->ImportMaterials = true;
			config->ImportTextures = true;
			config->ImportModels = true;
			config->ImportAnimations = true;
			config->ExtractEmbeddedData = true;

			return config;
		}

		static ImportConfiguration^ ImportModelOnly()
		{
			auto config = gcnew ImportConfiguration();

			config->ImportTemplates = false;
			config->ImportPivots = false;
			config->ImportGlobalSettings = false;
			config->ImportCharacters = false;
			config->ImportConstraints = false;
			config->ImportGobos = false;
			config->ImportShapes = false;
			config->ImportLinks = false;
			config->ImportMaterials = true;
			config->ImportTextures = false;
			config->ImportModels = true;
			config->ImportAnimations = false;
			config->ExtractEmbeddedData = false;

			return config;
		}

		static ImportConfiguration^ ImportMaterialsOnly()
		{
			auto config = gcnew ImportConfiguration();

			config->ImportTemplates = false;
			config->ImportPivots = false;
			config->ImportGlobalSettings = false;
			config->ImportCharacters = false;
			config->ImportConstraints = false;
			config->ImportGobos = false;
			config->ImportShapes = false;
			config->ImportLinks = false;
			config->ImportMaterials = true;
			config->ImportTextures = false;
			config->ImportModels = false;
			config->ImportAnimations = false;
			config->ExtractEmbeddedData = false;

			return config;
		}

		static ImportConfiguration^ ImportAnimationsOnly()
		{
			auto config = gcnew ImportConfiguration();

			config->ImportTemplates = false;
			config->ImportPivots = false;
			config->ImportGlobalSettings = false;
			config->ImportCharacters = false;
			config->ImportConstraints = false;
			config->ImportGobos = false;
			config->ImportShapes = false;
			config->ImportLinks = false;
			config->ImportMaterials = false;
			config->ImportTextures = false;
			config->ImportModels = false;
			config->ImportAnimations = true;
			config->ExtractEmbeddedData = false;

			return config;
		}

		static ImportConfiguration^ ImportTexturesOnly()
		{
			auto config = gcnew ImportConfiguration();

			config->ImportTemplates = false;
			config->ImportPivots = false;
			config->ImportGlobalSettings = false;
			config->ImportCharacters = false;
			config->ImportConstraints = false;
			config->ImportGobos = false;
			config->ImportShapes = false;
			config->ImportLinks = false;
			config->ImportMaterials = false;
			config->ImportTextures = true;
			config->ImportModels = false;
			config->ImportAnimations = false;
			config->ExtractEmbeddedData = true;

			return config;
		}

		static ImportConfiguration^ ImportEntityConfig()
		{
			auto config = gcnew ImportConfiguration();

			config->ImportTemplates = false;
			config->ImportPivots = false;
			config->ImportGlobalSettings = false;
			config->ImportCharacters = false;
			config->ImportConstraints = false;
			config->ImportGobos = false;
			config->ImportShapes = false;
			config->ImportLinks = false;
			config->ImportMaterials = true;
			config->ImportTextures = true;
			config->ImportModels = true;
			config->ImportAnimations = true;
			config->ExtractEmbeddedData = true;

			return config;
		}

		static ImportConfiguration^ ImportGlobalSettingsOnly()
		{
			auto config = gcnew ImportConfiguration();

			config->ImportGlobalSettings = true;

			return config;
		}
	};

private:
	static System::Object^ globalLock = gcnew System::Object();

	void Initialize(String^ inputFilename, String^ vfsOutputFilename, ImportConfiguration^ importConfig)
	{
		// -----------------------------------------------------
		// TODO: Workaround with FBX SDK not being multithreaded. 
		// We protect the whole usage of this class with a monitor
		//
		// Lock the whole class between Initialize/Destroy
		// -----------------------------------------------------
		System::Threading::Monitor::Enter( globalLock );
		// -----------------------------------------------------

		this->inputFilename = inputFilename;
		this->vfsOutputFilename = vfsOutputFilename;
		this->inputPath = Path::GetDirectoryName(inputFilename);

		polygonSwap = false;

		// Initialize the sdk manager. This object handles all our memory management.
		lSdkManager = FbxManager::Create();

		// Create the io settings object.
		FbxIOSettings *ios = FbxIOSettings::Create(lSdkManager, IOSROOT);
		ios->SetBoolProp(IMP_FBX_TEMPLATE, importConfig->ImportTemplates);
		ios->SetBoolProp(IMP_FBX_PIVOT, importConfig->ImportPivots);
		ios->SetBoolProp(IMP_FBX_GLOBAL_SETTINGS, importConfig->ImportGlobalSettings);
		ios->SetBoolProp(IMP_FBX_CHARACTER, importConfig->ImportCharacters);
		ios->SetBoolProp(IMP_FBX_CONSTRAINT, importConfig->ImportConstraints);
		ios->SetBoolProp(IMP_FBX_GOBO, importConfig->ImportGobos);
		ios->SetBoolProp(IMP_FBX_SHAPE, importConfig->ImportShapes);
		ios->SetBoolProp(IMP_FBX_LINK, importConfig->ImportLinks);
		ios->SetBoolProp(IMP_FBX_MATERIAL, importConfig->ImportMaterials);
		ios->SetBoolProp(IMP_FBX_TEXTURE, importConfig->ImportTextures);
		ios->SetBoolProp(IMP_FBX_MODEL, importConfig->ImportModels);
		ios->SetBoolProp(IMP_FBX_ANIMATION, importConfig->ImportAnimations);
		ios->SetBoolProp(IMP_FBX_EXTRACT_EMBEDDED_DATA, importConfig->ExtractEmbeddedData);
		lSdkManager->SetIOSettings(ios);

		// Create an importer using our sdk manager.
		lImporter = FbxImporter::Create(lSdkManager,"");
    
		auto inputFilenameUtf8 = System::Text::Encoding::UTF8->GetBytes(inputFilename);
		pin_ptr<Byte> inputFilenameUtf8Ptr = &inputFilenameUtf8[0];

		if(!lImporter->Initialize((const char*)inputFilenameUtf8Ptr, -1, lSdkManager->GetIOSettings()))
		{
			throw gcnew InvalidOperationException(String::Format("Call to FbxImporter::Initialize() failed.\n"
				"Error returned: {0}\n\n", gcnew String(lImporter->GetStatus().GetErrorString())));
		}

		// Create a new scene so it can be populated by the imported file.
		scene = FbxScene::Create(lSdkManager, "myScene");

		// Import the contents of the file into the scene.
		lImporter->Import(scene);

		auto documentInfo = scene->GetDocumentInfo();
		auto appliWhichExported = gcnew String(std::string(documentInfo->Original_ApplicationName.Get()).c_str());
		if(appliWhichExported == "Maya")
			exportedFromMaya = true;
			
		const float framerate = static_cast<float>(FbxTime::GetFrameRate(scene->GetGlobalSettings().GetTimeMode()));
		scene->GetRootNode()->ResetPivotSetAndConvertAnimation(framerate, false, false);

		// For some reason ConvertScene doesn't seem to work well in some cases with no animation
		//FbxAxisSystem ourAxisSystem(FbxAxisSystem::ZAxis, (FbxAxisSystem::eFrontVector)FbxAxisSystem::ParityOdd, FbxAxisSystem::LeftHanded);
		//ourAxisSystem.ConvertScene(scene);

		auto sceneAxisSystem = scene->GetGlobalSettings().GetAxisSystem();

		swapHandedness = sceneAxisSystem.GetCoorSystem() == FbxAxisSystem::eLeftHanded;
		polygonSwap = !swapHandedness;

		// Not sure if handedness is really what requires it to be rotated by 180?
		// Maybe we should swap Y instead of swapping Z?
		auto rotationAngle = swapHandedness ? 180 : 0;

		// Y axis is up, swap some stuff! -- ConvertScene doesn't seem to take care of it, maybe only because it was a case with no animation?
		// TODO: Maybe it would be better to do it on inner nodes instead of root node?
		//int upVectorSign;
		//if (sceneAxisSystem.GetUpVector(upVectorSign) == FbxAxisSystem::eYAxis)
		//	rotationAngle += 90;

		auto sceneRootNode = scene->GetRootNode();

		// Add the root rotation
		sceneRootNode->SetRotationActive(true);
		sceneRootNode->SetPreRotation(FbxNode::eSourcePivot, FbxVector4(rotationAngle, 0, 0));

		std::map<FbxNode*, std::string> nodeNames;
		GenerateNodesName(nodeNames);

		RegisterNode(scene->GetRootNode(), -1, nodeNames);
	}

	bool CheckAnimationData(FbxAnimLayer* animLayer, FbxNode* node)
	{
		if ((node->LclTranslation.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_X) != NULL
			&& node->LclTranslation.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_Y) != NULL
			&& node->LclTranslation.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_Z) != NULL)
			||
			(node->LclRotation.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_X) != NULL
			&& node->LclRotation.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_Y) != NULL
			&& node->LclRotation.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_Z) != NULL)
			||
			(node->LclScaling.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_X) != NULL
			&& node->LclScaling.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_Y) != NULL
			&& node->LclScaling.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_Z) != NULL))
			return true;

		FbxCamera* camera = node->GetCamera();
		if (camera != NULL)
		{
			if(camera->FieldOfViewY.GetCurve(animLayer))
				return true;
			
			if(camera->FocalLength.GetCurve(animLayer))
				return true;
		}

		for(int i = 0; i < node->GetChildCount(); ++i)
		{
			if (CheckAnimationData(animLayer, node->GetChild(i)))
				return true;
		}

		return false;
	}

	bool HasAnimationData(String^ inputFile)
	{
		try
		{
			Initialize(inputFile, nullptr, ImportConfiguration::ImportAnimationsOnly());

			int animStackCount = scene->GetMemberCount<FbxAnimStack>();

			if (animStackCount > 0)
			{
				bool check = true;
				for (int i = 0; i < animStackCount && check; ++i)
				{
					FbxAnimStack* animStack = scene->GetMember<FbxAnimStack>(i);
					int animLayerCount = animStack->GetMemberCount<FbxAnimLayer>();
					FbxAnimLayer* animLayer = animStack->GetMember<FbxAnimLayer>(0);

					check = check && CheckAnimationData(animLayer, scene->GetRootNode());
				}

				return check;
			}
				
			return false;
		}
		finally
		{
			Destroy();
		}
	}

	void GetAnimationNodes(FbxAnimLayer* animLayer, FbxNode* node, List<String^>^ animationNodes)
	{
		auto nodeData = nodeMapping[(IntPtr)node];
		auto nodeName = nodes[nodeData].Name;

		bool checkTranslation = true;
		checkTranslation = checkTranslation && node->LclTranslation.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_X) != NULL;
		checkTranslation = checkTranslation && node->LclTranslation.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_Y) != NULL;
		checkTranslation = checkTranslation && node->LclTranslation.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_Z) != NULL;
		
		bool checkRotation = true;
		checkRotation = checkRotation && node->LclRotation.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_X) != NULL;
		checkRotation = checkRotation && node->LclRotation.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_Y) != NULL;
		checkRotation = checkRotation && node->LclRotation.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_Z) != NULL;
		
		bool checkScale = true;
		checkScale = checkScale && node->LclScaling.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_X) != NULL;
		checkScale = checkScale && node->LclScaling.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_Y) != NULL;
		checkScale = checkScale && node->LclScaling.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_Z) != NULL;

		if (checkTranslation || checkRotation || checkScale)
		{
			animationNodes->Add(nodeName);
		}
		else
		{
			bool checkCamera = true;
			FbxCamera* camera = node->GetCamera();
			if (camera != NULL)
			{
				if(camera->FieldOfViewY.GetCurve(animLayer))
					checkCamera = checkCamera && camera->FieldOfViewY.GetCurve(animLayer) != NULL;
			
				if(camera->FocalLength.GetCurve(animLayer))
					checkCamera = checkCamera && camera->FocalLength.GetCurve(animLayer) != NULL;

				if (checkCamera)
					animationNodes->Add(nodeName);
			}
		}

		for(int i = 0; i < node->GetChildCount(); ++i)
		{
			GetAnimationNodes(animLayer, node->GetChild(i), animationNodes);
		}
	}
	
	void GenerateMaterialNames(std::map<FbxSurfaceMaterial*, std::string>& materialNames)
	{
		auto materials = gcnew List<MaterialDescription^>();
		std::map<std::string, int> materialNameTotalCount;
		std::map<std::string, int> materialNameCurrentCount;
		std::map<FbxSurfaceMaterial*, std::string> tempNames;
		auto materialCount = scene->GetMaterialCount();
		
		for (int i = 0;  i < materialCount; i++)
		{
			auto lMaterial = scene->GetMaterial(i);
			auto materialName = std::string(lMaterial->GetName());
			auto materialPart = std::string();

			int materialNameSplitPosition = materialName.find('#');
			if (materialNameSplitPosition != std::string::npos)
			{
				materialPart = materialName.substr(materialNameSplitPosition + 1);
				materialName = materialName.substr(0, materialNameSplitPosition);
			}

			materialNameSplitPosition = materialNameSplitPosition = materialName.find("__");
			if (materialNameSplitPosition != std::string::npos)
			{
				materialPart = materialName.substr(materialNameSplitPosition + 2);
				materialName = materialName.substr(0, materialNameSplitPosition);
			}

			// remove all bad characters
			ReplaceCharacter(materialName, ':', '_');
			RemoveCharacter(materialName, ' ');
			tempNames[lMaterial] = materialName;
			
			if (materialNameTotalCount.count(materialName) == 0)
				materialNameTotalCount[materialName] = 1;
			else
				materialNameTotalCount[materialName] = materialNameTotalCount[materialName] + 1;
		}

		for (int i = 0;  i < materialCount; i++)
		{
			auto lMaterial = scene->GetMaterial(i);
			auto materialName = tempNames[lMaterial];
			int currentCount = 0;

			if (materialNameCurrentCount.count(materialName) == 0)
				materialNameCurrentCount[materialName] = 1;
			else
				materialNameCurrentCount[materialName] = materialNameCurrentCount[materialName] + 1;

			if(materialNameTotalCount[materialName] > 1)
				materialName = materialName + "_" + std::to_string(materialNameCurrentCount[materialName]);

			materialNames[lMaterial] = materialName;
		}
	}

	void GetMeshes(FbxNode* pNode, std::vector<FbxMesh*>& meshes)
	{
		// Process the node's attributes.
		for(int i = 0; i < pNode->GetNodeAttributeCount(); i++)
		{
			auto pAttribute = pNode->GetNodeAttributeByIndex(i);

			if(!pAttribute) return;
		
			if (pAttribute->GetAttributeType() == FbxNodeAttribute::eMesh)
			{
				auto pMesh = (FbxMesh*)pAttribute;
				meshes.push_back(pMesh);
			}
		}

		// Recursively process the children nodes.
		for(int j = 0; j < pNode->GetChildCount(); j++)
		{
			GetMeshes(pNode->GetChild(j), meshes);
		}
	}
	
	void GenerateMeshesName(std::map<FbxMesh*, std::string>& meshNames)
	{
		std::vector<FbxMesh*> meshes;
		GetMeshes(scene->GetRootNode(), meshes);

		std::map<std::string, int> meshNameTotalCount;
		std::map<std::string, int> meshNameCurrentCount;
		std::map<FbxMesh*, std::string> tempNames;

		for (auto iter = meshes.begin(); iter != meshes.end(); ++iter)
		{
			auto pMesh = *iter;
			auto meshName = std::string(pMesh->GetNode()->GetName());

			// remove all bad characters
			RemoveCharacter(meshName, ' ');
			tempNames[pMesh] = meshName;

			if (meshNameTotalCount.count(meshName) == 0)
				meshNameTotalCount[meshName] = 1;
			else
				meshNameTotalCount[meshName] = meshNameTotalCount[meshName] + 1;
		}

		for (auto iter = meshes.begin(); iter != meshes.end(); ++iter)
		{
			auto pMesh = *iter;
			auto meshName = tempNames[pMesh];
			int currentCount = 0;

			if (meshNameCurrentCount.count(meshName) == 0)
				meshNameCurrentCount[meshName] = 1;
			else
				meshNameCurrentCount[meshName] = meshNameCurrentCount[meshName] + 1;

			if(meshNameTotalCount[meshName] > 1)
				meshName = meshName + "_" + std::to_string(meshNameCurrentCount[meshName]);

			meshNames[pMesh] = meshName;
		}
	}

	void GetNodes(FbxNode* pNode, std::vector<FbxNode*>& nodes)
	{
		nodes.push_back(pNode);
		
		// Recursively process the children nodes.
		for(int j = 0; j < pNode->GetChildCount(); j++)
			GetNodes(pNode->GetChild(j), nodes);
	}

	void GenerateNodesName(std::map<FbxNode*, std::string>& nodeNames)
	{
		std::vector<FbxNode*> nodes;
		GetNodes(scene->GetRootNode(), nodes);

		std::map<std::string, int> nodeNameTotalCount;
		std::map<std::string, int> nodeNameCurrentCount;
		std::map<FbxNode*, std::string> tempNames;

		for (auto iter = nodes.begin(); iter != nodes.end(); ++iter)
		{
			auto pNode = *iter;
			auto nodeName = std::string(pNode->GetName());
			auto subBegin = nodeName.find_last_of(':');
			if (subBegin != std::string::npos)
				nodeName = nodeName.substr(subBegin + 1);
			tempNames[pNode] = nodeName;

			if (nodeNameTotalCount.count(nodeName) == 0)
				nodeNameTotalCount[nodeName] = 1;
			else
				nodeNameTotalCount[nodeName] = nodeNameTotalCount[nodeName] + 1;
		}

		for (auto iter = nodes.begin(); iter != nodes.end(); ++iter)
		{
			auto pNode = *iter;
			auto nodeName = tempNames[pNode];
			int currentCount = 0;

			if (nodeNameCurrentCount.count(nodeName) == 0)
				nodeNameCurrentCount[nodeName] = 1;
			else
				nodeNameCurrentCount[nodeName] = nodeNameCurrentCount[nodeName] + 1;

			if(nodeNameTotalCount[nodeName] > 1)
				nodeName = nodeName + "_" + std::to_string(nodeNameCurrentCount[nodeName]);

			nodeNames[pNode] = nodeName;
		}
	}

	MaterialInstances^ GetOrCreateInstances(FbxSurfaceMaterial* lMaterial, List<MaterialInstances^>^ instances, std::map<FbxSurfaceMaterial*, std::string>& materialNames)
	{
		for (int i = 0; i < instances->Count; ++i)
		{
			if (lMaterial == instances[i]->SourceMaterial)
				return instances[i];
		}

		auto newInstance = gcnew MaterialInstances();
		newInstance->SourceMaterial = lMaterial;
		newInstance->MaterialsName = gcnew String(materialNames[lMaterial].c_str());
		instances->Add(newInstance);
		return newInstance;
	}

	MaterialInstanciation^ GetOrCreateMaterial(FbxSurfaceMaterial* lMaterial, List<String^>^ uvNames, List<MaterialInstances^>^ instances, std::map<std::string, int>& uvElements, std::map<FbxSurfaceMaterial*, std::string>& materialNames)
	{
		auto materialInstances = GetOrCreateInstances(lMaterial, instances, materialNames);

		for (int i = 0; i < materialInstances->Instances->Count; ++i)
		{
			auto parameters = materialInstances->Instances[i]->Parameters;
			if (uvNames->Count == parameters->Count)
			{
				bool equals = true;
				for (int j = 0; j < parameters->Count; ++j)
				{
					equals = equals && (parameters[j] == uvNames[j]);
				}

				if (equals)
					return materialInstances->Instances[i];
			}
		}

		auto newInstanciation = gcnew MaterialInstanciation();
		newInstanciation->Parameters = uvNames;
		
		if (materialInstances->Instances->Count > 0)
			newInstanciation->MaterialName = materialInstances->MaterialsName + "_" + materialInstances->Instances->Count;
		else
			newInstanciation->MaterialName = materialInstances->MaterialsName;

		newInstanciation->Material = ProcessMeshMaterialAsset(lMaterial, uvElements);
		materialInstances->Instances->Add(newInstanciation);
		return newInstanciation;
	}

	void SearchMeshInAttribute(FbxNode* pNode, FbxNodeAttribute* pAttribute, std::map<FbxSurfaceMaterial*, std::string> materialNames, std::map<FbxMesh*, std::string> meshNames, std::map<FbxNode*, std::string>& nodeNames, List<MeshParameters^>^ models, List<MaterialInstances^>^ materialInstances, List<CameraInfo^>^ cameras, List<LightInfo^>^ lights)
	{
		if(!pAttribute) return;
 
		if (pAttribute->GetAttributeType() == FbxNodeAttribute::eMesh)
		{
			auto pMesh = (FbxMesh*)pAttribute;
			int polygonCount = pMesh->GetPolygonCount();
			FbxGeometryElement::EMappingMode materialMappingMode = FbxGeometryElement::eNone;
			FbxLayerElementArrayTemplate<int>* materialIndices = NULL;
			
			if (pMesh->GetElementMaterial())
			{
				materialMappingMode = pMesh->GetElementMaterial()->GetMappingMode();
				materialIndices = &pMesh->GetElementMaterial()->GetIndexArray();
			}

			auto buildMeshes = gcnew List<BuildMesh^>();

			// Count polygon per materials
			for (int i = 0; i < polygonCount; i++)
			{
				int materialIndex = 0;
				if (materialMappingMode == FbxGeometryElement::eByPolygon)
				{
					materialIndex = materialIndices->GetAt(i);
				}

				// Equivalent to std::vector::resize()
				while (materialIndex >= buildMeshes->Count)
				{
					buildMeshes->Add(nullptr);
				}

				if (buildMeshes[materialIndex] == nullptr)
					buildMeshes[materialIndex] = gcnew BuildMesh();

				int polygonSize = pMesh->GetPolygonSize(i) - 2;
				if (polygonSize > 0)
					buildMeshes[materialIndex]->polygonCount += polygonSize;
			}

			for (int i = 0; i < buildMeshes->Count; ++i)
			{
				auto meshParams = gcnew MeshParameters();
				auto meshName = meshNames[pMesh];
				if (buildMeshes->Count > 1)
					meshName = meshName + "_" + std::to_string(i + 1);
				meshParams->MeshName = gcnew String(meshName.c_str());
				meshParams->NodeName = gcnew String(nodeNames[pNode].c_str());

				FbxGeometryElementMaterial* lMaterialElement = pMesh->GetElementMaterial();
				if (lMaterialElement != NULL)
				{
					FbxSurfaceMaterial* lMaterial = pNode->GetMaterial(i);
					std::map<std::string, int> uvElements;
					auto uvNames = gcnew List<String^>();
					for (int j = 0; j < pMesh->GetElementUVCount(); ++j)
					{
						uvElements[pMesh->GetElementUV(j)->GetName()] = j;
						uvNames->Add(gcnew String(pMesh->GetElementUV(j)->GetName()));
					}

					auto material = GetOrCreateMaterial(lMaterial, uvNames, materialInstances, uvElements, materialNames);
					meshParams->MaterialName = material->MaterialName;
				}
				else
				{
					logger->Warning("Mesh {0} do not have a material. It might not be displayed.", meshParams->MeshName);
				}

				models->Add(meshParams);
			}
		}
		else if (pAttribute->GetAttributeType() == FbxNodeAttribute::eCamera)
		{
			auto pCamera = (FbxCamera*)pAttribute;
			ProcessCamera(cameras, pNode, pCamera, nodeNames);
		}
		else if (pAttribute->GetAttributeType() == FbxNodeAttribute::eLight)
		{
			auto pLight = (FbxLight*)pAttribute;
			ProcessLight(lights, pNode, pLight, nodeNames);
		}
	}

	void SearchMesh(FbxNode* pNode, std::map<FbxSurfaceMaterial*, std::string> materialNames, std::map<FbxMesh*, std::string> meshNames, std::map<FbxNode*, std::string>& nodeNames, List<MeshParameters^>^ models, List<MaterialInstances^>^ materialInstances, List<CameraInfo^>^ cameras, List<LightInfo^>^ lights)
	{
		// Process the node's attributes.
		for(int i = 0; i < pNode->GetNodeAttributeCount(); i++)
			SearchMeshInAttribute(pNode, pNode->GetNodeAttributeByIndex(i), materialNames, meshNames, nodeNames, models, materialInstances, cameras, lights);

		// Recursively process the children nodes.
		for(int j = 0; j < pNode->GetChildCount(); j++)
		{
			SearchMesh(pNode->GetChild(j), materialNames, meshNames, nodeNames, models, materialInstances, cameras, lights);
		}
	}

	Dictionary<String^, MaterialDescription^>^ ExtractMaterialsNoInit()
	{
		std::map<FbxSurfaceMaterial*, std::string> materialNames;
		GenerateMaterialNames(materialNames);

		auto materials = gcnew Dictionary<String^, MaterialDescription^>();
		for (int i = 0;  i < scene->GetMaterialCount(); i++)
		{
			std::map<std::string, int> dict;
			auto lMaterial = scene->GetMaterial(i);
			auto materialName = materialNames[lMaterial];
			materials->Add(gcnew String(materialName.c_str()), ProcessMeshMaterialAsset(lMaterial, dict));
		}
		return materials;
	}

	MeshMaterials^ ExtractModelNoInit(std::map<FbxNode*, std::string>& nodeNames)
	{
		std::map<FbxSurfaceMaterial*, std::string> materialNames;
		GenerateMaterialNames(materialNames);

		std::map<FbxMesh*, std::string> meshNames;
		GenerateMeshesName(meshNames);
			
		std::map<std::string, FbxSurfaceMaterial*> materialPerMesh;
		auto models = gcnew List<MeshParameters^>();
		auto materialInstances = gcnew List<MaterialInstances^>();
		auto cameras = gcnew List<CameraInfo^>();
		auto lights = gcnew List<LightInfo^>();
		SearchMesh(scene->GetRootNode(), materialNames, meshNames, nodeNames, models, materialInstances, cameras, lights);

		auto ret = gcnew MeshMaterials();
		ret->Models = models;
		ret->Cameras = cameras;
		ret->Lights = lights;
		ret->Materials = gcnew Dictionary<String^, MaterialDescription^>();
		for (int i = 0; i < materialInstances->Count; ++i)
		{
			for (int j = 0; j < materialInstances[i]->Instances->Count; ++j)
			{
				ret->Materials->Add(materialInstances[i]->Instances[j]->MaterialName, materialInstances[i]->Instances[j]->Material);
			}
		}
		
		// patch lights count
		int numPointLights = 0;
        int numSpotLights = 0;
        int numDirectionalLights = 0;
		for (int i = 0; i < lights->Count; ++i)
		{
			auto lightType = lights[i]->Data->Type;
			if (lightType == LightType::Point)
				++numPointLights;
			else if (lightType == LightType::Directional)
				++numDirectionalLights;
			else if (lightType == LightType::Spot)
				++numSpotLights;
		}

		for (int i = 0; i < models->Count; ++i)
		{
			models[i]->Parameters->Add(LightingKeys::MaxPointLights, numPointLights);
			models[i]->Parameters->Add(LightingKeys::MaxDirectionalLights, numDirectionalLights);
			models[i]->Parameters->Add(LightingKeys::MaxSpotLights, numSpotLights);
		}
        
		return ret;
	}

	List<String^>^ ExtractTextureDependenciesNoInit()
	{
		auto textureNames = gcnew List<String^>();
			
		auto textureCount = scene->GetTextureCount();
		for(int i=0; i<textureCount; ++i)
		{
			auto texture  = FbxCast<FbxFileTexture>(scene->GetTexture(i));

			if(texture == nullptr)
				continue;
			
			auto texturePath = FindFilePath(texture);
			if (!String::IsNullOrEmpty(texturePath)
				&& File::Exists(texturePath))
			{
				if (texturePath->Contains(".fbm\\"))
					logger->Info("Importer detected an embedded texture. It has been extracted at address '{0}'.", texturePath);
				textureNames->Add(texturePath);
			}
			else
			{
				logger->Warning("Importer detected a texture not available on disk at address '{0}'", texturePath);
			}
		}

		return textureNames;
	}

	List<String^>^ ExtractAnimationNodesNoInit()
	{
		int animStackCount = scene->GetMemberCount<FbxAnimStack>();
		List<String^>^ animationNodes = nullptr;

		if (animStackCount > 0)
		{
			animationNodes = gcnew List<String^>();
			for (int i = 0; i < animStackCount; ++i)
			{
				FbxAnimStack* animStack = scene->GetMember<FbxAnimStack>(i);
				int animLayerCount = animStack->GetMemberCount<FbxAnimLayer>();
				FbxAnimLayer* animLayer = animStack->GetMember<FbxAnimLayer>(0);
				GetAnimationNodes(animLayer, scene->GetRootNode(), animationNodes);
			}
		}

		return animationNodes;
	}

	List<String^>^ GetAllAnimationNodes(String^ inputFile)
	{
		try
		{
			Initialize(inputFile, nullptr, ImportConfiguration::ImportAnimationsOnly());
			return ExtractAnimationNodesNoInit();
		}
		finally
		{
			Destroy();
		}
		return nullptr;
	}

	List<String^>^ ExtractTextureDependencies(String^ inputFile)
	{
		try
		{
			Initialize(inputFile, nullptr, ImportConfiguration::ImportTexturesOnly());
			return ExtractTextureDependenciesNoInit();
		}
		finally
		{
			Destroy();
		}
		return nullptr;
	}

	Dictionary<String^, MaterialDescription^>^ ExtractMaterials(String^ inputFilename)
	{
		try
		{
			Initialize(inputFilename, nullptr, ImportConfiguration::ImportMaterialsOnly());
			return ExtractMaterialsNoInit();
		}
		finally
		{
			Destroy();
		}
		return nullptr;
	}

	void GetNodes(FbxNode* node, int depth, std::map<FbxNode*, std::string>& nodeNames, List<NodeInfo^>^ allNodes)
	{
		auto newNodeInfo = gcnew NodeInfo();
		newNodeInfo->Name = gcnew String(nodeNames[node].c_str());
		newNodeInfo->Depth = depth;
		newNodeInfo->Preserve = false;
		
		allNodes->Add(newNodeInfo);
		for (int i = 0; i < node->GetChildCount(); ++i)
			GetNodes(node->GetChild(i), depth + 1, nodeNames, allNodes);
	}

	List<NodeInfo^>^ ExtractNodeHierarchy(std::map<FbxNode*, std::string>& nodeNames)
	{
		auto allNodes = gcnew List<NodeInfo^>();
		GetNodes(scene->GetRootNode(), 0, nodeNames, allNodes);
		return allNodes;
	}

public:
	EntityInfo^ ExtractEntity(String^ inputFileName)
	{
		try
		{
			Initialize(inputFileName, nullptr, ImportConfiguration::ImportEntityConfig());
			
			auto index = scene->GetGlobalSettings().GetOriginalUpAxis();
			auto originalUpAxis = Vector3::Zero;
			if (index < 0 || index > 2) // Default up vector is Z
				originalUpAxis[2] = 1;
			else
				originalUpAxis[index] = 1;
			
			std::map<FbxNode*, std::string> nodeNames;
			GenerateNodesName(nodeNames);

			auto entityInfo = gcnew EntityInfo();
			entityInfo->TextureDependencies = ExtractTextureDependenciesNoInit();
			entityInfo->AnimationNodes = ExtractAnimationNodesNoInit();
			auto models = ExtractModelNoInit(nodeNames);
			entityInfo->Models = models->Models;
			entityInfo->Materials = models->Materials;
			entityInfo->Nodes = ExtractNodeHierarchy(nodeNames);
			entityInfo->Lights = models->Lights;
			entityInfo->Cameras = models->Cameras;
			entityInfo->UpAxis = originalUpAxis;

			return entityInfo;
		}
		finally
		{
			Destroy();
		}
		return nullptr;
	}

	ModelData^ Convert(String^ inputFilename, String^ vfsOutputFilename)
	{
		try
		{
			Initialize(inputFilename, vfsOutputFilename, ImportConfiguration::ImportAll());

			// Create default ModelViewData
			modelData = gcnew ModelData();
			modelData->Hierarchy = gcnew ModelViewHierarchyDefinition();
			modelData->Hierarchy->Nodes = nodes.ToArray();

			//auto sceneName = scene->GetName();
			//if (sceneName != NULL && strlen(sceneName) > 0)
			//{
			//	entity->Name = gcnew String(sceneName);
			//}
			//else
			//{
			//	// Build scene name from file name
			//	entity->Name = Path::GetFileName(this->inputFilename);
			//}

			std::map<FbxMesh*, std::string> meshNames;
			GenerateMeshesName(meshNames);

			// Process and add root entity
			ProcessNode(scene->GetRootNode(), meshNames);

			// Process animation
			//sceneData->Animation = ProcessAnimation(scene);

			return modelData;
		}
		finally
		{
			Destroy();
		}

		return nullptr;
	}

	AnimationClip^ ConvertAnimation(String^ inputFilename, String^ vfsOutputFilename)
	{
		try
		{
			Initialize(inputFilename, vfsOutputFilename, ImportConfiguration::ImportAnimationsOnly());

			// Process animation
			auto animationClip = ProcessAnimation(scene);

			return animationClip;
		}
		finally
		{
			Destroy();
		}

		return nullptr;
	}
};

} } } }
