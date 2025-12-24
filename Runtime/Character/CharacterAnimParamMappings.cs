namespace RyanMillerGameCore.Character {
	using UnityEngine;

	[CreateAssetMenu(fileName = "Character Anim Param Mapping", menuName = "RyanMillerGameCore/Character/Anim Param Mapping")]
	public class CharacterAnimParamMappings : ScriptableObject {
		public string m_ParamTriggerHurt = "hurt";
		public string m_ParamBoolDead = "isDead";
		public string m_ParamBoolAggro = "HasAggro";
		public string m_ParamSpeedHorizontal = "speed_horizontal";
		public string m_ParamTriggerAttack = "attack";
		public string m_ParamTriggerInteract = "interact";
	}
}
