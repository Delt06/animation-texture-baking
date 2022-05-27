using DELTation.ToonShader.Editor;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Editor
{
	[UsedImplicitly]
	public class ToonShaderAnimationTextureEditor : ToonShaderEditor
	{
		protected override void DrawProperties(MaterialEditor materialEditor, MaterialProperty[] properties,
			Material material)
		{
			if (Foldout("Animation", true))
			{
				DrawProperty(materialEditor, properties, "_AnimationTexture");
				DrawProperty(materialEditor, properties, "_FrameRate");
			}

			base.DrawProperties(materialEditor, properties, material);
		}
	}
}