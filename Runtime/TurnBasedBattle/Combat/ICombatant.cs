using System.Collections.Generic;
using UnityEngine;

namespace RyanMillerGameCore.TurnBasedCombat {

	/// <summary>
	/// Interface defining the contract for a combatant in turn-based battle.
	/// </summary>
	public interface ICombatant {

		#region Properties

		string CombatantName { get; }

		int CurrentHp { get; }

		bool IsAlive { get; }

		#endregion

		#region Methods

		void TakeDamage(int dmg);

		void Heal(int amount);

		#endregion
	}
}
