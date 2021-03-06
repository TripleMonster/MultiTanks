using UnityEngine;

namespace Complete
{
    public class ShellExplosion : MonoBehaviour
    {
        public LayerMask m_TankMask;                        // Used to filter what the explosion affects, this should be set to "Players".
        public ParticleSystem m_ExplosionParticles;         // Reference to the particles that will play on explosion.
        public AudioSource m_ExplosionAudio;                // Reference to the audio that will play on explosion.
        public float m_MaxDamage = 100f;                    // The amount of damage done if the explosion is centred on a tank.
        public float m_ExplosionForce = 1000f;              // The amount of force added to a tank at the centre of the explosion.
        public float m_MaxLifeTime = 0.5f;                    // The time in seconds before the shell is removed.
        public float m_ExplosionRadius = 1f;                // The maximum distance away from the explosion tanks can be and are still affected.
        [HideInInspector] public byte m_index;

        private void Start ()
        {
            Destroy (gameObject, m_MaxLifeTime);
        }

        private void OnTriggerEnter (Collider other)
        {
            if (other.gameObject.layer <= 0)
                return;

            Collider[] colliders = Physics.OverlapSphere(transform.position, m_ExplosionRadius, m_TankMask);
            for (int i = 0; i < colliders.Length; i++)
            {
                Rigidbody targetRigidbody = colliders[i].GetComponent<Rigidbody>();

                if (!targetRigidbody)
                    continue;

                TankHealth targetHealth = targetRigidbody.GetComponent<TankHealth> ();
                if (!targetHealth)
                    continue;

                if (targetHealth.m_index == m_index)
                    continue;
                
                float damage = CalculateDamage (targetRigidbody.position);
                targetHealth.TakeDamage (m_index, damage);
            }
			m_ExplosionParticles.transform.parent = null;
			m_ExplosionParticles.Play();
			m_ExplosionAudio.Play();

			ParticleSystem.MainModule mainModule = m_ExplosionParticles.main;
			Destroy(m_ExplosionParticles.gameObject, mainModule.duration);
			Destroy(gameObject);
		}


        private float CalculateDamage (Vector3 targetPosition)
        {
            Vector3 explosionToTarget = targetPosition - transform.position;
            float explosionDistance = explosionToTarget.magnitude;
            float relativeDistance = (m_ExplosionRadius - explosionDistance) / m_ExplosionRadius;

			float damage = 0.05f * m_MaxDamage;

            damage = Mathf.Max (0f, damage);

            return damage;
        }
    }
}