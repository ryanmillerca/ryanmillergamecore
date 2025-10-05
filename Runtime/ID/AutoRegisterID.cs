namespace RyanMillerGameCore
{
    using UnityEngine;

    public class AutoRegisterID : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour idSource;
        
        void OnEnable()
        {
            IHasID hasID = idSource as IHasID;
            if (IDService.Instance)
            {
                IDService.Instance.AddGameObjectWithID(hasID);
            }
        }

        void OnDisable()
        {
            IHasID hasID = idSource as IHasID;
            if (IDService.Instance)
            {
                IDService.Instance.RemoveGameObjectWithID(hasID);
            }
        }
    }
}