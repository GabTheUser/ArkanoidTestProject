using System;
using UnityEngine;

namespace MiniIT.ARKANOID
{
    public class Brick : MonoBehaviour
    {
        [Header("Параметры кирпича")]
        [SerializeField] private int hitPoints = 1;
        [SerializeField] private int scoreValue = 100;

        [Header("Визуальные стадии повреждения")]
        [SerializeField] private GameObject[] damageVisuals = null;

        [Header("Аудио")]
        [SerializeField] private AudioClip hitSound = null;
        [SerializeField] private AudioClip destroySound = null;
        [SerializeField] private float soundVolume = 1f;

        [Header("Эффекты")]
        [SerializeField] private GameObject hitVfxPrefab = null;
        [SerializeField] private GameObject destroyVfxPrefab = null;

        [Header("Попап очков")]
        [SerializeField] private ScorePopup scorePopupPrefab = null;

        public int ScoreValue
        {
            get { return scoreValue; }
        }

        public event Action<Brick> BrickDestroyed;

        private int maxHitPoints = 1;

        private void Awake()
        {
            maxHitPoints = hitPoints;

            if (damageVisuals != null)
            {
                for (int i = 0; i < damageVisuals.Length; i++)
                {
                    if (damageVisuals[i] != null)
                    {
                        damageVisuals[i].SetActive(false);
                    }
                }
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.collider.CompareTag("Ball"))
            {
                return;
            }

            int damage = 1;

            BallController ball = collision.collider.GetComponent<BallController>();
            if (ball != null)
            {
                damage = ball.CurrentDamage;
            }

            TakeHit(damage);
        }

        protected void TakeHit(int damage)
        {
            if (damage <= 0)
            {
                return;
            }

            int newHitPoints = hitPoints - damage;
            bool willDie = newHitPoints <= 0;

            hitPoints = newHitPoints;

            if (willDie)
            {
                PlayDestroyEffects();
                OnDestroyed();
            }
            else
            {
                UpdateDamageVisual();
                PlayHitEffects();
            }
        }

        private void UpdateDamageVisual()
        {
            if (damageVisuals == null || damageVisuals.Length == 0)
            {
                return;
            }

            int hitsTaken = maxHitPoints - hitPoints;
            int stageIndex = hitsTaken - 1;

            if (stageIndex < 0 || stageIndex >= damageVisuals.Length)
            {
                return;
            }

            for (int i = 0; i < damageVisuals.Length; i++)
            {
                if (damageVisuals[i] != null)
                {
                    damageVisuals[i].SetActive(i == stageIndex);
                }
            }
        }

        private void PlayHitEffects()
        {
            if (hitSound != null)
            {
                AudioSource.PlayClipAtPoint(hitSound, transform.position, soundVolume);
            }

            if (hitVfxPrefab != null)
            {
                Instantiate(hitVfxPrefab, transform.position, Quaternion.identity);
            }
        }

        private void PlayDestroyEffects()
        {
            if (destroySound != null)
            {
                AudioSource.PlayClipAtPoint(destroySound, transform.position, soundVolume);
            }

            if (destroyVfxPrefab != null)
            {
                Instantiate(destroyVfxPrefab, transform.position, Quaternion.identity);
            }

            if (scorePopupPrefab != null)
            {
                ScorePopup popup = Instantiate(
                    scorePopupPrefab,
                    transform.position,
                    Quaternion.identity
                );
                popup.Init(scoreValue);
            }
        }

        protected virtual void OnDestroyed()
        {
            if (BrickDestroyed != null)
            {
                BrickDestroyed(this);
            }

            Destroy(gameObject);
        }

        public void ForceDestroy()
        {
            hitPoints = 0;
            PlayDestroyEffects();
            OnDestroyed();
        }
    }
}
