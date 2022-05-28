using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace Editor
{
    public class AnimationToTexture : EditorWindow
    {
        private const int LayerIndex = 0;
        private static int _frameRate = 24;
        private Animator _animator;
        private string _stateName;

        private void OnEnable()
        {
            titleContent = new GUIContent("Bake Animation to Texture");
        }

        private void OnGUI()
        {
            _frameRate = Mathf.Clamp(EditorGUILayout.IntField("Framerate", _frameRate), 1, 60);
            _stateName = EditorGUILayout.TextField("State Name", _stateName);

            if (GUILayout.Button("Bake"))
                CreateAnimationTexture();
        }

        [MenuItem("CONTEXT/Animator/Bake Animation")]
        private static void Open(MenuCommand menuCommand)
        {
            var window = GetWindow<AnimationToTexture>();
            window._animator = (Animator) menuCommand.context;
            window.ShowModalUtility();
        }

        private void CreateAnimationTexture()
        {
            Close();

            Assert.IsNotNull(_animator, "Animator was destroyed");
            Assert.IsFalse(string.IsNullOrWhiteSpace(_stateName), "State Name is empty");

            var skinnedMeshRenderer = _animator.GetComponentInChildren<SkinnedMeshRenderer>();
            Assert.IsNotNull(skinnedMeshRenderer, "No SkinnedMeshRenderer found");

            _animator.Play(_stateName, LayerIndex, 0);
            _animator.Update(0);
            var animatorStateInfo = _animator.GetCurrentAnimatorStateInfo(LayerIndex);

            var duration = animatorStateInfo.length;
            var frameCount = (int) (duration * _frameRate);
            var vertexCount = skinnedMeshRenderer.sharedMesh.vertexCount;

            var animationTexture = new Texture2D(
                frameCount,
                vertexCount * 3,
                TextureFormat.RGBAHalf, false, false
            )
            {
                wrapMode = TextureWrapMode.Clamp,
            };

            BakeAnimation(_animator, frameCount, animationTexture, skinnedMeshRenderer);
            CreateTextureAsset(animationTexture);
        }

        private void BakeAnimation(Animator animator, int frameCount, Texture2D animationTexture,
            SkinnedMeshRenderer skinnedMeshRenderer)
        {
            var mesh = new Mesh();

            var lossyScale = skinnedMeshRenderer.transform.lossyScale;
            var invScale = new Vector3(1 / lossyScale.x, 1 / lossyScale.y, 1 / lossyScale.z);

            var lastFrameIndex = frameCount - 1;
            for (var frameIndex = 0; frameIndex < frameCount; frameIndex++)
            {
                var normalizedTime = (float) frameIndex / lastFrameIndex;
                animator.Play(_stateName, LayerIndex, normalizedTime);
                animator.Update(0f);
                skinnedMeshRenderer.BakeMesh(mesh);

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