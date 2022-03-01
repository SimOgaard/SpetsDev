using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is earthbening pillar "uppgrade" 1 (only spawn in a straight line)
/// </summary>
public class EarthBendingPillarBase : Ability, Equipment.IEquipment
{
    private float _currentCooldown;
    public new float currentCooldown
    {
        get { return _currentCooldown; }
        set { _currentCooldown = Mathf.Max(0f, value); }
    }

    protected float pillarTraverseTime = 0.2f;
    protected float pillarGrowthSpeed = 1.25f;
    protected float pillarSleepTime = 3.5f;
    protected float pillarGap = 3f;

    protected int maxPillars = 6;
    protected int readyPillars = 6;
    protected Vector3 scale = new Vector3(1.75f, 8f, 1.75f);

    [SerializeField] private float pillarDistanceFromPlayer = 7f;

    public override void UsePrimary()
    {
        if (readyPillars >= maxPillars)
        {
            Vector3 playerPos = Global.playerTransform.position;
            Vector3 mousePos = MousePoint.MousePositionWorldAndEnemy();

            Vector3 direction = (mousePos - playerPos);
            direction.y = 0f;
            direction = direction.normalized;

            Vector3 startPoint = playerPos + direction * pillarDistanceFromPlayer;

            StartCoroutine(SpawnStraight(startPoint, direction));
        }
    }

    public virtual void Update()
    {
        if (readyPillars < maxPillars)
        {
            currentCooldown -= Time.deltaTime;

            if (currentCooldown == 0f)
            {
                readyPillars++;
                currentCooldown = cooldown;
            }
            //Debug.Log(currentCooldown);
            //Debug.Log(readyPillars);
        }
    }

    public IEnumerator SpawnStraight(Vector3 startPoint, Vector3 direction)
    {
        WaitForSeconds wait = new WaitForSeconds(pillarTraverseTime);

        while (readyPillars > 0)
        {
            Vector3 newPoint = startPoint + direction * pillarGap * (maxPillars - readyPillars);
            SpawnRock(newPoint);

            readyPillars--;
            yield return wait;
        }
    }

    public EarthBendingRockScale SpawnRock(Vector3 position)
    {
        GameObject earthBendingRockGameObject = new GameObject("pillar");
        EarthBendingRockScale earthBendingRockScript = earthBendingRockGameObject.AddComponent<EarthBendingRockScale>();
        earthBendingRockScript.growthSpeed = pillarGrowthSpeed;
        earthBendingRockScript.sleepTime = pillarSleepTime;
        earthBendingRockScript.PlacePillar(position, Quaternion.identity, scale);
        return earthBendingRockScript;
    }

    /*
    private void Update()
    {
        currentCooldown -= Time.deltaTime;
        if (currentCooldown == 0f && currentPillarAmount < pillarAmount && !isCasting)
        {
            currentPillarAmount++;
            currentCooldown = ultimateCooldown / pillarAmount;
        }
    }
    */

    /*// alt 2
    private float _cooldown;
    public new float cooldown
    {
        get { return _cooldown / pillarAmount; }
        set { _cooldown = value; }
    }
    private float _currentCooldown;
    public new float currentCooldown
    {
        get { return ultimateCooldown - (currentPillarAmount * (ultimateCooldown / pillarAmount)) + currentCooldown; }
        set { _currentCooldown = value; }
    }
    */

    public override void UpdateUI()
    {
        base.UpdateUI();
        UIInventory.currentAbility_UIImage.sprite = iconSprite;
    }

    public override void Awake()
    {
        base.Awake();
        cooldown = 0.4f;
        iconSprite = Resources.Load<Sprite>("Sprites/UI/earthbending");
    }
}
