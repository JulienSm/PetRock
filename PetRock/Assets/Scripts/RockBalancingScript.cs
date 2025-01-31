﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class RockBalancingScript : MonoBehaviour
{
    [HideInInspector]
    public Player player;
    [Range(0f, 0.5f)] public float deadZone = 0.01f;

    //Speed rock
    [Header("Speed rock movement")]
    [SerializeField, Min(0f)] float _speed = 5f;
    [SerializeField, Min(0f)] float _rotationSpeed = 90f;

    [Header("Start rock position")]
    [SerializeField]
    private Transform _startPosRock;

    [Header("Validate position rock")]
    [SerializeField]
    private Transform _finalPosRock;
    private Transform _finalA;
    private Transform _finalB;
    [SerializeField]
    private  float _margeRot;

    private float _posVib;
    private float _rotVib;

    [Header("RockPrefab")]
    [SerializeField]
    private GameObject _prefabRock;
    private GameObject _theRock;

    private bool _canRockBalance = false;
    private bool _goodPlace = false;
    private bool _goodRot = false;
    private bool _endRockBalancing = false;

    [Header("Camera views")]
    [SerializeField]
    private TriggeredViewVolume _triggeredViewVolume;
    [SerializeField]
    private DollyViewAutoCircle _dollyView;

    private void OnEnable()
    {
        player = GameManager.instance.player;
        _theRock = Instantiate(_prefabRock, _startPosRock.position, Quaternion.identity, _startPosRock.parent);

        _finalA = _finalPosRock.GetChild(0);
        _finalB = _finalPosRock.GetChild(1);

        StartCoroutine(startRockBalance());
    }

    // Update is called once per frame
    void Update()
    {
        if (!_canRockBalance)
            return;

        float move = player.GetAxis("MovePetRock");
        move = Mathf.Abs(move) > deadZone ? move : 0;

        float rotate = player.GetAxis("TiltPetRock");
        rotate = Mathf.Abs(rotate) > deadZone ? rotate : 0;

        _theRock.transform.position += move * -transform.forward * _speed * Time.deltaTime; // += gravity * 
        _theRock.transform.Rotate(Vector3.right, -rotate * _rotationSpeed * Time.deltaTime);

        _posVib = Mathf.Clamp(Mathf.Abs(_theRock.transform.position.z - _finalPosRock.position.z), 0f, 0.5f);
        _rotVib = Mathf.Clamp(Mathf.Abs((_theRock.transform.rotation.x - _finalPosRock.rotation.x)), 0f, 0.5f);


        if (ValidateRot() == true && ValidatePos() == true && player.GetButton("ValidatePetRockPos"))
        {
            _endRockBalancing = true;

            player.SetVibration(0, 0);
            player.StopVibration();
            EndRockBalancing();
        }
        else
        {
            if(!_endRockBalancing)
                player.SetVibration(0, _posVib + _rotVib);
        }
    }

    private bool ValidatePos()
    {
        if (_theRock.transform.position.z <= _finalA.position.z 
            && _theRock.transform.position.z >= _finalB.position.z)
            _goodPlace = true;
        else
            _goodPlace = false;

        return _goodPlace;
    }

    private bool ValidateRot()
    {
        if (_theRock.transform.localRotation.eulerAngles.x >= (_finalPosRock.localRotation.eulerAngles.x - _margeRot)
            && _theRock.transform.localRotation.eulerAngles.x <= (_finalPosRock.localRotation.eulerAngles.x + _margeRot))
            _goodRot = true;
        else
            _goodRot = false;

        return _goodRot;
    }

    private IEnumerator startRockBalance()
    {
        yield return new WaitForSeconds(1);
        _canRockBalance = true;
    }

    private void EndRockBalancing()
    {
        _dollyView.SetActive(true);
        StartCoroutine(finishRockBalancing());
    }

    private IEnumerator finishRockBalancing()
    {
        yield return new WaitForSeconds(_dollyView.getTimeToRotate + 1);

        GetComponent<Collider>().enabled = false;

        _dollyView.SetActive(false);
        _triggeredViewVolume.ActiveView(false);

        Destroy(_theRock);

        enabled = false;
        GameManager.instance.inRockBalancing = false;
    }
}
