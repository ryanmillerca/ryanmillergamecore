namespace RyanMillerGameCore.Utilities {
    using SceneControl;
    using UnityEngine;

    public class ShowFullscreenFade : MonoBehaviour {
        public void FadeIn() {
            SceneTransitioner.Instance.FadeIn();
        }

        public void FadeOut() {
            SceneTransitioner.Instance.FadeOut();
        }

        public void FadeInAndOut() {
            SceneTransitioner.Instance.FadeInAndOut();
        }
    }
}