namespace RyanMillerGameCore.LevelToys
{
    using System.Collections;
    using System.Collections.Generic;
    using Character;
    using UnityEngine;
    #if UNITY_EDITOR
    using UnityEditor;
    #endif
    
    /// <summary>
    /// Takes control of player movement and moves them to targetPoint
    /// Also has an arc for vertical movement to sell the jumping motion
    /// </summary>
    public class JumpPad : MonoBehaviour
    {
        [Header("Jump Target")] 
        [SerializeField] private Transform targetPoint;

        [Header("Motion")] 
        [SerializeField] private float durationIntro = 0.5f;
        [SerializeField] private float durationTravel = 2;
        [SerializeField] private float arcHeight = 2;
        
        private Vector3 position;

        public void DoJump(Collider other)
        {
            CharacterBrain brain = other.GetComponentInParent<CharacterBrain>();
            if (brain)
            {
                Vector3 start = transform.position;
                Vector3 end = targetPoint.position;
                StartCoroutine(MoveOverTime(brain, start, end));
            }
        }

        private IEnumerator MoveOverTime(CharacterBrain brain, Vector3 start, Vector3 end)
        {
            brain.SetMovementEnabled(false);
            brain.LookAt(targetPoint.position);

            for (float preTravelTime = 0f; preTravelTime < durationIntro; preTravelTime += Time.deltaTime)
            {
                yield return new WaitForEndOfFrame();

                float pt = preTravelTime / durationIntro;
                Vector3 pos = Vector3.Lerp(brain.transform.position, start, pt);
                brain.Teleport(pos);
            }

            for (float travelTime = 0f; travelTime < durationTravel; travelTime += Time.deltaTime)
            {
                yield return new WaitForEndOfFrame();
                float tt = travelTime / durationTravel;
                Vector3 flatPos = Vector3.Lerp(start, end, tt);
                float arcY = Mathf.Sin(tt * Mathf.PI) * arcHeight;
                Vector3 arcedPosition = new Vector3(flatPos.x, flatPos.y + arcY, flatPos.z);
                brain.Teleport(arcedPosition);
            }

            // Final snap to ensure perfect position
            brain.Teleport(end);
            yield return new WaitForEndOfFrame();;
            brain.SetMovementEnabled(true);
        }

        private void OnDrawGizmos()
        {
            if (targetPoint == null) return;
        
            Vector3 start = transform.position;
            Vector3 end = targetPoint.position;
        
            const int resolution = 20;
            var arcPoints = new List<Vector3>();
        
            for (int i = 0; i <= resolution; i++)
            {
                float t = i / (float)resolution;
                Vector3 flat = Vector3.Lerp(start, end, t);
                float arcY = Mathf.Sin(t * Mathf.PI) * arcHeight;
                arcPoints.Add(new Vector3(flat.x, flat.y + arcY, flat.z));
            }
        
        #if UNITY_EDITOR
            Handles.color = Color.cyan;
            Handles.DrawAAPolyLine(6f, arcPoints.ToArray());
        #endif
        
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(start, 0.1f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(end, 0.1f);
        }
    }
}