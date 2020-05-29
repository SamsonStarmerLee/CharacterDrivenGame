using RotaryHeart.Lib.SerializableDictionary;
using UnityEditorInternal.VersionControl;
using UnityEngine;

namespace Assets.Scripts.Visuals
{
    public enum Overlay
    {
        None,
        Destination,
        Path_Active,
        Path_Inactive,
        Range
    }

    [System.Serializable]
    internal class OverlaySpriteDictionary : SerializableDictionaryBase<Overlay, Sprite> { }

    public class FloorTile : MonoBehaviour
    {
        [SerializeField]
        private OverlaySpriteDictionary spriteMap;

        private SpriteRenderer overlaySprite;

        private void Awake()
        {
            var overlayObject = transform.Find("Overlay");
            overlaySprite = overlayObject.GetComponent<SpriteRenderer>();
        }

        public void SetOverlay(Overlay overlay)
        {
            if (overlay == Overlay.None)
            {
                overlaySprite.sprite = null;
                return;
            }

            overlaySprite.sprite = spriteMap[overlay];
        }
    }
}
