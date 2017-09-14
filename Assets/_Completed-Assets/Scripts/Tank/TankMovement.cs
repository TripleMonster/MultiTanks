﻿using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using UnityEventUtils;

namespace Complete
{
	public class TankMovement : MonoBehaviour
	{
		[SerializeField] bool m_IsRobotMode;
		[SerializeField] int m_Number;
		[SerializeField] float m_Speed;                 // 因为用了线性差值移动, 为了保证 差值参数在0-1, 所以  这个速度 固定在 (0 - 50) 之间
		[SerializeField] AudioSource m_MovementAudio;         // Reference to the audio source used to play engine sounds. NB: different to the shooting audio source.
		[SerializeField] AudioClip m_EngineIdling;            // Audio to play when the tank isn't moving.
		[SerializeField] AudioClip m_EngineDriving;           // Audio to play when the tank is moving.
		[SerializeField] float m_PitchRange;           // The amount by which the pitch of the engine noises can vary.
		[SerializeField] Transform m_BodyTransform;

		private Rigidbody m_Rigidbody;              // Reference used to move the tank.
		private float m_OriginalPitch;              // The pitch of the audio source at the start of the scene.
		private ParticleSystem[] m_particleSystems; // References to all the particles systems used by the Tanks

        private bool m_isMove = false;
		private Vector3 m_direction;
		private Vector3 m_TargetPosition;  // 移动的目标点
		private float m_synTime;
		
        [HideInInspector] public bool m_isSelf = true;
		[HideInInspector] public UnityEvent m_moveEvent;
        [HideInInspector] public UEvent_f m_speedEvent = new UEvent_f();
        [HideInInspector] public UnityEvent m_stopMoveEvent = new UnityEvent();

		private void Awake ()
		{
            m_Speed = 15;
			m_Rigidbody = GetComponent<Rigidbody> ();

			if (m_moveEvent == null) {
				m_moveEvent = new UnityEvent ();
			}
				
		}


		private void OnEnable ()
		{
			m_Rigidbody.isKinematic = false;

			m_particleSystems = GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < m_particleSystems.Length; ++i)
			{
				m_particleSystems[i].Play();
			}
		}


		private void OnDisable ()
		{
			m_Rigidbody.isKinematic = true;

			for(int i = 0; i < m_particleSystems.Length; ++i)
			{
				m_particleSystems[i].Stop();
			}
		}


		private void Start ()
		{
			//m_OriginalPitch = m_MovementAudio.pitch;
		}

		private void Update ()
		{
			//EngineAudio ();
		}


		private void EngineAudio ()
		{
			if (m_isMove)
			{
				if (m_MovementAudio.clip == m_EngineDriving)
				{
					m_MovementAudio.clip = m_EngineIdling;
					m_MovementAudio.pitch = UnityEngine.Random.Range (m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
					m_MovementAudio.Play ();
				}
			}
			else
			{
				if (m_MovementAudio.clip == m_EngineIdling)
				{
					m_MovementAudio.clip = m_EngineDriving;
					m_MovementAudio.pitch = UnityEngine.Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
					m_MovementAudio.Play();
				}
			}
		}


		private void FixedUpdate ()
		{
			if(m_isMove){
				Move ();
			}
		}

		private void Move ()
		{
			Vector3 movement = m_direction * m_Speed * Time.deltaTime;
			m_TargetPosition = m_Rigidbody.position + movement;
			Vector3 targetPos = Vector3.Lerp(m_Rigidbody.position, m_TargetPosition, Time.deltaTime * m_Speed);
			m_Rigidbody.position = new Vector3(targetPos.x, 0, targetPos.z);


			if (m_isSelf) {
                Syn();
            } else {
				//Vector3 targetPos = Vector3.Lerp(m_Rigidbody.position, m_TargetPosition, Time.deltaTime * m_Speed);
				//m_Rigidbody.position = new Vector3(targetPos.x, 0, targetPos.z);
            }
		}

        private void Syn() {
			m_synTime += Time.deltaTime;

			if (m_synTime >= 0.06f)
			{
				m_moveEvent.Invoke();
				m_synTime = 0;
			}
        }

        void SelfTurn (Vector3 direction) {
            Vector3 target_direction = direction;
			float angle = 90 - Mathf.Atan2(target_direction.z, target_direction.x) * Mathf.Rad2Deg;
			m_Rigidbody.rotation = Quaternion.Lerp (m_Rigidbody.rotation, Quaternion.AngleAxis (angle, Vector3.up), Time.deltaTime * 15f);
        }

		public void OnMove(Vector2 movePos) {
			m_direction = new Vector3 (movePos.x, 0, movePos.y);
			m_direction.Normalize ();

            transform.forward = m_direction;
            m_isMove = true;
		}

        public void OnOtherMove(Vector2 movePos) {
            Vector3 newPos = new Vector3(movePos.x, 0, movePos.y);
            //m_TargetPosition = m_direction;
            Vector3 newDirection = newPos - m_Rigidbody.position;
            m_direction = newDirection;

			m_direction.Normalize();
            newDirection.Normalize();

            transform.forward = newDirection;
			m_isMove = true;
		}

		public void OnMoveEnd() {
            m_isMove = false;
            if (m_isSelf){
                m_stopMoveEvent.Invoke();   
            }
		}

        public void setSpeed (float speed) {
            m_Speed = speed;
        }

		public void OnSpeedupPressed () {
			m_Speed = 25;
            m_speedEvent.Invoke(m_Speed);
		}

		public void OnSpeedupUp () {
			m_Speed = 15;
            m_speedEvent.Invoke(m_Speed);
		}
	}
}