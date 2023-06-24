using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bucket : MonoBehaviour {

	[SerializeField] bool waterBucket;

	[SerializeField] Item bucketItem;
	[SerializeField] Item waterBucketItem;

	[SerializeField] Transform waterLevel;


	float fillTime;
	float timeToFill = 10f;

	float nextTimeToWorldCheck;
	float worldCheckInterval = 1f;

	void Start() {
		if(waterBucket) {
			fillTime = timeToFill;
		}
	}

	void Update() {
		if(transform.up.y <= 0.6f) {
			fillTime += Time.deltaTime + Mathf.Min(0f, (transform.up.y - 0.65f) / 2f);
		}
		waterLevel.transform.localPosition = Vector3.Lerp(Vector3.up * -0.49f, Vector3.zero, fillTime / timeToFill);
		waterLevel.transform.localScale = Vector3.Lerp(Vector3.one * 0.82f, Vector3.one, fillTime / timeToFill);
		if(fillTime <= 0f) {
			if(waterBucket) {
				EmptyWater();
			} else {
				fillTime = 0f;
			}
		} else if(fillTime >= timeToFill) {
			if(waterBucket) {
				fillTime = timeToFill;
			} else {
				FillWithWater();
			}
		}
		if(fillTime <= 0f) {
			if(waterLevel.gameObject.activeSelf) {
				waterLevel.gameObject.SetActive(false);
			}
		} else {
			if(!waterLevel.gameObject.activeSelf) {
				waterLevel.gameObject.SetActive(true);
			}
		}

		if(Time.time >= nextTimeToWorldCheck) {
			nextTimeToWorldCheck = Time.time + worldCheckInterval;
		}
	}

	void EmptyWater() {
		Instantiate(bucketItem.prefab, transform.position, transform.rotation);
		Destroy(gameObject);
	}

	void FillWithWater() {
		Instantiate(waterBucketItem.prefab, transform.position, transform.rotation);
		Destroy(gameObject);
	}
}
