using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Gun : MonoBehaviour
{
    [SerializeField] protected Animator _animator;
    [SerializeField] private GameObject _animationRoot;
    [SerializeField] private float _zoomRatio = 1f;
    [SerializeField] protected float _recoilAmount = 3f;
    [SerializeField] protected float _recoilSpeed = 20f;
    [SerializeField] protected float _hipfireBloom = 1.5f;

    [SerializeField] protected float _range = 50f;
    [SerializeField] protected float _delayBeforeRayCase = 0.1f;
    [SerializeField] private int _maxAmmo = 30;
    [SerializeField] private List<int> _magazines = new List<int>();
    private int _currentMagazineIndex = -1;
    [SerializeField] protected float _reloadTime=0.5f;
    public float ReloadTime => _reloadTime;
    private bool _isWeaponActive;

    public float ZoomRatio => _zoomRatio;

    public float RecoilAmount => _recoilAmount;

    public float HipfireBloom => _hipfireBloom;

    public float RecoilSpeed => _recoilSpeed;

    public float Range => _range;

    public float DelayBeforeRayCast => _delayBeforeRayCase;
    private void Awake()
    {
        _magazines.Add(10);
        Reload();
    }
    
    public virtual void Update()
    {
        _animationRoot.SetActive(_isWeaponActive);
    }
    public void SetVisibility(bool visible)
    {
        _isWeaponActive = visible;
    }
    public bool CanReload()
    {
        return _magazines.Any(magazine => magazine > 0);
    }

    public void ReduceAmmo()
    {
        if (_currentMagazineIndex >= 0 && _currentMagazineIndex < _magazines.Count && _magazines[_currentMagazineIndex] > 0)
        {
            _magazines[_currentMagazineIndex]--;

            if (_magazines[_currentMagazineIndex] == 0)
            {
                _currentMagazineIndex++;
            }
        }
    }

    public void Reload()
    {
        if (CanReload())
        {
            // Encontre o próximo pente com munição
            _currentMagazineIndex = _magazines.FindIndex(magazine => magazine > 0);

            // Recarregue o pente atual
            _magazines[_currentMagazineIndex] = _maxAmmo;
        }
    }

    //We'll make this virtual so that derived classes can override this if they have differently configured animations
    public virtual void PlayShootAnimation()
    {
        _animator.SetTrigger("Shoot");
    }

    public virtual void PlayReloadAnimation(){
          _animator.SetTrigger("Reload");
    }
}
