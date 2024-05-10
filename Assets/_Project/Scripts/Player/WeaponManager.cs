using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WeaponManager : MonoBehaviour
{
    public enum WeaponPositionState
    {
        Up,
        Down,
        PutUp,
        PutDown,
        Aim,
        Sprint
    }

    public enum WeaponEventType
    {
        Switch,
        Reload,
        OutOfAmmo
    }

 // public UnityAction<Gun> WeaponSwitchEvent;
    public UnityAction<WeaponEventType, Gun> WeaponEvent;
    [SerializeField] private Camera _playerCamera, _weaponCamera;
    [SerializeField] private PlayerCharacterController _playerController;
    [SerializeField] private LayerMask _weaponLayer;
    [SerializeField, Tooltip("The selectable weapons")] private List<Gun> _availableWeaponsList;
    [SerializeField] private Transform _weaponParentPosition;
    [SerializeField] private Transform _weaponDownPosition;
    [SerializeField] private Transform _weaponDefaultPosition;
    [SerializeField] private Transform _weaponAimingPosition;
    [SerializeField] private Transform _weaponSprintPosition;

    [SerializeField] private float _aimAnimationSpeed = 10f, _sprintAnimationSpeed = 10f;
    [SerializeField] private float _weaponPutUpAnimationSpeed = 5f,  _defaultFOV = 60f;

    //Create weapons inventory with 2 slots for weapons
    private Gun[] _weaponSlots = new Gun[2];
    //The currently active weapon
    private int _activeWeaponIndex;
    //The calculated position of the weapon
    private Vector3 _weaponPosition;
    //The switch state of the weapon
    private WeaponPositionState _weaponPositionState = WeaponPositionState.Down;
    //The currently active weapon
    private Gun _activeWeapon;

    private float _timeWeaponSwitchStarted;

    public Gun ActiveWeapon => _activeWeapon;

    // Start is called before the first frame update
    void Start()
    {
        _activeWeaponIndex = -1;
        _weaponPositionState = WeaponPositionState.Down;
        WeaponEvent += OnWeaponEvent;

        foreach (Gun weapon in _availableWeaponsList)
        {
            AddWeapon(weapon);
        }

        SwitchWeapon(0);
    }

    // Update is called once per frame
    void LateUpdate()
    {
       UpdateWeaponState();
        UpdateCameraFieldOfView();
        _weaponParentPosition.localPosition = _weaponPosition;
    }

    private void SwitchWeapon(int index)
    {
        int newWeaponIndex = -1;
        int closestSlotDistance = _weaponSlots.Length;

        if (index < _weaponSlots.Length && index >= 0 && index != _activeWeaponIndex)
        {
            newWeaponIndex = index;
        }

        //Valid weapon index we switch
        if (newWeaponIndex > -1)
        {
            _timeWeaponSwitchStarted = Time.time;
            _weaponPosition = _weaponDownPosition.localPosition;

            _activeWeaponIndex = newWeaponIndex;
            _activeWeapon = _weaponSlots[_activeWeaponIndex];
            _weaponPositionState = WeaponPositionState.PutUp;
            WeaponEvent?.Invoke(WeaponEventType.Switch, _activeWeapon);
        }
    }


    private void AddWeapon(Gun weaponToAdd)
    {
        for (int i = 0; i < _weaponSlots.Length; i++)
        {
            if (_weaponSlots[i] == null)
            {
                Gun weaponInstance = Instantiate(weaponToAdd, _weaponParentPosition);
                weaponInstance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                weaponInstance.SetVisibility(false);
                //Convert to selected weapon layer to an index.
                int layerIndex = Mathf.RoundToInt(Mathf.Log(_weaponLayer.value, 2));
                foreach (Transform t in weaponInstance.gameObject.GetComponentsInChildren<Transform>(true))
                {
                    t.gameObject.layer  = layerIndex;
                }

                _weaponSlots[i] = weaponInstance;
                break;
            }
        }
    }
    private void UpdateWeaponState()
    {

        //If the player is holding the sprint button, this method will lerp the position to the sprinting position
        UpdateWeaponSprintState();
        //If the player is holding the aim , this method will lerp the position to the aiming position
        UpdateWeaponAimState();
        //If the player is switching weapons, this method will update the positions
        UpdateWeaponPutUpState();

        UpdateWeaponPosition();
    }

    /// <summary>
    /// Will lerp the current position to the default position
    /// </summary>
    private void UpdateWeaponPutUpState()
    {
        if (_weaponPositionState == WeaponPositionState.PutUp && _weaponPosition == _weaponDefaultPosition.localPosition)
        {
            _weaponPositionState = WeaponPositionState.Up;
        }
    }

    //Will lerp the current weapon position to the ADS position
    private void UpdateWeaponAimState()
    {
        _weaponPositionState = _playerController.IsAimingDownSight && _activeWeapon ? WeaponPositionState.Aim : WeaponPositionState.PutUp;
    }

    private void UpdateWeaponSprintState()
    {
        _weaponPositionState = _playerController.IsSprinting ? WeaponPositionState.Sprint : WeaponPositionState.PutUp;
    }

    private void UpdateWeaponPosition()
    {
        _weaponPosition = _weaponPositionState switch
        {
            WeaponPositionState.PutUp => Vector3.Lerp(_weaponPosition, _weaponDefaultPosition.localPosition, _weaponPutUpAnimationSpeed * Time.deltaTime),
            WeaponPositionState.Aim => Vector3.Lerp(_weaponPosition, _weaponAimingPosition.localPosition, _aimAnimationSpeed * Time.deltaTime),
            WeaponPositionState.Sprint => Vector3.Lerp(_weaponPosition, _weaponSprintPosition.localPosition, _sprintAnimationSpeed * Time.deltaTime),
            _ => Vector3.Lerp(_weaponPosition, _weaponDefaultPosition.localPosition, _weaponPutUpAnimationSpeed * Time.deltaTime),
        };
    }
    private void UpdateCameraFieldOfView()
    {
        switch (_weaponPositionState)
        {
            case WeaponPositionState.Aim:
                SetFieldOfView(Mathf.Lerp(_playerCamera.fieldOfView, _activeWeapon.ZoomRatio * _defaultFOV, _aimAnimationSpeed * Time.deltaTime));
                break;
            default:
                SetFieldOfView(Mathf.Lerp(_playerCamera.fieldOfView, _defaultFOV, _aimAnimationSpeed * Time.deltaTime));
                break;
        }
    }

    private void SetFieldOfView(float fov)
    {
        _playerCamera.fieldOfView = Mathf.Lerp(_playerCamera.fieldOfView, fov, _aimAnimationSpeed * Time.deltaTime);
        _weaponCamera.fieldOfView = _playerCamera.fieldOfView;
    }

    private void OnWeaponEvent(WeaponEventType eventType, Gun weapon)
    {
        switch (eventType)
        {
            case WeaponEventType.Reload:
                ReloadWeapon();
                break;
            case WeaponEventType.OutOfAmmo:
                // Handle out of ammo event
                break;
            case WeaponEventType.Switch:
                 if (weapon != null){
                    weapon.SetVisibility(true);
                }
            int weaponIndex = System.Array.IndexOf(_weaponSlots, weapon);
            if (weaponIndex != -1){
                SwitchWeapon(weaponIndex);
            }
                break;
        }
    }

    private void ReloadWeapon()
    {
     if (_activeWeapon != null && !_activeWeapon.CanReload()) // Assuming HasAmmo() is a method in your Gun class that checks if the gun has ammo
    {
        _activeWeapon.Reload(); // Assuming Reload() is a method in your Gun class that handles the reloading logic
    }
    }

    public void PlayShootAnimation()
    {
        _activeWeapon.PlayShootAnimation();
    }
}
