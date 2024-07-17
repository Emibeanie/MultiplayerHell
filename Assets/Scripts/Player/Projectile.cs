using System;
using System.Collections;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;

namespace Player
{
    public class Projectile : MonoBehaviourPun
    {
        [SerializeField] private GameObject visualsParent;
        public static event Action<Photon.Realtime.Player, Projectile> OnHitOtherCollider;

        private float _speed;
        private Vector3 _forward;
        
        public void Init(float speed, Vector3 transformForward, float lifeLength)
        {
            _speed = speed;
            _forward = transformForward;

            StartCoroutine(PhotonDestroyAfterDelay(lifeLength));
        }

        private void Update()
        {
            if (!photonView.IsMine) return;
            
            transform.Translate(_forward * (_speed * Time.deltaTime));
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!photonView.IsMine) return;
            
            if (!other.TryGetComponent<PhotonView>(out var otherPhotonView))
            {
                return;
            }
            
            OnHitOtherCollider?.Invoke(otherPhotonView.Owner, this);
        }

        public void TurnOffVisuals()
        {
            visualsParent.SetActive(false);
        }

        public void DestroyAfterDelay(float delay)
        {
            StartCoroutine(PhotonDestroyAfterDelay(delay));
        }
        
        private IEnumerator PhotonDestroyAfterDelay(float lifeLength)
        {
            yield return new WaitForSeconds(lifeLength);
            
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
