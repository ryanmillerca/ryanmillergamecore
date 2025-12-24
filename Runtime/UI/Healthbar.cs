namespace RyanMillerGameCore.UI
{
    using Character;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.UI;
    
    public class Healthbar : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image healthBar;
        [SerializeField] private Image healthBarFlash;
        [SerializeField] private float barLerpSpeed = 10;
        [SerializeField] private float flashInTime = 0.1f;
        [SerializeField] private float flashOutTime = 0.2f;
        
        private Vector2 _healthBarSize;
        private Vector3 _targetSizeDelta;
        private Character _playerCharacter;
        private Coroutine _flashHealthBarCoroutine;

        private void Awake()
        {
            _healthBarSize = healthBar.rectTransform.sizeDelta;
            _targetSizeDelta = _healthBarSize;
        }
        
        private void Start()
        {
            _playerCharacter = CharacterManager.Instance.Player;
            _playerCharacter.OnReceiveDamage += OnHurt;
            _playerCharacter.OnReceiveHeal += OnHeal;
            _playerCharacter.Spawned += OnRespawn;
        }

        public void SetVisible(bool visible)
        {
            canvasGroup.alpha = visible ? 1 : 0;
        }

        private void OnDestroy()
        {
            if (_playerCharacter)
            {
                _playerCharacter.OnReceiveDamage -= OnHurt;
                _playerCharacter.OnReceiveHeal -= OnHeal;
            }
        }

        private void OnHurt(float hurtAmount)
        {
            if (_flashHealthBarCoroutine != null)
            {
                StopCoroutine(_flashHealthBarCoroutine);
            }
            _flashHealthBarCoroutine = StartCoroutine(FlashHealthbar());
            _targetSizeDelta = new Vector2(
                _healthBarSize.x * _playerCharacter.percentHealth, 
                _healthBarSize.y);
        }
        
        private void OnHeal(float healAmount)
        {
            if (_flashHealthBarCoroutine != null)
            {
                StopCoroutine(_flashHealthBarCoroutine);
            }
            _flashHealthBarCoroutine = StartCoroutine(FlashHealthbar());
            _targetSizeDelta = new Vector2(
                _healthBarSize.x * _playerCharacter.percentHealth, 
                _healthBarSize.y);
        }

        private void OnRespawn()
        {
            _targetSizeDelta = new Vector3(_healthBarSize.x, _healthBarSize.y);
            if (_flashHealthBarCoroutine != null)
            {
                StopCoroutine(_flashHealthBarCoroutine);
            }
            _flashHealthBarCoroutine = StartCoroutine(FlashHealthbar());
        }

        private void Update()
        {
            healthBar.rectTransform.sizeDelta = Vector2.Lerp(healthBar.rectTransform.sizeDelta, _targetSizeDelta, Time.unscaledDeltaTime * barLerpSpeed);
        }

        private IEnumerator FlashHealthbar()
        {
            healthBarFlash.enabled = true;
            for (float f = 0; f < flashInTime; f += Time.deltaTime)
            {
                healthBarFlash.color = new Color(1, 1, 1, f/(flashInTime));
                yield return new WaitForEndOfFrame();
            }
            for (float f = 0; f < flashOutTime; f += Time.deltaTime)
            {
                healthBarFlash.color = new Color(1, 1, 1, 1-f/flashOutTime);
                yield return new WaitForEndOfFrame();
            }
            healthBarFlash.color = new Color(1, 1, 1, 0);
            healthBarFlash.enabled = false;
        }
    }
}