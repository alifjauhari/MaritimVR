using System.Collections;
using UnityEngine;

namespace Autohand.Demo
{
    [DisallowMultipleComponent]
    public class BulletTrailFX : MonoBehaviour
    {
        [Header("Prefab Parts")]
        [SerializeField] private Transform trail;        // your "Trail" child (mesh or sprite etc)
        [SerializeField] private GameObject explosion;   // your "Explosion" child (ParticleSystem or FX)
        [SerializeField] private bool parentExplosionToHit = true;

        [Header("Motion")]
        [SerializeField] private float unitsPerSecond = 120f;  // visual travel speed

        // optional (if you use TrailRenderer and want to clear on reuse)
        [SerializeField] private TrailRenderer trailRenderer;

        // pool callback assigned by spawner
        private System.Action<BulletTrailFX> _onDone;

        public void Play(
            Vector3 start,
            Vector3 end,
            bool hitSomething,
            Vector3 hitNormal,
            Transform hitParent,
            System.Action<BulletTrailFX> onDone
        )
        {
            StopAllCoroutines();
            _onDone = onDone;

            if (trailRenderer != null) trailRenderer.Clear();
            if (explosion != null) { explosion.SetActive(false); explosion.transform.SetParent(transform, true); }

            gameObject.SetActive(true);
            if (trail != null) trail.gameObject.SetActive(true);

            transform.position = start;
            transform.rotation = Quaternion.LookRotation((end - start).normalized, Vector3.up);

            StartCoroutine(Co_Run(start, end, hitSomething, hitNormal, hitParent));
        }

        private IEnumerator Co_Run(Vector3 start, Vector3 end, bool hitSomething, Vector3 hitNormal, Transform hitParent)
        {
            float dist = Vector3.Distance(start, end);
            float dur = dist / Mathf.Max(0.01f, unitsPerSecond);
            float t = 0f;

            // fly the trail
            while (t < dur)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / dur);
                transform.position = Vector3.Lerp(start, end, u);
                yield return null;
            }

            // impact FX
            if (hitSomething && explosion != null)
            {
                if (parentExplosionToHit && hitParent != null)
                    explosion.transform.SetParent(hitParent, true);

                explosion.transform.position = end;
                if (hitNormal.sqrMagnitude > 0.0001f)
                    explosion.transform.rotation = Quaternion.LookRotation(hitNormal);

                explosion.SetActive(true);

                // play all ParticleSystems and wait for them to finish
                var psList = explosion.GetComponentsInChildren<ParticleSystem>(true);
                float wait = 0f;
                for (int i = 0; i < psList.Length; i++)
                {
                    var ps = psList[i];
                    ps.Play(true);
                    var main = ps.main;
                    float d = main.duration;
                    // support both constant and curve startLifetime (take max)
                    float life = 0f;
                    if (main.startLifetime.mode == ParticleSystemCurveMode.TwoConstants)
                        life = main.startLifetime.constantMax;
                    else if (main.startLifetime.mode == ParticleSystemCurveMode.Constant)
                        life = main.startLifetime.constant;
                    else
                        life = main.startLifetime.constantMax; // safe fallback

                    wait = Mathf.Max(wait, d + life);
                }
                if (wait > 0f)
                    yield return new WaitForSeconds(wait);
            }

            // reset small bits for reuse
            if (trail != null) trail.gameObject.SetActive(true);
            if (explosion != null) { explosion.SetActive(false); explosion.transform.SetParent(transform, true); }

            // back to pool
            _onDone?.Invoke(this);
        }
    }
}
