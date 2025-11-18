using TMPro;
using UnityEngine;

namespace MiniIT.ARKANOID
{
    public class ScorePopup : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text = null;
        [SerializeField] private float lifeTime = 1f;
        [SerializeField] private float moveSpeed = 1f;

        private float timer = 0f;

        public void Init(int score)
        {
            if (text != null)
            {
                text.text = score.ToString();
            }

            timer = lifeTime;
        }

        private void Update()
        {
            transform.position += Vector3.up * moveSpeed * Time.deltaTime;

            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                Destroy(gameObject);
            }
        }
    }
}
