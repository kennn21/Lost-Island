using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiveMind : MonoBehaviour {

	public List<ResourceHandler> worldResources = new List<ResourceHandler>();
	public List<GameObject> worldEnemies = new List<GameObject>();

	public void AddResource(ResourceHandler handler) {
		worldResources.Add(handler);
	}

	public void RemoveResource(ResourceHandler handler) {
		worldResources.Remove(handler);
	}

	public void AddEnemy(GameObject handler)
    {
		worldEnemies.Add(handler);
    }
	public void RemoveEnemy(GameObject handler)
	{
		worldEnemies.Remove(handler);
	}

    private void Update()
    {
		for (var i = worldEnemies.Count - 1; i > -1; i--)
		{
			if (worldEnemies[i] == null)
				worldEnemies.RemoveAt(i);
		}
	}
}
