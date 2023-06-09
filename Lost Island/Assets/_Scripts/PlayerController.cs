using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.AI;
using EZCameraShake;
using UnityEngine.SceneManagement;


public class PlayerController : MonoBehaviour {


	public GameObject playerCameraHolder;
	public GameObject playerCamera;
	public GameObject handContainer;
	public Animator handAnimator;
	public Animation damagePanelAnim;
	public GameObject infoText;

	[HideInInspector] public Rigidbody rb;

	[Header("Keybinds")]
	public KeyCode jumpKey = KeyCode.Space;
	public KeyCode sprintKey = KeyCode.LeftShift;
	public KeyCode crouchKey = KeyCode.LeftControl;

	public float mouseSensitivityX;
	public float mouseSensitivityY;
	public float walkSpeed;
	public float runSpeed;
	public float initialRunSpeed;
	public float jumpForce;
	public float smoothTime;
	public LayerMask groundedMask;
	public float interactRange = 2f;
	public Item fuelItem;

	public GameObject progressContainer;
	public Image progressImage;
	public Text progressText;

	public Text noticeText;

	public Transform purgatorySpawn;
	public TextMesh purgatoryRespawnTimeText;

	public GameObject lightDeactivateObjects;
	public GameObject darkDeactivateObjects;

	public Transform lightWorldEnterPoint;

	public Text realmNoticeText;
	public Color lightRealmNoticeTextColor;

	public ParticleSystem useParticles;

	private float verticalLookRotation;

	private float currentMoveSpeed;
	private Vector3 moveAmount;
	private Vector3 smoothMoveVelocity;

	[HideInInspector] public Inventory inventory;
	AudioManager audioManager;
	SettingsManager settingsManager;

	public GameObject canvas;
	Animator canvasAnim;

	[HideInInspector] public bool grounded;

	[HideInInspector] public float distanceToTarget; // Accesible from other GameObjects to see if they are in range of interaction and such
	[HideInInspector] public GameObject target;
	[HideInInspector] public RaycastHit targetHit;

	GameObject currentHandObj;

	private Vector3 spawnpoint;

	private Quaternion startRot;
	
	bool lockLook;
	bool gathering = false;
	float gatheringTime;

	bool pickingUp = false;
	float pickingUpTime;

	bool consuming = false;
	float consumeTime;

	bool firing = false;
	float drawTime;

	int difficulty;
	int mode;

	Color defaultFogColor;
	float defaultFogDensity;
	Color defaultPlayerCameraColor;

	float nextTimeToRespawn;
	float respawnTime = 15f;

	[HideInInspector] public bool dead = false;

	public Image healthAmountImage;
	public Image hungerAmountImage;

	public float maxHealth = 100f;
	public float health;
	public float maxHunger = 100f;
	public float hunger;

	public float hungerLoss = 0.1f;
	public float hungerDamage = 2f;

	public float fullLevel = 90f;
	public float fullHealthRegainAmount = 1f;

	public float impactVelocityToDamage = 1f;
	public float impactDamage = 20f;

	public float handDamage = 15f;

	bool ignoreFallDamage;
	bool climbing;

	public GameObject rain;

	float flyDoubleTapCooldown;
	bool flying;

	WeaponHandler currentWeaponHandler;
	Animator currentWeaponAnimator;

	ResourceHandler currentResource;

	float originalDrag;
	float flyingDrag = 10f;


	void Awake() {
		GameObject scriptHolder = GameObject.FindGameObjectWithTag("ScriptHolder");
		audioManager = scriptHolder.GetComponent<AudioManager>();
		settingsManager = scriptHolder.GetComponent<SettingsManager>();
		canvasAnim = canvas.GetComponent<Animator>();

		rb = GetComponent<Rigidbody>();
		inventory = GetComponent<Inventory>();
		originalDrag = rb.drag;

		defaultFogColor = RenderSettings.fogColor;
		defaultFogDensity = RenderSettings.fogDensity;
		defaultPlayerCameraColor = playerCamera.GetComponent<Camera>().backgroundColor;
	}

