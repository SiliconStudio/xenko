// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#pragma once

#include "stdafx.h"
#include "SceneMapping.h"

using namespace System;
using namespace System::IO;
using namespace System::Collections::Generic;
using namespace System::Runtime::InteropServices;
using namespace SiliconStudio::Core::Mathematics;

namespace SiliconStudio {
	namespace Paradox {
		namespace Importer {
			namespace FBX {

				ref class AnimationConverter
				{
				private:
					FbxScene* scene;
					bool exportedFromMaya;
					SceneMapping^ sceneMapping;

				public:
					AnimationConverter(SceneMapping^ sceneMapping)
					{
						if (sceneMapping == nullptr) throw gcnew ArgumentNullException("sceneMapping");

						this->sceneMapping = sceneMapping;
						this->scene = sceneMapping->Scene;;

						auto documentInfo = scene->GetDocumentInfo();
						if (documentInfo->Original_ApplicationName.Get() == "Maya")
							exportedFromMaya = true;
					}

					bool HasAnimationData()
					{
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

					AnimationClip^ ProcessAnimation()
					{
						auto animationClip = gcnew AnimationClip();

						int animStackCount = scene->GetMemberCount<FbxAnimStack>();
						// We support only anim stack count.
						if (animStackCount > 1)
						{
							// TODO: Add a log
							animStackCount = 1;
						}

						for (int i = 0; i < animStackCount; ++i)
						{
							FbxAnimStack* animStack = scene->GetMember<FbxAnimStack>(i);
							int animLayerCount = animStack->GetMemberCount<FbxAnimLayer>();
							FbxAnimLayer* animLayer = animStack->GetMember<FbxAnimLayer>(0);

							//// From http://www.the-area.com/forum/autodesk-fbx/fbx-sdk/resetpivotsetandconvertanimation-issue/page-1/
							//scene->GetRootNode()->ResetPivotSet(FbxNode::eDestinationPivot);
							//SetPivotStateRecursive(scene->GetRootNode());
							//scene->GetRootNode()->ConvertPivotAnimationRecursive(animStack, FbxNode::eDestinationPivot, 30.0f);

							scene->SetCurrentAnimationStack(animStack);

							ProcessAnimation(animationClip, animStack, scene->GetRootNode());

							//scene->GetRootNode()->ResetPivotSet(FbxNode::eSourcePivot);
						}

						if (animationClip->Curves->Count == 0)
							animationClip = nullptr;

						return animationClip;
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

				private:

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
					AnimationCurve<T>^ ProcessAnimationCurveVector(AnimationClip^ animationClip, String^ name, int numCurves, FbxAnimCurve** curves, float maxErrorThreshold)
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

					template <class T> AnimationCurve<T>^ CreateCurve(AnimationClip^ animationClip, String^ name, List<KeyFrameData<T>>^ keyFrames)
					{
						// Add curve
						auto animationCurve = gcnew AnimationCurve<T>();

						if (T::typeid == Vector3::typeid)
						{
							// Switch to cubic implicit interpolation mode for Vector3
							animationCurve->InterpolationType = AnimationCurveInterpolationType::Cubic;
						}

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

					AnimationCurve<Quaternion>^ ProcessAnimationCurveRotation(AnimationClip^ animationClip, String^ name, FbxAnimCurve** curves, float maxErrorThreshold)
					{
						auto keyFrames = ProcessAnimationCurveFloatsHelper<Vector3>(curves, 3);
						if (keyFrames == nullptr)
							return nullptr;

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
							Quaternion quatX, quatY, quatZ;

							Vector3 rotation = keyFrame.Value;
							Quaternion::RotationX(rotation.X, quatX);
							Quaternion::RotationY(rotation.Y, quatY);
							Quaternion::RotationZ(rotation.Z, quatZ);

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

						return animationCurve;
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

					void ProcessAnimation(AnimationClip^ animationClip, FbxAnimStack* animStack, FbxNode* pNode)
					{
						auto layer0 = animStack->GetMember<FbxAnimLayer>(0);

						if (HasAnimation(layer0, pNode))
						{
							float start, end;
							const FbxTakeInfo* take_info = scene->GetTakeInfo(animStack->GetName());
							if (take_info)
							{
								start = (float)take_info->mLocalTimeSpan.GetStart().GetSecondDouble();
								end = (float)take_info->mLocalTimeSpan.GetStop().GetSecondDouble();
							}
							else
							{
								// Take the time line value.
								FbxTimeSpan lTimeLineTimeSpan;
								scene->GetGlobalSettings().GetTimelineDefaultTimeSpan(lTimeLineTimeSpan);
								start = (float)lTimeLineTimeSpan.GetStart().GetSecondDouble();
								end = (float)lTimeLineTimeSpan.GetStop().GetSecondDouble();
							}

							auto evaluator = scene->GetAnimationEvaluator();

							auto animationName = animStack->GetName();

							// Create curves
							auto scalingFrames = gcnew List<KeyFrameData<Vector3>>();
							auto rotationFrames = gcnew List<KeyFrameData<Quaternion>>();
							auto translationFrames = gcnew List<KeyFrameData<Vector3>>();

							auto nodeData = sceneMapping->FindNode(pNode);

							auto parentNode = pNode->GetParent();
							auto nodeName = nodeData.Name;
							String^ parentNodeName = nullptr;
							if (parentNode != nullptr)
							{
								parentNodeName = sceneMapping->FindNode(parentNode).Name;
							}

							const float sampling_period = 1.f / 60.0f;
							bool loop_again = true;
							for (float t = start; loop_again; t += sampling_period) {
								if (t >= end) {
									t = end;
									loop_again = false;
								}

								auto fbxTime = FbxTimeSeconds(t);

								// Use GlobalTransform instead of LocalTransform
								auto fbxMatrix = evaluator->GetNodeGlobalTransform(pNode, fbxTime);
								if (parentNode != nullptr)
								{
									auto parentMatrixInverse = evaluator->GetNodeGlobalTransform(parentNode, fbxTime).Inverse();
									fbxMatrix = parentMatrixInverse * fbxMatrix;
								}
								auto matrix = sceneMapping->ConvertMatrixFromFbx(fbxMatrix);

								Vector3 scaling;
								Vector3 translation;
								Quaternion rotation;
								matrix.Decompose(scaling, rotation, translation);

								auto time = FBXTimeToTimeSpane(fbxTime);

								scalingFrames->Add(KeyFrameData<Vector3>(time, scaling));
								translationFrames->Add(KeyFrameData<Vector3>(time, translation));
								rotationFrames->Add(KeyFrameData<Quaternion>(time, rotation));
								//System::Diagnostics::Debug::WriteLine("[{0}] Parent:{1} Transform.Position[{2}] = {3}", t, parentNodeName, nodeName, translation);
								//System::Diagnostics::Debug::WriteLine("[{0}] Parent:{1} Transform.Rotation[{2}] = {3}", t, parentNodeName, nodeName, rotation);
								//System::Diagnostics::Debug::WriteLine("[{0}] Parent:{1} Transform.Scale[{2}] = {3}", t, parentNodeName, nodeName, scaling);
							}

							CreateCurve(animationClip, String::Format("Transform.Position[{0}]", nodeName), translationFrames);
							CreateCurve(animationClip, String::Format("Transform.Rotation[{0}]", nodeName), rotationFrames);
							CreateCurve(animationClip, String::Format("Transform.Scale[{0}]", nodeName), scalingFrames);
						}

						for (int i = 0; i < pNode->GetChildCount(); ++i)
						{
							ProcessAnimation(animationClip, animStack, pNode->GetChild(i));
						}
					}

					void SetPivotStateRecursive(FbxNode* pNode)
					{
						pNode->SetPivotState(FbxNode::eSourcePivot, FbxNode::ePivotActive);
						pNode->SetPivotState(FbxNode::eDestinationPivot, FbxNode::ePivotActive);

						for (int i = 0; i < pNode->GetChildCount(); ++i)
						{
							SetPivotStateRecursive(pNode->GetChild(i));
						}
					}

					bool CheckAnimationData(FbxAnimLayer* animLayer, FbxNode* pNode)
					{
						if ((pNode->LclTranslation.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_X) != NULL
							&& pNode->LclTranslation.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_Y) != NULL
							&& pNode->LclTranslation.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_Z) != NULL)
							||
							(pNode->LclRotation.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_X) != NULL
							&& pNode->LclRotation.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_Y) != NULL
							&& pNode->LclRotation.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_Z) != NULL)
							||
							(pNode->LclScaling.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_X) != NULL
							&& pNode->LclScaling.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_Y) != NULL
							&& pNode->LclScaling.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_Z) != NULL))
							return true;

