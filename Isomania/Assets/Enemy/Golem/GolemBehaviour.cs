using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds general functionality that all golems should inherrent from.
/// </summary>
public class GolemBehaviour : MonoBehaviour, EnemyAI.IAIBehaviour
{
    public enum GolemType { Small, Medium, Large };
    public GolemType golemType;

    [SerializeField] private Transform meeleTransform;

    private Node topNode;
    private EnemyAI enemyAi;

    /// <summary>
    /// Range where enemy will try to hit the player.
    /// Will try to minimize distance between meele transform and player transform.
    /// </summary>
    [SerializeField] private float meeleRange;

    /// <summary>
    /// How much enemy should walk around where they think player is.
    /// </summary>
    [SerializeField] private float wanderStrength;
    // Node that makes agent go to point and walk around near point for x amount of time.

    public bool hasGolemInHands;
    [SerializeField] private Transform closestGolem;
    [HideInInspector] private Transform throwableParrentTransform;

    [SerializeField] private float golemPickupRange;

    [SerializeField] private float throwRange;
    [SerializeField] private float golemFindRange;
    [SerializeField] private float golemFindRangeOveride;

    public Node ConstructBehaviourTree(GolemType golemType)
    {
        switch (golemType)
        {
            case GolemType.Small:
                return SmallGolemBehaviourTree();
            case GolemType.Medium:
                return MediumGolemBehaviourTree();
            case GolemType.Large:
                return LargeGolemBehaviourTree();
        }
        return null;
    }

    private Node SmallGolemBehaviourTree()
    {
        // Unburies self
        IsBuriedNode isBuried = new IsBuriedNode(this);
        TransformNotNullNode notNull = new TransformNotNullNode(enemyAi);
        RunFunctionNode unBurySelf = new RunFunctionNode(UnBurySelf);
        Sequence unBury = new Sequence(new List<Node> { isBuried, notNull, unBurySelf });

        // Buries self
        RunFunctionNode burySelf = new RunFunctionNode(BurySelf);

        ChaseNode chase = new ChaseNode(enemyAi.agent, enemyAi, meeleTransform, enemyAi.agent.sprintSpeed);
        Sequence chaseSequence = new Sequence(new List<Node> { notNull, chase });

        // Patroll
        RandomWalkNode randomWalk = new RandomWalkNode(enemyAi, wanderStrength);
        ChaseVectorNode chaseVector = new ChaseVectorNode(enemyAi, enemyAi.agent, meeleTransform, enemyAi.agent.walkSpeed);
        Sequence patrollSequence = new Sequence(new List<Node> { notNull, chase });

        ///
        /// If chaseTransform != playerTransform act curious
        /// 
        ///
        /// Else try to kill the player
        /// 

        //ProceduralWalkNode proceduralWalk = new ProceduralWalkNode(enemyAi.chaseTransform, wanderStrength);


        // Controlls spawned enemy in ground
        //Sequence hideSequence = new Sequence(new List<Node> { NOT_YET_IMPLEMENTED });

        // Hits player
        //RangeNode meeleRangeNode = new RangeNode(meeleRange, enemyAi.playerTransform, meeleTransform);
        //MeeleNode meeleNode = new MeeleNode(enemyAi.agent, enemyAi);
        //Sequence meeleSequence = new Sequence(new List<Node> { meeleRangeNode, meeleNode });

        // Chases player
        //RangeNode chasingRangeNode = new RangeNode(chasingRange, enemyAi.playerTransform, meeleTransform);
        //ChaseNode chaseNode = new ChaseNode(enemyAi.playerTransform, enemyAi.agent, enemyAi);
        //Sequence chaseSequence = new Sequence(new List<Node> { chasingRangeNode, chaseNode });

        // Searches for player
        // NOT YET IMPLEMENTED
        //Sequence searchSequence = new Sequence(new List<Node> { NOT_YET_IMPLEMENTED });

        return new Selector(new List<Node> { unBury, isBuried, burySelf, chaseSequence, patrollSequence });
    }

