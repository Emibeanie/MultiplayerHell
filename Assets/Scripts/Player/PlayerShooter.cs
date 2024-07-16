using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;

namespace Player
{
    public class PlayerShooter : MonoBehaviourPun
    {
        [SerializeField] private Transform projectileOrigin;
        [SerializeField] private float bulletSpeed = 10f;
        [SerializeField] private int bulletDamage = 8;
        [SerializeField] private float bulletLifeLength = 15f;
        [SerializeField] private float timeToDestroyAfterImpact = 3f;
        [SerializeField] private int maxHealth = 100;
        
        private const string BulletPrefabPath = "Bullet";
        
        private int _health;

        private void Awake()
        {
            _health = maxHealth;
            
            Projectile.OnHitOtherCollider += HandleOnHitOtherCollider;
        }

        private void Update()
        {
            if (!photonView.IsMine)
            {
                return;
            }
            
            if (Input.GetMouseButtonDown(0))
            {
                Shoot();
            }
        }

        private void OnDestroy()
        {
            Projectile.OnHitOtherCollider -= HandleOnHitOtherCollider;
        }

        private void Shoot()
        {
            var bullet = PhotonNetwork.Instantiate(BulletPrefabPath, projectileOrigin.position, Quaternion.identity);
            
            var proj = bullet.GetComponent<Projectile>();
            proj.Init(bulletSpeed, projectileOrigin.forward, bulletLifeLength);
        }

        private void HandleOnHitOtherCollider(Photon.Realtime.Player otherPlayer, Projectile bullet)
        {
            Debug.Log($"Here 1");
            if (bullet.photonView.Owner.ActorNumber == otherPlayer.ActorNumber)
            {
            Debug.Log($"Here 2");
                return;
            }

            Debug.Log($"Here 3");
            if (otherPlayer.ActorNumber == photonView.Owner.ActorNumber)
            {
            Debug.Log($"Here 4");
                TakeDamage(bulletDamage);
            }

            Debug.Log($"Here 5");
            if (!photonView.IsMine) return;
            
            Debug.Log($"Here 6");
            
            bullet.TurnOffVisuals();
            bullet.DestroyAfterDelay(timeToDestroyAfterImpact);
        }
        
        private IEnumerator PhotonDestroyAfterDelay(float delay)
        {
            Debug.Log($"Here 7");
            yield return new WaitForSeconds(delay);
            
            Debug.Log($"Here 8");
            PhotonNetwork.Destroy(gameObject);
        }
        
        private void TakeDamage(int damage)
        {
            _health -= damage;
            Debug.Log($"Here 9 {_health}");

            if (!photonView.IsMine)
            {
                return;
            }
            
            if (_health <= 0)
            {
            Debug.Log($"Here 10 {_health}");
                Die();
            }
            
            photonView.RPC(nameof(PlayerTookDamageRpc), RpcTarget.Others, damage);
        }

        private void Die()
        {
            StartCoroutine(PhotonDestroyAfterDelay(3f));
        }

        [PunRPC]
        private void PlayerTookDamageRpc(int damageTaken)
        {
            TakeDamage(damageTaken);
            Debug.Log($"{name} took {damageTaken} damage");
        }
    }
}