						FbxCamera* camera = pNode->GetCamera();
						if (camera != NULL)
						{
							if (camera->FieldOfViewY.GetCurve(animLayer))
								return true;

							if (camera->FocalLength.GetCurve(animLayer))
								return true;
						}

						for (int i = 0; i < pNode->GetChildCount(); ++i)
						{
							if (CheckAnimationData(animLayer, pNode->GetChild(i)))
								return true;
						}

						return false;
					}

					bool HasAnimation(FbxAnimLayer* animLayer, FbxNode* pNode)
					{
						return (pNode->LclTranslation.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_X) != NULL
							|| pNode->LclTranslation.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_Y) != NULL
							|| pNode->LclTranslation.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_Z) != NULL
							|| pNode->LclRotation.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_X) != NULL
							|| pNode->LclRotation.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_Y) != NULL
							|| pNode->LclRotation.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_Z) != NULL
							|| pNode->LclScaling.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_X) != NULL
							|| pNode->LclScaling.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_Y) != NULL
							|| pNode->LclScaling.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_Z) != NULL);
					}

					void GetAnimationNodes(FbxAnimLayer* animLayer, FbxNode* pNode, List<String^>^ animationNodes)
					{
						auto nodeData = sceneMapping->FindNode(pNode);;
						auto nodeName = nodeData.Name;

						bool checkTranslation = pNode->LclTranslation.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_X) != NULL;
						checkTranslation = checkTranslation || pNode->LclTranslation.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_Y) != NULL;
						checkTranslation = checkTranslation || pNode->LclTranslation.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_Z) != NULL;

						bool checkRotation = pNode->LclRotation.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_X) != NULL;
						checkRotation = checkRotation || pNode->LclRotation.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_Y) != NULL;
						checkRotation = checkRotation || pNode->LclRotation.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_Z) != NULL;

						bool checkScale = pNode->LclScaling.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_X) != NULL;
						checkScale = checkScale || pNode->LclScaling.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_Y) != NULL;
						checkScale = checkScale || pNode->LclScaling.GetCurve(animLayer, FBXSDK_CURVENODE_COMPONENT_Z) != NULL;

						if (checkTranslation || checkRotation || checkScale)
						{
							animationNodes->Add(nodeName);
						}
						else
						{
							bool checkCamera = true;
							FbxCamera* camera = pNode->GetCamera();
							if (camera != NULL)
							{
								if (camera->FieldOfViewY.GetCurve(animLayer))
									checkCamera = checkCamera && camera->FieldOfViewY.GetCurve(animLayer) != NULL;

								if (camera->FocalLength.GetCurve(animLayer))
									checkCamera = checkCamera && camera->FocalLength.GetCurve(animLayer) != NULL;

								if (checkCamera)
									animationNodes->Add(nodeName);
							}
						}

						for (int i = 0; i < pNode->GetChildCount(); ++i)
						{
							GetAnimationNodes(animLayer, pNode->GetChild(i), animationNodes);
						}
					}
				};

			}
		}
	}
}
