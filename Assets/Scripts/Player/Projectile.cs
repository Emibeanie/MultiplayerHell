using System;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;

namespace Player
{
    public class Projectile : MonoBehaviourPun
    {
        public event Action<Collider> OnHitOtherCollider;

        private void OnTriggerEnter(Collider other)
        {
            OnHitOtherCollider?.Invoke(other);
        }
    }
}
