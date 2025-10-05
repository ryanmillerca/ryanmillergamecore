namespace RyanMillerGameCore.Dialog
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "RyanMillerGameCore/Dialog/Dialog Skins")]
    public class DialogSkins : ScriptableObject
    {
        public SpeakerSkin DefaultSkin => defaultSkin;
        public SpeakerSkin[] SpeakerSkins => speakerSkins;

        [SerializeField] private SpeakerSkin defaultSkin = new SpeakerSkin(new Color(0, 0, 0, 0.75f), new Color(1, 1, 1, 1), 3);
        [SerializeField] private SpeakerSkin[] speakerSkins;
    }

    #region SpeakerSkin

    [System.Serializable]
    public class SpeakerSkin
    {
        public ID speaker;
        public AudioClip[] sounds;
        public Color frameColor;
        public Color bodyTextColor;
        public Color characterTextColor = Color.white;

        public float pitchVariance;
        public int rate;

        public SpeakerSkin(Color frameColor, Color bodyTextColor, int rate)
        {
            this.frameColor = frameColor;
            this.bodyTextColor = bodyTextColor;
            this.rate = rate;
        }
    }

    #endregion

}