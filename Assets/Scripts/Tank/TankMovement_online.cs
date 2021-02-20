﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class TankMovement_online : MonoBehaviourPun, IPunObservable
{
    public int m_PlayerNumber = 1;         
    public float m_Speed = 12f;            
    public float m_TurnSpeed = 180f;       
    public AudioSource m_MovementAudio;    
    public AudioClip m_EngineIdling;       
    public AudioClip m_EngineDriving;      
    public float m_PitchRange = 0.2f;

    private Vector3 smoothMove;

    private string m_MovementAxisName;     
    private string m_TurnAxisName;         
    private Rigidbody m_Rigidbody;
    private float m_MovementInputValue;    
    private float m_TurnInputValue;        
    private float m_OriginalPitch;         


    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }

 
    private void OnEnable ()
    {
        m_Rigidbody.isKinematic = false;
        m_MovementInputValue = 0f;
        m_TurnInputValue = 0f;
    }


    private void OnDisable ()
    {
        m_Rigidbody.isKinematic = true;

    }


    private void Start()
    {
        PhotonNetwork.SendRate = 20;
        PhotonNetwork.SerializationRate = 15;
        m_MovementAxisName = "Vertical" + m_PlayerNumber;
        m_TurnAxisName = "Horizontal" + m_PlayerNumber;

        m_OriginalPitch = m_MovementAudio.pitch;

    }


    private void Update()
    {
        // Store the player's input and make sure the audio for the engine is playing.
        if (photonView.IsMine)
        {
            m_MovementInputValue = (Input.GetAxis(m_MovementAxisName));

            m_TurnInputValue = (Input.GetAxis(m_TurnAxisName)); ;


            EngineAudio();
        }
      
    }


    private void EngineAudio()
    {
        // Play the correct audio clip based on whether or not the tank is moving and what audio is currently playing.
        if (Mathf.Abs(m_MovementInputValue) > 0.1f|| Mathf.Abs(m_TurnInputValue) > 0.1f )
        {
            if (m_MovementAudio.clip != m_EngineDriving)
            {
                m_MovementAudio.clip = m_EngineDriving;
                m_MovementAudio.Play();
            }

        }
        else
        {
            if (m_MovementAudio.clip != m_EngineIdling)
            {
                m_MovementAudio.clip = m_EngineIdling;
                m_MovementAudio.Play();
            }

        }
                 
        
    }


    private void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            Move();
            Turn();
        }
        else
        {
            smoothMovement();
        }
       
    }
    private void smoothMovement()
    {
        transform.position = Vector3.Lerp(transform.position, smoothMove, Time.deltaTime * 10);
    }


    private void Move()
    {
        // Adjust the position of the tank based on the player's input.
        m_Rigidbody.MovePosition( (transform.forward* m_MovementInputValue* m_Speed*Time.deltaTime) + m_Rigidbody.position);
    }


    private void Turn()
    {
        // Adjust the rotation of the tank based on the player's input.
        m_Rigidbody.MoveRotation(Quaternion.Euler(0f, m_TurnInputValue * m_TurnSpeed  * Time.deltaTime , 0f) * m_Rigidbody.rotation);


    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
        }
        else if (stream.IsReading)
        {
            smoothMove = (Vector3)stream.ReceiveNext();
        }

    }
}