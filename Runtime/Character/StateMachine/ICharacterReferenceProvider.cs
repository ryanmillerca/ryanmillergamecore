namespace RyanMillerGameCore.Character.SMB {
	using UnityEngine;
	using Animation;
	using Interactions;

	public interface ICharacterReferenceProvider {
		CharacterMovement Movement { get; set; }
		Collider MainCollider { get; set; }
		CharacterInput CharacterInput { get; set; }
		CharacterAnimation CharacterAnimation { get; set; }
		Character Character { get; set; }
		CharacterBrain CharacterBrain { get; set; }
		PlayerCharacter PlayerCharacter { get; set; }
		ColliderSensor AttackColliderSensor { get; set; }
		ColliderSensor AggroColliderSensor { get; set; }
		InteractiveObjectColliderSensor InteractColliderSensor { get; set; }
		Rigidbody Rb { get; set; }
		DamageDealer DamageDealer { get; set; }
		Animator Animator { get; set; }
		Renderer[] Renderers { get; set; }
		CharacterPathfind CharacterPathfind { get; set; }
		ColliderSensor GetColliderSensor(ColliderSensorType sensorType);
		CharacterAnimParamMappings CharacterAnimParamMappings { get; set; }
	}
}