    private Node MediumGolemBehaviourTree()
    {
        RangeNode NOT_YET_IMPLEMENTED = new RangeNode(0f, Global.playerTransform, transform);

        // Controlls when ai dies
        HealthNode healthNode = new HealthNode(enemyAi);
        DeathNode deathNode = new DeathNode(enemyAi);
        Sequence healthSequence = new Sequence(new List<Node> { healthNode, deathNode });

        // Controlls spawned enemy in ground
        // NOT YET IMPLEMENTED
        Sequence hideSequence = new Sequence(new List<Node> { NOT_YET_IMPLEMENTED });

        // Hits player
        RangeNode meeleRangeNode = new RangeNode(meeleRange, Global.playerTransform, meeleTransform);
        MeeleNode meeleNode = new MeeleNode(enemyAi.agent, enemyAi);
        Sequence meeleSequence = new Sequence(new List<Node> { meeleRangeNode, meeleNode });

        // Picks up golem and throws them at player
        HasGolemInHandsNode hasGolemInHandsNode = new HasGolemInHandsNode(this);
        RangeNode throwRangeNode = new RangeNode(throwRange, Global.playerTransform, transform);
        ThrowGolemNode throwGolemNode = new ThrowGolemNode(enemyAi.agent, enemyAi, this);
        Sequence throwGolemInHandsSequence = new Sequence(new List<Node> { hasGolemInHandsNode, throwRangeNode, throwGolemNode });

        GolemRangeNode golemGoToRangeNode = new GolemRangeNode(golemFindRange, this);
        GoToGolemNode goToGolemNode = new GoToGolemNode(enemyAi.agent, enemyAi, this, enemyAi.agent.walkSpeed);
        Sequence goToGolemSequence = new Sequence(new List<Node> { golemGoToRangeNode, goToGolemNode });

        GolemRangeNode golemPickupRangeNode = new GolemRangeNode(golemPickupRange, this);
        PickupGolemNode pickupGolemNode = new PickupGolemNode(this);
        Sequence pickUpGolemSequence = new Sequence(new List<Node> { golemPickupRangeNode, pickupGolemNode });

        RangeNode playerToCloseIgnoreNode = new RangeNode(golemFindRangeOveride, Global.playerTransform, transform);
        Inverter playerToCloseIgnoreNodeInverter = new Inverter(playerToCloseIgnoreNode);
        IsGolemAvailableNode isGolemAvailableNode = new IsGolemAvailableNode(throwableParrentTransform, this);
        Selector goToGolemSelector = new Selector(new List<Node> { pickUpGolemSequence, goToGolemSequence });
        Sequence findGolemSequence = new Sequence(new List<Node> { playerToCloseIgnoreNodeInverter, isGolemAvailableNode, goToGolemSelector });

        Selector throwSelector = new Selector(new List<Node> { throwGolemInHandsSequence, findGolemSequence });

        // Chases player
        float chasingRange = 0f;
        RangeNode chasingRangeNode = new RangeNode(chasingRange, Global.playerTransform, transform);
        ChaseNode chaseNode = new ChaseNode(enemyAi.agent, enemyAi, meeleTransform, enemyAi.agent.sprintSpeed);
        Sequence chaseSequence = new Sequence(new List<Node> { chasingRangeNode, chaseNode });

        // Searches for player
        // NOT YET IMPLEMENTED
        Sequence searchSequence = new Sequence(new List<Node> { NOT_YET_IMPLEMENTED });

        return new Selector(new List<Node> { healthSequence, hideSequence, meeleSequence, throwSelector, chaseSequence, searchSequence });
    }

