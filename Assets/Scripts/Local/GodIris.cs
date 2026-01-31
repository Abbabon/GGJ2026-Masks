using UnityEngine;

public class GodIris : MonoBehaviour
{
    [SerializeField] private Animation _animation;
    [SerializeField] private AnimationClip _winClip;
    [SerializeField] private AnimationClip _loseClip;

    public void GodWon()
    {
        PlayClip(_winClip);
    }

    public void GodLost()
    {
        PlayClip(_loseClip);
    }

    private void PlayClip(AnimationClip clip)
    {
        if (_animation == null || clip == null) return;
        _animation.clip = clip;
        _animation.Play();
    }
}
