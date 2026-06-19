using UnityEngine;
using DG.Tweening;
using Unity.Cinemachine;

public class StarAnimation : MonoBehaviour //this script is now useless
{
    private Animator _animator;
    private Transform _big;
    private Transform _small;
    public AnimationCurve punch;

    [Header("Particules")]
    public ParticleSystem glow;
    public ParticleSystem charge;
    public ParticleSystem explode;
    public ParticleSystem smoke;

    void Start()
    {
        _animator = GetComponent<Animator>();
        _big = transform.childCount > 0 ? transform.GetChild(0) : null;
        _small = transform.childCount > 1 ? transform.GetChild(1) : null;
    }

    public Sequence Reset(float time)
    {
        if (_animator != null) _animator.enabled = false;
        Sequence s = DOTween.Sequence();
        if (_big != null)
            s.Append(_big.DOLocalRotate(Vector3.zero, time).SetEase(Ease.InOutSine));
        if (_small != null)
            s.Join(_small.DOLocalRotate(Vector3.zero, time).SetEase(Ease.InOutSine));
        return s;
    }

    public Sequence PullStar(float pullTime)
    {
        if (glow != null) glow.Play();
        if (charge != null) charge.Play();
        Sequence s = DOTween.Sequence();
        if (_big != null)
            s.Append(_big.DOLocalRotate(
                new Vector3(0, 0, 360 * 2), pullTime,
                RotateMode.LocalAxisAdd).SetEase(Ease.OutQuart));
        if (_small != null)
        {
            s.Join(_small.DOLocalRotate(
                new Vector3(0, 0, 360 * 2), pullTime,
                RotateMode.LocalAxisAdd).SetEase(Ease.OutQuart));
            s.Join(_small.DOLocalMoveZ(-4.2f, pullTime));
        }
        return s;
    }

    public Sequence PunchStar(float punchTime)
    {
        var impulses = FindObjectsByType<CinemachineImpulseSource>(
            FindObjectsSortMode.None);

        if (_animator != null) _animator.enabled = false;
        Sequence s = DOTween.Sequence();
        if (explode != null) s.AppendCallback(() => explode.Play());
        if (smoke != null) s.AppendCallback(() => smoke.Play());
        if (impulses.Length > 0)
            s.AppendCallback(() => impulses[0].GenerateImpulse());
        if (_small != null)
        {
            s.Append(_small.DOLocalMove(Vector3.zero, 0.8f).SetEase(punch));
            s.Join(_small.DOLocalRotate(
                new Vector3(0, 0, 360 * 2), 0.8f).SetEase(Ease.OutBack));
        }
        s.AppendInterval(0.8f);
        if (_animator != null)
            s.AppendCallback(() => _animator.enabled = true);
        return s;
    }
}