	void Start() {
		health = maxHealth;
		hunger = maxHunger;
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		LockLook(false);
	}

	public void InventoryUpdate() {
		if(inventory.currentSelectedItem) {
			if(currentHandObj) {
				Destroy(currentHandObj);
			}
			GameObject obj = Instantiate(inventory.currentSelectedItem.prefab, handContainer.transform) as GameObject;
			obj.transform.Rotate(inventory.currentSelectedItem.handRotation);
			obj.transform.localScale = inventory.currentSelectedItem.handScale;
			Rigidbody objRB = obj.GetComponent<Rigidbody>();
			if(objRB) {
				objRB.isKinematic = true;
			}
			Collider[] cols = obj.GetComponentsInChildren<Collider>();
			if(cols.Length > 0) {
				foreach(Collider col in cols) {
					col.enabled = false;
				}
			}
			AudioSource[] audioSources = obj.GetComponentsInChildren<AudioSource>();
			if(audioSources.Length > 0) {
				foreach(AudioSource audio in audioSources) {
					audio.enabled = false;
				}
			}
			if(inventory.currentSelectedItem.id == 23) { // AutoMiner
				obj.GetComponent<AutoMiner>().enabled = false;
				obj.GetComponent<NavMeshAgent>().enabled = false;
			} else if(inventory.currentSelectedItem.id == 32 || inventory.currentSelectedItem.id == 33) { // Bucket or WaterBucket
				Bucket bucket = obj.GetComponent<Bucket>();
				if(bucket) {
					bucket.enabled = false;
				}
			} else if(inventory.currentSelectedItem.id == 20) { // Lightning Stone
				LightningStone ls = obj.GetComponent<LightningStone>();
				if(ls) {
					ls.enabled = false;
				}
			} else if(inventory.currentSelectedItem.id == 35) {
				LightItem li = obj.GetComponent<LightItem>();
				if(li) {
					li.enabled = false;
				}
			}
			obj.tag = "Untagged";
			foreach(Transform trans in obj.transform) {
				trans.tag = "Untagged";
			}

			currentHandObj = obj;

			if(inventory.currentSelectedItem.type == Item.ItemType.Weapon) {
				currentWeaponHandler = currentHandObj.GetComponent<WeaponHandler>();
				currentWeaponAnimator = currentHandObj.GetComponent<Animator>();
			} else {
				currentWeaponHandler = null;
				currentWeaponAnimator = null;
			}

		} else if(currentHandObj) {
			Destroy(currentHandObj);
			currentHandObj = null;
			currentWeaponHandler = null;
		}
	}

