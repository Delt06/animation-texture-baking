using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace Editor
{
	public class AnimationToTexture : EditorWindow
	{
		private static int _frameRate = 24;
		private Animation _animation;
		private AnimationClip _animationClip;
		private SkinnedMeshRenderer _context;

		private void OnEnable()
		{
			titleContent = new GUIContent("Bake Animation to Texture");
		}

		private void OnGUI()
		{
			_frameRate = Mathf.Clamp(EditorGUILayout.IntField("Framerate", _frameRate), 1, 60);
			_animationClip =
				EditorGUILayout.ObjectField("Clip", _animationClip, typeof(AnimationClip), false) as AnimationClip;
			_animation = EditorGUILayout.ObjectField("Animation", _animation, typeof(Animation), true) as Animation;

			if (GUILayout.Button("Bake")) CreateAnimationTexture();
		}

		[MenuItem("CONTEXT/SkinnedMeshRenderer/Bake Animation")]
		private static void Open(MenuCommand menuCommand)
		{
			var window = GetWindow<AnimationToTexture>();
			window._context = (SkinnedMeshRenderer)menuCommand.context;
			window.ShowModalUtility();
		}

		private void CreateAnimationTexture()
		{
			Close();
			Assert.IsNotNull(_animationClip, "Animation clip is null");
			Assert.IsNotNull(_animation, "Animation is null");

			var duration = _animationClip.length;
			var frameCount = (int)(duration * _frameRate);
			var vertexCount = _context.sharedMesh.vertexCount;

			var animationTexture = new Texture2D(
				frameCount,
				vertexCount * 3,
				TextureFormat.RGBAHalf, false, false
			)
			{
				wrapMode = TextureWrapMode.Clamp,
			};

			var targetGameObject = _animation.gameObject;
			BakeAnimation(targetGameObject, frameCount, duration, animationTexture);
			CreateTextureAsset(animationTexture);
		}

		private void BakeAnimation(GameObject targetGameObject, int frameCount, float duration,
			Texture2D animationTexture)
		{
			var mesh = new Mesh();

			var lossyScale = _context.transform.lossyScale;
			var invScale = new Vector3(1 / lossyScale.x, 1 / lossyScale.y, 1 / lossyScale.z);

			var lastFrameIndex = frameCount - 1;
			for (var frameIndex = 0; frameIndex < frameCount; frameIndex++)
			{
				var normalizedTime = (float)frameIndex / lastFrameIndex * duration;
				_animationClip.SampleAnimation(targetGameObject, normalizedTime);
				_context.BakeMesh(mesh);

				var vertices = mesh.vertices;
				var normals = mesh.normals;
				var tangents = mesh.tangents;

				for (var i = 0; i < vertices.Length; i++)
				{
					var vertex = vertices[i];
					vertex.x *= invScale.x;
					vertex.y *= invScale.y;
					vertex.z *= invScale.z;
					var positionColor = new Color(vertex.x, vertex.y, vertex.z);

					var normal = normals[i];

					var normalColor = new Color(normal.x, normal.y, normal.z);
					Color tangentColor;
					if (tangents.Length > 0)
					{
						var tangent = tangents[i];
						tangentColor = new Color(tangent.x, tangent.y, tangent.z, tangent.w);
					}
					else
					{
						tangentColor = Color.clear;
					}

					animationTexture.SetPixel(frameIndex, i * 3, positionColor);
					animationTexture.SetPixel(frameIndex, i * 3 + 1, normalColor);
					animationTexture.SetPixel(frameIndex, i * 3 + 2, tangentColor);
				}
			}

			DestroyImmediate(mesh);
		}

		private static void CreateTextureAsset(Texture2D animationTexture)
		{
			var path = EditorUtility.SaveFilePanelInProject("Save animation texture", "Animation", "asset",
				"Select animation asset path"
			);
			if (string.IsNullOrEmpty(path))
			{
				DestroyImmediate(animationTexture);
				return;
			}

			AssetDatabase.CreateAsset(animationTexture, path);
			AssetDatabase.SaveAssets();
		}
	}
}