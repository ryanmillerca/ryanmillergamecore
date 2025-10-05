namespace RyanMillerGameCore.Dialog
{
    using UnityEngine;
    using System;
    using System.Collections.Generic;

    public class DialogManager : Singleton<DialogManager>
    {
        [SerializeField] private List<DialogStylePair> dialogPlayers = new List<DialogStylePair>();

        private Dictionary<DialogStyle, DialogPlayer> _dialogLookup;

        private void Start()
        {
            _dialogLookup = new Dictionary<DialogStyle, DialogPlayer>(dialogPlayers.Count);
            foreach (var pair in dialogPlayers)
            {
                if (!_dialogLookup.ContainsKey(pair.dialogStyle))
                {
                    _dialogLookup.Add(pair.dialogStyle, pair.dialogPlayer);
                }
            }
        }

        public DialogPlayer GetDialogPlayer(DialogStyle key)
        {
            if (_dialogLookup.TryGetValue(key, out var player))
            {
                return player;
            }

            Debug.LogWarning($"No DialogPlayer found for key '{key}' in {name}.");
            return null;
        }
    }

    [Serializable]
    public class DialogStylePair
    {
        public DialogStyle dialogStyle;
        public DialogPlayer dialogPlayer;
    }
}