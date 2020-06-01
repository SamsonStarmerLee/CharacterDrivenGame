using Assets.Scripts.Notifications;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class HealthView : MonoBehaviour
    {
        [SerializeField]
        Image image;

        [SerializeField]
        Sprite[] healthStages;

        private void OnEnable()
        {
            this.AddObserver(OnHealthChanged, GameManager.HealthChangedNotification);
        }

        private void OnDisable()
        {
            this.RemoveObserver(OnHealthChanged, GameManager.HealthChangedNotification);
        }

        private void OnHealthChanged(object sender, object args)
        {
            var health = (args as GameManager).Health;

            if (health <= 0)
            {
                image.sprite = null;
            }
            else
            {
                image.sprite = healthStages[health - 1];
            }
        }
    }
}
