using Boo.Lang;
using RotaryHeart.Lib.SerializableDictionary;
using System.Collections.Generic;
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
        
        public Overlay Overlay { get; private set; }

        private void Awake()
        {
            var overlayObject = transform.Find("Overlay");
            overlaySprite = overlayObject.GetComponent<SpriteRenderer>();
        }

        public void SetOverlay(Overlay ovr)
        {
            if (ovr == Overlay.None)
                overlaySprite.sprite = null;
            else
                overlaySprite.sprite = spriteMap[ovr];

            Overlay = ovr;
        }
    }
}
