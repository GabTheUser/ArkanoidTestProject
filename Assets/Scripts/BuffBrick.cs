using UnityEngine;

namespace MiniIT.ARKANOID
{
    public class BuffBrick : Brick
    {
        [Header("Бафф урона мяча")]
        [SerializeField] private int damageBuffAmount = 1;
        [SerializeField] private float buffDuration = 5f;

        protected override void OnDestroyed()
        {
            ApplyBuffToBall();
            base.OnDestroyed();
        }

        private void ApplyBuffToBall()
        {
            BallController ball = FindFirstObjectByType<BallController>();

            if (ball != null)
            {
                ball.AddTemporaryDamageBuff(damageBuffAmount, buffDuration);
            }
        }
    }
}
