using System;
using Photon.Pun;
using UnityEngine;

namespace Player
{
    public class PlayerShooter : MonoBehaviourPun
    {
        [SerializeField] private Transform projectileOrigin;
        
        private readonly string _bulletPrefabPath = "Bullet";
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Shoot();
            }
        }

        private void Shoot()
        {
            var bullet = PhotonNetwork.Instantiate(_bulletPrefabPath, projectileOrigin.position, Quaternion.identity);
            
            bullet.GetComponent<Projectile>().OnHitOtherCollider += HandleOnHitOtherCollider;
        }

        private void HandleOnHitOtherCollider(Collider other)
        {
            
        }
    }
}