    private Node LargeGolemBehaviourTree()
    {
        RangeNode NOT_YET_IMPLEMENTED = new RangeNode(0f, Global.playerTransform, transform);

        // Controlls when ai dies
        HealthNode healthNode = new HealthNode(enemyAi);
        DeathNode deathNode = new DeathNode(enemyAi);
        Sequence healthSequence = new Sequence(new List<Node> { healthNode, deathNode });

        // Controlls spawned enemy in ground
        // NOT YET IMPLEMENTED
        Sequence hideSequence = new Sequence(new List<Node> { NOT_YET_IMPLEMENTED });

        // Hits player
        RangeNode meeleRangeNode = new RangeNode(meeleRange, Global.playerTransform, meeleTransform);
        MeeleNode meeleNode = new MeeleNode(enemyAi.agent, enemyAi);
        Sequence meeleSequence = new Sequence(new List<Node> { meeleRangeNode, meeleNode });

        // Picks up golem and throws them at player
        HasGolemInHandsNode hasGolemInHandsNode = new HasGolemInHandsNode(this);
        RangeNode throwRangeNode = new RangeNode(throwRange, Global.playerTransform, transform);
        ThrowGolemNode throwGolemNode = new ThrowGolemNode(enemyAi.agent, enemyAi, this);
        Sequence throwGolemInHandsSequence = new Sequence(new List<Node> { hasGolemInHandsNode, throwRangeNode, throwGolemNode });

        GolemRangeNode golemGoToRangeNode = new GolemRangeNode(golemFindRange, this);
        GoToGolemNode goToGolemNode = new GoToGolemNode(enemyAi.agent, enemyAi, this, enemyAi.agent.walkSpeed);
        Sequence goToGolemSequence = new Sequence(new List<Node> { golemGoToRangeNode, goToGolemNode });

        GolemRangeNode golemPickupRangeNode = new GolemRangeNode(golemPickupRange, this);
        PickupGolemNode pickupGolemNode = new PickupGolemNode(this);
        Sequence pickUpGolemSequence = new Sequence(new List<Node> { golemPickupRangeNode, pickupGolemNode });

        RangeNode playerToCloseIgnoreNode = new RangeNode(golemFindRangeOveride, Global.playerTransform, transform);
        Inverter playerToCloseIgnoreNodeInverter = new Inverter(playerToCloseIgnoreNode);
        IsGolemAvailableNode isGolemAvailableNode = new IsGolemAvailableNode(throwableParrentTransform, this);
        Selector goToGolemSelector = new Selector(new List<Node> { pickUpGolemSequence, goToGolemSequence });
        Sequence findGolemSequence = new Sequence(new List<Node> { playerToCloseIgnoreNodeInverter, isGolemAvailableNode, goToGolemSelector });

        Selector throwSelector = new Selector(new List<Node> { throwGolemInHandsSequence, findGolemSequence });

        // Chases player
        float chasingRange = 0f;
        RangeNode chasingRangeNode = new RangeNode(chasingRange, Global.playerTransform, transform);
        ChaseNode chaseNode = new ChaseNode(enemyAi.agent, enemyAi, meeleTransform, enemyAi.agent.sprintSpeed);
        Sequence chaseSequence = new Sequence(new List<Node> { chasingRangeNode, chaseNode });

        // Searches for player
        // NOT YET IMPLEMENTED
        Sequence searchSequence = new Sequence(new List<Node> { NOT_YET_IMPLEMENTED });

        return new Selector(new List<Node> { healthSequence, hideSequence, meeleSequence, throwSelector, chaseSequence, searchSequence });
    }

    private void LateUpdate()
    {
//#if UNITY_EDITOR
//        topNode = ConstructBehaviourTree(golemType);
//#endif
        topNode.Evaluate();
        if (topNode.nodeState == NodeState.failure)
        {
            enemyAi.SetColor(Color.magenta);
        }
    }

    [SerializeField] private float repeatingDamageRadius;
    private DamageByFire damageByFire;

    private void Awake()
    {
        throwableParrentTransform = GetThrowableTransform(golemType);
        damageByFire = GameObject.Find("Flammable").GetComponent<DamageByFire>();
        enemyAi = GetComponent<EnemyAI>();
    }

    private Transform GetThrowableTransform(GolemType golemType)
    {
        switch (golemType)
        {
            case GolemType.Small:
                return null;
            case GolemType.Medium:
                return GameObject.Find("sGolems").transform;
            case GolemType.Large:
                return GameObject.Find("mGolems").transform;
        }
        return null;
    }

    private void Start()
    {
        topNode = ConstructBehaviourTree(golemType);
        InvokeRepeating("DamageCheckInterval", 0.25f, 0.25f);
    }

    /// <summary>
    /// All reacurrent damages that golems should be effected by. Like: Fire etc.
    /// </summary>
    private void DamageCheckInterval()
    {
        enemyAi.Damage(damageByFire.DamageStacked(transform.position, repeatingDamageRadius));
    }

    /// <summary>
    /// Returns the closest golem that can be picked up
    /// </summary>
    public Transform GetClosestGolem()
    {
        return closestGolem;
    }

    public void PlaceGolemInHands()
    {
        Transform handTransform = transform.Find("Golem").Find("Hand.R");
        closestGolem.transform.parent = handTransform;
        closestGolem.transform.localPosition = Vector3.zero;
        //closestGolem.GetComponent<Agent>().Tumble();
    }

    public void SetClosestGolem(Transform closestGolem)
    {
        this.closestGolem = closestGolem;
    }

    public void Die()
    {
        this.enabled = false;
    }

    public bool isBuried = true;
    public NodeState BurySelf()
    {
        if (enemyAi.currentAttention <= 0f)
        {
            enemyAi.agent.enabled = false;
            enemyAi.eyesOpen = false;
            isBuried = true;

            transform.position += Vector3.down * 3.7f;

            return NodeState.running;
        }
        return NodeState.failure;
    }

    public NodeState UnBurySelf()
    {
        enemyAi.agent.enabled = true;
        enemyAi.eyesOpen = true;
        isBuried = false;

        transform.position += Vector3.up * 3.7f;

        return NodeState.running;
    }
}
