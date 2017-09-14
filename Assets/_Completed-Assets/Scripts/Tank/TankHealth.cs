using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

using UnityEventUtils;

namespace Complete
{
	public class TankHealth : MonoBehaviour
	{
		[HideInInspector] public bool m_isTeamMate;
        [HideInInspector] public UEvent_f m_HealthEvent;
        [HideInInspector] public UEvent_by m_DeathEvent;

		public float m_StartingHealth = 100f;               // The amount of health each tank starts with.
		public Slider m_Slider;                             // The slider to represent how much health the tank currently has.
		public Image m_FillImage;                           // The image component of the slider.
		public Color m_FullHealthColor = Color.green;       // The color the health bar will be when on full health.
		public Color m_ZeroHealthColor = Color.red;         // The color the health bar will be when on no health.
		public GameObject m_ExplosionPrefab;                // A prefab that will be instantiated in Awake, then used whenever the tank dies.

		private AudioSource m_ExplosionAudio;               // The audio source to play when the tank explodes.
		private ParticleSystem m_ExplosionParticles;        // The particle system the will play when the tank is destroyed.
		private float m_CurrentHealth;                      // How much health the tank currently has.
		private bool m_Dead = false;                        // Has the tank been reduced beyond zero health yet?
        private byte m_indexOfDamager;


		private void Awake ()
		{
			m_ExplosionParticles = Instantiate (m_ExplosionPrefab).GetComponent<ParticleSystem> ();

			m_ExplosionAudio = m_ExplosionParticles.GetComponent<AudioSource> ();

			m_ExplosionParticles.gameObject.SetActive (false);

			if (m_HealthEvent == null) {
                m_HealthEvent = new UEvent_f ();
			}

			if (m_DeathEvent == null) {
                m_DeathEvent = new UEvent_by ();
			}
		}

		private void OnEnable()
		{
			m_CurrentHealth = m_StartingHealth;
			m_Dead = false;

			SetHealthUI(m_CurrentHealth);
		}


		public void TakeDamage (byte index, float amount)
		{
			m_CurrentHealth -= amount;

			if (m_HealthEvent != null) {
				m_HealthEvent.Invoke (m_CurrentHealth);
			}

			SetHealthUI (m_CurrentHealth);

			if (m_CurrentHealth <= 0f && !m_Dead)
			{
                OnDeath (index);
			}
		}


		public void SynOtherHealth (float health)
		{
			m_CurrentHealth = health;
			SetHealthUI (m_CurrentHealth);
		}

		public void SetHealthUI (float health)
		{
			m_Slider.value = health;

			m_FillImage.color = Color.Lerp (m_ZeroHealthColor, m_FullHealthColor, m_CurrentHealth / m_StartingHealth);
		}


        private void OnDeath (byte index)
		{
			m_Dead = true;

            m_DeathEvent.Invoke(index);

			m_ExplosionParticles.transform.position = transform.position;
			m_ExplosionParticles.gameObject.SetActive (true);

			m_ExplosionParticles.Play ();

			m_ExplosionAudio.Play();

			Destroy(m_ExplosionParticles.gameObject);
            PlayDeathAnimation();
		}

        void PlayDeathAnimation () {
            MeshRenderer[] renderers = gameObject.GetComponentsInChildren<MeshRenderer>();

			for (int i = 0; i < renderers.Length; i++)
			{
                renderers[i].material.color = Color.black;
			}

            Destroy(gameObject, 1.0f);
        }

		public bool getDeath () {
			return m_Dead;
		}

		public float getCurrentHealth () {
			return m_CurrentHealth;
		}

	}
}