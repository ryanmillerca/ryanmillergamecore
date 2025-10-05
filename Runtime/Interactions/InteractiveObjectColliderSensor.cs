namespace RyanMillerGameCore.Interactions
{
    using UnityEngine;
    using System.Linq;
    
    public class InteractiveObjectColliderSensor : ColliderSensor
    {
        [SerializeField] private Interactive currentInteractive; 
       
        public Interactive CurrentInteractive => currentInteractive;
        
        protected override void ItemEnteredTrigger(Collider item)
        {
            Interactive interactive = item.GetComponent<Interactive>();
            if (interactive)
            {
                if (interactive.enabled == false)
                {
                    return;
                }
                
                DigestInteractives();

                if (currentInteractive)
                {
                    currentInteractive.SetSelected(true);
                }
            }
        }

        
        /// <summary>
        /// Sorts all the interactives in the collider by distance, closest first
        /// </summary>
        private void DigestInteractives()
        {
            if (currentInteractive)
            {
                currentInteractive.SetSelected(false);
            }

            Collider[] colliders = GetCollidersAsArray();
            colliders = FilterColliders(colliders, requireActiveGameObject: true, requireEnabled: true);

            var interactives = colliders
                .Select(c => c.GetComponent<Interactive>())
                .Where(i => i != null && i.enabled && i.gameObject.activeInHierarchy)
                .OrderBy(i => Vector3.Distance(transform.position, i.transform.position))
                .ToList();

            currentInteractive = interactives.FirstOrDefault();
        }
        
        
        protected override void ItemExitedTrigger(Collider item) 
        {
            Interactive interactive = item.GetComponent<Interactive>();
            if (interactive)
            {
                interactive.SetSelected(false);
                if (interactive == currentInteractive)
                {
                    DigestInteractives();
                }
            }
        }
    }
}