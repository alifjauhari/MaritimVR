using System.Collections.Generic;
using UnityEngine;

namespace Autohand.Demo
{
    public class Pistol : MonoBehaviour
    {
        public Rigidbody body;

        public Transform barrelTip;
        public float hitPower = 1;
        public float recoilPower = 1;
        public float range = 100;
        public LayerMask layer;

        public AudioClip shootSound;
        public float shootVolume = 1f;

        // --- NEW: visual bullet + pool ---
        [Header("Bullet Visual FX")]
        [SerializeField] private BulletTrailFX bulletPrefab;
        [SerializeField] private int prewarmCount = 8;
        [SerializeField] private Transform poolParent;    // optional; will be created if null
        private readonly Queue<BulletTrailFX> bulletPool = new Queue<BulletTrailFX>();

        private void Start()
        {
            if (body == null && TryGetComponent(out Rigidbody rb))
                body = rb;

            // --- NEW: prewarm pool ---
            if (bulletPrefab != null)
            {
                if (poolParent == null)
                {
                    var p = new GameObject("BulletPool");
                    poolParent = p.transform;
                    poolParent.SetParent(transform, false);
                }
                for (int i = 0; i < Mathf.Max(1, prewarmCount); i++)
                    EnqueueNew();
            }
        }

        // --- pooling helpers ---
        private void EnqueueNew()
        {
            var fx = Instantiate(bulletPrefab, poolParent);
            fx.gameObject.SetActive(false);
            bulletPool.Enqueue(fx);
        }
        private BulletTrailFX GetBullet()
        {
            if (bulletPool.Count == 0) EnqueueNew();
            var fx = bulletPool.Dequeue();
            fx.gameObject.SetActive(true);
            return fx;
        }
        private void ReturnBullet(BulletTrailFX fx)
        {
            fx.gameObject.SetActive(false);
            fx.transform.SetParent(poolParent, true);
            bulletPool.Enqueue(fx);
        }

        public void Shoot()
        {
            //Play the audio sound
            if (shootSound)
                AudioSource.PlayClipAtPoint(shootSound, transform.position, shootVolume);

            // Raycast first (your original hit logic)
            RaycastHit hit;
            bool hitSomething = Physics.Raycast(barrelTip.position, barrelTip.forward, out hit, range, layer);

            if (hitSomething)
            {
                var hitBody = hit.transform.GetComponent<Rigidbody>();
                Debug.Log("Hit: " + hit.transform.name);
                if (hitBody != null)
                {
                    Debug.DrawRay(barrelTip.position, (hit.point - barrelTip.position), Color.green, 5);
                    hitBody.GetComponent<Smash>()?.DoSmash();
                    hitBody.AddForceAtPosition((hit.point - barrelTip.position).normalized * hitPower * 10, hit.point, ForceMode.Impulse);
                }
            }
            else
            {
                Debug.DrawRay(barrelTip.position, barrelTip.forward * range, Color.red, 1);
            }

            // --- NEW: spawn/animate bullet visual to the destination ---
            if (bulletPrefab != null)
            {
                Vector3 endPoint = hitSomething ? hit.point : (barrelTip.position + barrelTip.forward * range);
                Vector3 hitNormal = hitSomething ? hit.normal : Vector3.forward; // fallback
                Transform hitParent = hitSomething ? hit.transform : null;

                var fx = GetBullet();
                fx.Play(
                    barrelTip.position,
                    endPoint,
                    hitSomething,
                    hitNormal,
                    hitParent,
                    onDone: ReturnBullet
                );
            }

            // recoil (original)
            body.AddForce(barrelTip.transform.up * recoilPower * 5, ForceMode.Impulse);
        }
    }
}
