namespace RyanMillerGameCore.Character
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using System.Collections;
    using SaveSystem;
    using Camera;
    using Utilities;
    using Interactions;

    public class CharacterManager : Singleton<CharacterManager>
    {
        [SerializeField] public List<Character> activeCharacters = new List<Character>();
        [SerializeField] public List<Character> inactiveCharacters = new List<Character>();
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Character player;
        [SerializeField] private bool autoRespawnPlayer = true;
        [SerializeField] private float autoRespawnTime = 5;
        [Foldout("Unity Events"), SerializeField] private UnityEvent<Character> characterSpawned;
        [Foldout("Unity Events"), SerializeField] private UnityEvent<Character> characterRemoved;

        private Coroutine _resetSequence;
        private bool _isResetting;

        public Character Player
        {
            get
            {
                if (!player)
                {
                    if (PlayerCharacter.Instance)
                    {
                        player = PlayerCharacter.Instance.Character;
                    }
                    else if (!player) {
                        var playerChar = FindFirstObjectByType<PlayerCharacter>();
                        if (playerChar) {
                            player = playerChar.Character;
                        }
                        if (!player) {
                            //Debug.Log("Instantiating new Player Character from prefab.");
                            GameObject newPlayer = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                            player = newPlayer.GetComponent<Character>();
                            StartCoroutine(PostPlayerSpawn());
                        }
                    }
                }
                return player;
            }
        }

        private IEnumerator PostPlayerSpawn() {
            yield return new WaitForEndOfFrame();
            MovePlayerToSpawnPoint();
        }

        private void MovePlayerToSpawnPoint() {
            var checkPoint = DataManager.Instance.TryGetCheckpoint();
            if (checkPoint != null) {
                //Debug.Log("Moving player to saved checkpoint at " + checkPoint.position);
                Player.characterMovement.Teleport(checkPoint.position, checkPoint.rotation);
                return;
            }
            var setCheckpoints = FindObjectsByType<Checkpoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            //Debug.Log("No saved checkpoint found, looking for SetCheckpoints in scene.");
            foreach (var setCheck in setCheckpoints) {
                if (setCheck.IsSpawnPoint) {
                    //Debug.Log("Found spawn point at " + setCheck.transform.position + ", moving player.");
                    Player.characterMovement.Teleport(setCheck.transform.position, setCheck.transform.rotation);
                    return;
                }
            }
            //Debug.Log("No spawn point found, using first SetCheckpoint in scene.");
            var setCheckPoint = FindFirstObjectByType<Checkpoint>();
            Player.characterMovement.Teleport(setCheckPoint.transform.position, setCheckPoint.transform.rotation);
        }

        public void RegisterCharacter(Character character)
        {
            if (character.gameObject.activeInHierarchy)
            {
                if (activeCharacters.Contains(character) == false)
                {
                    activeCharacters.Add(character);
                    character.OnDied += OnCharacterDied;
                }
            }
        }

        private void OnCharacterDied(Character chara)
        {
            if (_isResetting == true)
            {
                return;
            }

            if (autoRespawnPlayer)
            {
                if (chara.IsPlayer())
                {
                    if (_resetSequence != null)
                    {
                        StopCoroutine(_resetSequence);
                    }
                    _resetSequence = StartCoroutine(ResetSequence());
                }
            }
        }

        IEnumerator ResetSequence()
        {
            _isResetting = true;
            yield return new WaitForSeconds(autoRespawnTime);
            CheckpointData checkpointData = DataManager.Instance.TryGetCheckpoint();
            if (checkpointData != null)
            {
                Player.transform.position = checkpointData.position;
                CameraController.Instance.TargetYRotation = checkpointData.cameraRotation;
            }
            Player.Respawn();
            _isResetting = false;
        }

        public Character GetCharacter(CharacterID ID)
        {
            if (ID == null)
            {
                return null;
            }
            foreach (Character character in activeCharacters)
            {
                if (character && character.ID().Equals(ID))
                {
                    return character;
                }
            }
            Debug.LogWarning("Character with ID " + ID.name + "(" + ID.GetHashCode() + ") not found", gameObject);
            return null;
        }

        public void SpawnCharacter(Character character, Vector3 position, Quaternion rotation)
        {
            // try to find from pool
            Character characterInstance = GetCharacter(character.ID());
            if (characterInstance)
            {
                inactiveCharacters.Remove(characterInstance);
                activeCharacters.Add(characterInstance);
                characterInstance.gameObject.SetActive(true);
                characterInstance.transform.SetParent(null);
                characterInstance.transform.SetPositionAndRotation(position, rotation);
                characterInstance.Reset();
                characterSpawned?.Invoke(characterInstance);
            }
            // otherwise instantiate a new one
            else
            {
                GameObject newCharacter = Instantiate(character.gameObject, position, rotation);
                var characterComponent = newCharacter.GetComponent<Character>();
                activeCharacters.Add(characterComponent);
                characterSpawned?.Invoke(characterComponent);
            }
        }
        
        
        public void RemoveCharacter(Character character, bool destroy = false)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            inactiveCharacters.Add(character);
            character.OnDied -= OnCharacterDied;

            if (destroy)
            {
                Destroy(character.gameObject);
                return;
            }

            activeCharacters.Remove(character);
            character.gameObject.SetActive(false);
            characterRemoved?.Invoke(character);
        }
    }
}