	void Update() {
		if(!dead && mode != 1) {
			LoseCalories(Time.deltaTime * hungerLoss);
			if(hunger <= 10f) {
				TakeEffectDamage(Time.deltaTime * hungerDamage);
				if(!infoText.activeSelf) {
					infoText.SetActive(true);
				}
			} else if(infoText.activeSelf) {
				infoText.SetActive(false);
			}
		}

		if(hunger >= fullLevel) {
			GainHealth(Time.deltaTime * fullHealthRegainAmount);
		}

		if(!lockLook) {
			transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * mouseSensitivityX);
			verticalLookRotation += Input.GetAxisRaw("Mouse Y") * mouseSensitivityY;
			verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90, 90);
			playerCameraHolder.transform.localEulerAngles = Vector3.left * verticalLookRotation;
		}

		Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

		if(Input.GetKey(sprintKey)) {
			currentMoveSpeed = runSpeed;
		} else {
			currentMoveSpeed = walkSpeed;
		}

		if(Input.GetKey(jumpKey)) {
			if(grounded) {
				rb.AddForce(transform.up * jumpForce);
			}
		}

		Vector3 targetMoveAmount = moveDir * currentMoveSpeed;

		moveAmount = Vector3.SmoothDamp(moveAmount, targetMoveAmount, ref smoothMoveVelocity, smoothTime);

		flyDoubleTapCooldown -= Time.deltaTime;

		if(Input.GetKey(KeyCode.C)) {
			if(canvas.activeSelf) {
				canvas.SetActive(false);
			}
		} else {
			if(!canvas.activeSelf) {
				canvas.SetActive(true);
			}
		}

		RaycastHit hit;
		Ray ray = playerCamera.GetComponent<Camera>().ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));

		Physics.Raycast(ray, out hit);

		distanceToTarget = hit.distance;

		bool hideNoticeText = false;
		string noticeText = string.Empty;

		if(!ActiveMenu()) {
			if(currentWeaponHandler && currentWeaponHandler.weapon.type == Weapon.WeaponType.Bow) {
				if(!firing) {
					firing = true;
					currentWeaponAnimator.SetBool("PullingBack", true);
				}
				if(Input.GetMouseButton(2)) {
					if(drawTime < currentWeaponHandler.weapon.chargeTime) {
						drawTime++;
					}
				}
			} else if(firing) {
				firing = false;
				currentWeaponAnimator.SetTrigger("Fire");
				currentWeaponAnimator.SetBool("PullingBack", false);
			}

			if(hit.collider) {
				target = hit.collider.gameObject;
				targetHit = hit;

				if(Input.GetMouseButtonDown(0)) {
					if(hit.collider.CompareTag("Resource")) {
						if(distanceToTarget >= interactRange) {
							Attack();
						}
					} else {
						Attack();
					}
				}

				if(target.CompareTag("Item") && distanceToTarget <= interactRange && !inventory.placingStructure) {
					AutoMiner autoMiner = null;
					ItemHandler itemHandler = target.GetComponentInParent<ItemHandler>();
					if(itemHandler.item.id == 12) { // Is it a crate?
						noticeText = "Hold [E] to pick up, [F] to open";
						if(Input.GetButton("Interact")) {
							itemHandler.GetComponent<LootContainer>().Open();
						}
					} else if(itemHandler.item.id == 23) { // Auto Miner

						autoMiner = itemHandler.GetComponent<AutoMiner>();

						if(inventory.currentSelectedItem && inventory.currentSelectedItem.type == Item.ItemType.Tool) {
							noticeText = "Hold [E] to pick up, [F] to replace tool";

							if(Input.GetButton("Interact")) {
								autoMiner.SetTool(inventory.currentSelectedItem);
								inventory.hotbar.GetChild(inventory.selectedHotbarSlot).GetComponent<InventorySlot>().DecreaseItem(1);
								inventory.InventoryUpdate();
							}

						} else {
							noticeText = "Hold [E] to pick up, [F] to gather items";
							if(Input.GetButton("Interact")) {
								int i = 0;
								foreach(Item item in autoMiner.items) {
									inventory.AddItem(item, autoMiner.itemAmounts[i]);
									i++;
								}

								autoMiner.ClearItems();
							}
						}
					} else if(itemHandler.item.id == 24) { // Door
						Door door = itemHandler.GetComponent<Door>();
						if(door.open) {
							noticeText = "Hold [E] to pick up, [F] to close door";
						} else {
							noticeText = "Hold [E] to pick up, [F] to open door";
						}
						if(Input.GetButtonDown("Interact")) {
							door.ToggleOpen();
						}
					} else if(itemHandler.item.id == 25) { // Conveyor belt
						ConveyorBelt conveyor = itemHandler.GetComponent<ConveyorBelt>();
						noticeText = "Hold [E] to pick up, [F] to change speed. Current speed is " + conveyor.speeds[conveyor.speedNum];
						if(Input.GetButtonDown("Interact")) {
							conveyor.IncreaseSpeed();
						}
					} else if(itemHandler.item.id == 27) { // Auto Sorter
						AutoSorter sorter = itemHandler.GetComponent<AutoSorter>();
						if(inventory.currentSelectedItem) {
							noticeText = "Hold [E] to pick up, [F] set sorting item";
							if(Input.GetButtonDown("Interact")) {
								sorter.SetItem(inventory.currentSelectedItem);
							}
						} else {
							noticeText = "Hold [E] to pick up";
						}
					} else if(itemHandler.item.id == 30) { // Radio
						Radio radio = itemHandler.GetComponent<Radio>();
						if(radio.songNum == -1) {
							noticeText = "Hold [E] to pick up, [F] to turn on";
						} else if(radio.songNum == radio.songs.Length - 1) {
							/*noticeText = "Hold [E] to pick up, [F] to turn off. Currently playing " + radio.songs[radio.songNum].name;*/
							noticeText = "A Helicopter is on it's way. Please wait for a while.";
						} else {
							noticeText = "Hold [E] to pick up, [F] to change song. Currently playing " + radio.songs[radio.songNum].name;
						}
						if(Input.GetButtonDown("Interact")) {
							radio.ChangeSong();
						}
					} else if(itemHandler.item.id == 35) { // Light
						LightItem li = itemHandler.GetComponent<LightItem>();
						if(li.intensities[li.intensityNum] == 0) {
							noticeText = "Hold [E] to pick up, [F] to turn on";
						} else if(li.intensityNum == li.intensities.Length - 1) {
							noticeText = "Hold [E] to pick up, [F] to turn off. Current brightness is " + li.intensities[li.intensityNum];
						} else {
							noticeText = "Hold [E] to pick up, [F] to change brightness. Current brightness is " + li.intensities[li.intensityNum];
						}
						if(Input.GetButtonDown("Interact")) {
							li.IncreaseIntensity();
						}
					} else {
						noticeText = "Hold [E] to pick up";
					}
					if(Input.GetButton("PickUp")) {
						if(!pickingUp || (pickingUp && !progressContainer.activeSelf)) {
							progressContainer.SetActive(true);
							pickingUp = true;
						} else {
							pickingUpTime += Time.deltaTime;
							progressImage.fillAmount = pickingUpTime / itemHandler.item.timeToGather;
							progressText.text = (itemHandler.item.timeToGather - pickingUpTime).ToString("0.0");

							if(pickingUpTime >= itemHandler.item.timeToGather) {
								if(autoMiner) {
									if(autoMiner.currentToolItem) {
										inventory.AddItem(autoMiner.GatherTool(), 1);
									}
									int q = 0;
									foreach(Item item in autoMiner.items) {
										inventory.AddItem(item, autoMiner.itemAmounts[q]);
										q++;
									}
								}
								if(itemHandler.item.id == 6) { // Is it a furnace?
									Furnace furnace = itemHandler.GetComponent<Furnace>();
									if(furnace) {
										inventory.AddItem(fuelItem, (int)Mathf.Floor(furnace.fuel)); // ONLY WORKS IF WOOD IS ONLY FUEL SOURCE
										if(furnace.currentSmeltingItem) {
											inventory.AddItem(furnace.currentSmeltingItem, 1);
										}
									}
								}
								pickingUpTime = 0f;
								progressImage.fillAmount = 0f;
								inventory.Pickup(itemHandler);
							}
						}
					} else if(pickingUp) {
						hideNoticeText = true;
						pickingUpTime = 0f;
						progressImage.fillAmount = 0f;
						progressContainer.SetActive(false);
						pickingUp = false;
					}
				} else if(target.CompareTag("Resource") && distanceToTarget <= interactRange && !inventory.placingStructure) { // Gather resources
					noticeText = "Hold [LMB] to gather";
					if(Input.GetMouseButton(0)) {
						hideNoticeText = true;
						if(!gathering || (gathering && !progressContainer.activeSelf)) {
							gathering = true;
							progressContainer.SetActive(true);
						} else {
							float multiplier = 1f;
							bool hasTool = false;
							if(inventory.currentSelectedItem && inventory.currentSelectedItem.type == Item.ItemType.Tool) {
								multiplier = inventory.currentSelectedItem.speed;
								hasTool = true;
							}
							gatheringTime += Time.deltaTime;
							if(currentResource == null || currentResource.gameObject != target) {
								currentResource = target.GetComponent<ResourceHandler>();
							}
							progressImage.fillAmount = gatheringTime / (currentResource.resource.gatherTime / multiplier);
							progressText.text = (currentResource.resource.gatherTime / multiplier - gatheringTime).ToString("0.0");
							if(gatheringTime >= currentResource.resource.gatherTime / multiplier) {
								int i = 0;
								foreach(Item item in currentResource.resource.resourceItems) {
									if(Random.Range(0f, 1f) <= currentResource.resource.chances[i]) {
										inventory.AddItem(item, hasTool ? inventory.currentSelectedItem.gatherAmount : 1);
									}
									i++;
								}
								gatheringTime = 0f;
								progressImage.fillAmount = 0f;
								currentResource.Gather(hasTool ? inventory.currentSelectedItem.gatherAmount : 1); // CURRENTLY GATHERS GATHER AMOUNT EVEN IF RESOURCE HAS LESS THAN THAT AMOUNT LEFT
							}
						}
					} else if(gathering || pickingUp) {
						CancelGatherAndPickup();
						hideNoticeText = true;
					}
				} else if(gathering || pickingUp) {
					CancelGatherAndPickup();
					hideNoticeText = true;
				} else {
					hideNoticeText = true;
				}
			} else {
				if(gathering || pickingUp) {
					CancelGatherAndPickup();
					hideNoticeText = true;
				}
				if(Input.GetMouseButtonDown(0)) {
					Attack();
				}
				target = null;
				hideNoticeText = true;
				if(progressContainer.activeSelf) {
					progressContainer.SetActive(false);
				}
			}
		}

		handAnimator.SetBool("Gathering", gathering);

		if(hideNoticeText) {
			if(!inventory.placingStructure) {
				HideNoticeText();
			}
		} else {
			ShowNoticeText(noticeText);
		} 

		if(dead) {
			purgatoryRespawnTimeText.text = (-(Time.time - nextTimeToRespawn)).ToString("0");
			if(Time.time >= nextTimeToRespawn) {
				Respawn();
			}
		}
	}

	public void LoadCreativeMode() {
		mode = 1;
		hungerAmountImage.gameObject.SetActive(false);
		healthAmountImage.gameObject.SetActive(false);
	}

	public void Consume(Item item) {
		useParticles.Play();
		GainCalories(item.calories);
	}

	public bool ActiveMenu() {
		return inventory.inventoryContainer.activeSelf;
	}

	void Attack() {
		if(handAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.name != "PlayerAttack") { // IF PLAYERATTACK IS RENAMED, THIS WILL NOT WORK
			handAnimator.SetTrigger("Attack");
			if(target) {
				Health targetHealth = target.GetComponent<Health>();
				if(targetHealth) {
					if(inventory.currentSelectedItem) {
						WeaponHandler handler = inventory.currentSelectedItem.prefab.GetComponent<WeaponHandler>();
						if(handler && handler.weapon.type == Weapon.WeaponType.Melee) {
							targetHealth.TakeDamage(handler.weapon.damage);
						}
					} else {
						targetHealth.TakeDamage(handDamage);
					}
				}
			}
		}
	}
	void OnTriggerEnter(Collider other) {

		if (other.CompareTag("Projectile"))
		{
			Debug.Log("Hit");
			TakeDamage(other.GetComponent<WeaponHandler>().weapon.damage);
			Destroy(other);
		}

		if (other.CompareTag("Item")) {
			ItemHandler itemHandler = other.GetComponentInParent<ItemHandler>();
			if(itemHandler) {
				if(itemHandler.item.id == 36) { // Ladder
					Debug.Log("Climbing");
					StartClimbing();
                }
                else
                {
					StopClimbing();
                }
			}
		}	

		if (other.CompareTag("DeathWater"))
        {
			Die();
        }
	}

	void OnTriggerStay(Collider other) {
		if(!climbing) {
			if(other.CompareTag("Item")) {
				ItemHandler itemHandler = other.GetComponentInParent<ItemHandler>();
				if(itemHandler) {
					if(itemHandler.item.id == 36) { // Ladder
						StartClimbing();
					}
				}
			}
		}
	}

	void OnTriggerExit(Collider other) {
		StopClimbing();
	}

	void StartClimbing() {
		climbing = true;
	}

	void StopClimbing() {
		climbing = false;
		if(!flying) {
			rb.useGravity = true;
		}
	}
	void GainCalories(float amount) {
		hunger += amount;
		if(hunger > maxHunger) {
			hunger = maxHunger;
		}
		HungerChange();
	}

	void LoseCalories(float amount) {
		hunger -= amount;
		HungerChange();
		if(hunger < 0f) {
			hunger = 0f;
		}
	}

	void GainHealth(float amount) {
		health += amount;
		if(health > maxHealth) {
			health = maxHealth;
		}
		HealthChange();
	}

	public void TakeDamage(float amount) {
		health -= amount;
		CameraShaker.Instance.ShakeOnce(4f, 5f, 0.1f, 0.5f);
		damagePanelAnim.Play();
		if(health <= 0 && !dead) {
			Die();
		} else if(health < 0f) {
			health = 0f;
		}
		HealthChange();
	}

	public void TakeEffectDamage(float amount) {
		health -= amount;
		if(health <= 0 && !dead) {
			Die();
		} else if(health < 0f) {
			health = 0f;
		}
		HealthChange();
	}

	void HungerChange() {
		hungerAmountImage.fillAmount = hunger / maxHunger;
	}

	void HealthChange() {
		healthAmountImage.fillAmount = health / maxHealth;
	}
	void CancelGatherAndPickup() {
		gatheringTime = 0f;
		pickingUpTime = 0f;
		progressImage.fillAmount = 0f;
		progressContainer.SetActive(false);
		gathering = false;
		pickingUp = false;
	}
	public void LoadLightWorld() {
		RenderSettings.fogColor = defaultFogColor;
		RenderSettings.fogDensity = defaultFogDensity;
		playerCamera.GetComponent<Camera>().backgroundColor = defaultPlayerCameraColor;
		lightDeactivateObjects.SetActive(true);
		darkDeactivateObjects.SetActive(false);
		rain.SetActive(false);
	}

	public void LoadDarkWorld() {
		RenderSettings.fogColor = Color.black;
		RenderSettings.fogDensity = 0.04f;
		playerCamera.GetComponent<Camera>().backgroundColor = Color.black;
		darkDeactivateObjects.SetActive(true);
		lightDeactivateObjects.SetActive(false);
		rain.SetActive(true);
	}
	public void Die() {
		if(difficulty > 1) {
			inventory.ClearInventory();
		}
		if(inventory.placingStructure) {
			inventory.CancelStructurePlacement();
		}
		dead = true;

		SceneManager.LoadScene("GameOverScene");
		/*		Respawn();*/
	}

	void Respawn() {
		health = maxHealth;
		hunger = maxHunger;
		dead = false;
		Vector3 spawnPos = new Vector3(0, 1000, 0);
		transform.position = spawnPos;
	}

	public void ShowNoticeText(string text) {
		if(!noticeText.gameObject.activeSelf) {
			noticeText.gameObject.SetActive(true);
		}
		noticeText.text = text;
	}

	public void HideNoticeText() {
		if(noticeText.gameObject.activeSelf) {
			noticeText.text = "";
			noticeText.gameObject.SetActive(false);
		}
	}

	public void LockLook(bool _lockLook) {
		lockLook = _lockLook;
	}

	void FixedUpdate() {

		if(grounded) {
			Collider[] cols = Physics.OverlapBox(transform.position - Vector3.up * 0.5f, 0.5f * Vector3.one);
			if(cols.Length - 2 < 1) { // Doesn't seem to be ground near (Prevents flying). We do the -2 because the player and groundCheck collide with it.
				grounded = false;
			}
		}
			rb.MovePosition(rb.position + (transform.TransformDirection(moveAmount) * Time.fixedDeltaTime));
	}
}