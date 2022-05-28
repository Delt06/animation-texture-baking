using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class TextureAnimator : MonoBehaviour
{
    private static readonly int AnimationTimeId = Shader.PropertyToID("_AnimationTime");

    [SerializeField] [Min(0f)] private float _timeScale = 1f;

    private float _animationTime;
    private MaterialPropertyBlock _materialPropertyBlock;
    private Renderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _materialPropertyBlock = new MaterialPropertyBlock();
    }

    private void Update()
    {
        _animationTime += Time.deltaTime * _timeScale;
        _materialPropertyBlock.SetFloat(AnimationTimeId, _animationTime);
        _renderer.SetPropertyBlock(_materialPropertyBlock);
    }

    private void OnEnable()
    {
        _animationTime = 0f;
    }
}