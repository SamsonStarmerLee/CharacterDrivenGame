using Assets.Scripts.Notifications;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class HealthView : MonoBehaviour
    {
        [SerializeField]
        int maxHealth;

        [SerializeField]
        List<Image> slots;

        [SerializeField]
        Sprite full, empty;

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
            var game = (args as GameManager).Health;

            for (var i = 0; i < game; ++i)
            { 
                SetSpriteForHealthSlot(full, i);
            }

            for (var i = game; i < GameManager.MaxHealth; ++i)
            {
                SetSpriteForHealthSlot(empty, i);
            }
        }

        private void SetSpriteForHealthSlot(Sprite sprite, int index)
        {
            if (index >= 0 && index < slots.Count)
            {
                slots[index].sprite = sprite;
            }
        }
    }
}
