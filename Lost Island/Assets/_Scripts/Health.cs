using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour {

	public float maxHealth;
	[HideInInspector] public float health;

	HiveMind hive;

	void Start () {
		health = maxHealth;

		hive = GameObject.Find("_ScriptHolder_").GetComponent<HiveMind>();
	}
	
	public void TakeDamage(float amount) {
		health -= amount;
		if(health <= 0f) {
			Die();
		}
	}

	void Die() {
		Destroy(gameObject);
	}
}
