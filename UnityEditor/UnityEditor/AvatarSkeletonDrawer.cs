using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor
{
	internal class AvatarSkeletonDrawer
	{
		private static bool sPoseError;

		private static Color kSkeletonColor = new Color(0.403921574f, 0.403921574f, 0.403921574f, 0.25f);

		private static Color kDummyColor = new Color(0.235294119f, 0.235294119f, 0.235294119f, 0.25f);

		private static Color kHumanColor = new Color(0f, 0.8235294f, 0.2901961f, 0.25f);

		private static Color kErrorColor = new Color(1f, 0f, 0f, 0.25f);

		private static Color kErrorMessageColor = new Color(1f, 0f, 0f, 0.75f);

		private static Color kSelectedColor = new Color(0.5019608f, 0.7529412f, 1f, 0.15f);

		public static void DrawSkeleton(Transform reference, Dictionary<Transform, bool> actualBones)
		{
			AvatarSkeletonDrawer.DrawSkeleton(reference, actualBones, null);
		}

		public static void DrawSkeleton(Transform reference, Dictionary<Transform, bool> actualBones, AvatarSetupTool.BoneWrapper[] bones)
		{
			if (!(reference == null) && actualBones != null)
			{
				AvatarSkeletonDrawer.sPoseError = false;
				Bounds bounds = default(Bounds);
				Renderer[] componentsInChildren = reference.root.GetComponentsInChildren<Renderer>();
				if (componentsInChildren != null)
				{
					Renderer[] array = componentsInChildren;
					for (int i = 0; i < array.Length; i++)
					{
						Renderer renderer = array[i];
						bounds.Encapsulate(renderer.bounds.min);
						bounds.Encapsulate(renderer.bounds.max);
					}
				}
				Quaternion orientation = Quaternion.identity;
				if (bones != null)
				{
					orientation = AvatarSetupTool.AvatarComputeOrientation(bones);
				}
				AvatarSkeletonDrawer.DrawSkeletonSubTree(actualBones, bones, orientation, reference, bounds);
				Camera current = Camera.current;
				if (AvatarSkeletonDrawer.sPoseError && current != null)
				{
					GUIStyle gUIStyle = new GUIStyle(GUI.skin.label);
					gUIStyle.normal.textColor = Color.red;
					gUIStyle.wordWrap = false;
					gUIStyle.alignment = TextAnchor.MiddleLeft;
					gUIStyle.fontSize = 20;
					GUIContent content = EditorGUIUtility.TrTextContent("Character is not in T pose", null, null);
					Rect rect = GUILayoutUtility.GetRect(content, gUIStyle);
					rect.x = 30f;
					rect.y = 30f;
					Handles.BeginGUI();
					GUI.Label(rect, content, gUIStyle);
					Handles.EndGUI();
				}
			}
		}

		private static bool DrawSkeletonSubTree(Dictionary<Transform, bool> actualBones, AvatarSetupTool.BoneWrapper[] bones, Quaternion orientation, Transform tr, Bounds bounds)
		{
			bool result;
			if (!actualBones.ContainsKey(tr))
			{
				result = false;
			}
			else
			{
				int num = 0;
				IEnumerator enumerator = tr.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						Transform tr2 = (Transform)enumerator.Current;
						if (AvatarSkeletonDrawer.DrawSkeletonSubTree(actualBones, bones, orientation, tr2, bounds))
						{
							num++;
						}
					}
				}
				finally
				{
					IDisposable disposable;
					if ((disposable = (enumerator as IDisposable)) != null)
					{
						disposable.Dispose();
					}
				}
				if (!actualBones[tr] && num <= 1)
				{
					result = false;
				}
				else
				{
					int num2 = -1;
					if (bones != null)
					{
						for (int i = 0; i < bones.Length; i++)
						{
							if (bones[i].bone == tr)
							{
								num2 = i;
								break;
							}
						}
					}
					bool flag = AvatarSetupTool.GetBoneAlignmentError(bones, orientation, num2) > 0f;
					AvatarSkeletonDrawer.sPoseError |= flag;
					if (flag)
					{
						AvatarSkeletonDrawer.DrawPoseError(tr, bounds);
						Handles.color = AvatarSkeletonDrawer.kErrorColor;
					}
					else if (num2 != -1)
					{
						Handles.color = AvatarSkeletonDrawer.kHumanColor;
					}
					else if (!actualBones[tr])
					{
						Handles.color = AvatarSkeletonDrawer.kDummyColor;
					}
					else
					{
						Handles.color = AvatarSkeletonDrawer.kSkeletonColor;
					}
					Handles.DoBoneHandle(tr, actualBones);
					if (Selection.activeObject == tr)
					{
						Handles.color = AvatarSkeletonDrawer.kSelectedColor;
						Handles.DoBoneHandle(tr, actualBones);
					}
					result = true;
				}
			}
			return result;
		}

		private static void DrawPoseError(Transform node, Bounds bounds)
		{
			Camera current = Camera.current;
			if (current)
			{
				GUIStyle gUIStyle = new GUIStyle(GUI.skin.label);
				gUIStyle.normal.textColor = Color.red;
				gUIStyle.wordWrap = false;
				gUIStyle.alignment = TextAnchor.MiddleLeft;
				Vector3 position = node.position;
				Vector3 vector = node.position + Vector3.up * 0.2f;
				if (node.position.x <= node.root.position.x)
				{
					vector.x = bounds.min.x;
				}
				else
				{
					vector.x = bounds.max.x;
				}
				GUIContent content = new GUIContent(node.name);
				Rect position2 = HandleUtility.WorldPointToSizedRect(vector, content, gUIStyle);
				position2.x += 2f;
				if (node.position.x > node.root.position.x)
				{
					position2.x -= position2.width;
				}
				Handles.BeginGUI();
				position2.y -= gUIStyle.CalcSize(content).y / 4f;
				GUI.Label(position2, content, gUIStyle);
				Handles.EndGUI();
				Handles.color = AvatarSkeletonDrawer.kErrorMessageColor;
				Handles.DrawLine(position, vector);
			}
		}
	}
}
