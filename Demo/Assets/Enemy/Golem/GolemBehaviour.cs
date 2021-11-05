using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds general functionality that all golems should inherrent from.
/// </summary>
public class GolemBehaviour : MonoBehaviour, EnemyAI.IAIBehaviour
{
    public enum GolemType { Small, Medium, Large };
    public GolemType golem_type;

    [SerializeField] private Transform meele_transform;

    private Node top_node;
    private EnemyAI enemy_ai;

    /// <summary>
    /// Range where enemy will try to hit the player.
    /// Will try to minimize distance between meele transform and player transform.
    /// </summary>
    [SerializeField] private float meele_range;

    /// <summary>
    /// Range where enemy will unburrow self.
    /// Player position updates to where they stood when provoked.
    /// </summary>
    [SerializeField] private float buried_find_range;

    /// <summary>
    /// If player position is not updated for this long burry self.
    /// </summary>
    [SerializeField] private float buried_self_time;

    /// <summary>
    /// How much enemy should walk around where they think player is.
    /// </summary>
    [SerializeField] private float wander_strength;

    /// <summary>
    /// If unburried and player this close to golem, update player position
    /// </summary>
    [SerializeField] private float find_range;

    /// <summary>
    /// If sees player and player this close to golem, update player position
    /// </summary>
    [SerializeField] private float chasing_range;
    // Node that makes agent go to point and walk around near point for x amount of time.

    public bool has_golem_in_hands;
    [SerializeField] private Transform closest_golem;
    [HideInInspector] private Transform throwable_parrent_transform;

    [SerializeField] private float golem_pickup_range;

    [SerializeField] private float throw_range;
    [SerializeField] private float golem_find_range;
    [SerializeField] private float golem_find_range_overide;

    public Node ConstructBehaviourTree(GolemType golem_type)
    {
        switch (golem_type)
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
        IsBuriedNode is_buried = new IsBuriedNode(this);
        RangeNode buried_range = new RangeNode(buried_find_range, enemy_ai.player_transform, transform);
        RunFunctionNode un_bury_self = new RunFunctionNode(UnBurySelf);
        Sequence un_bury = new Sequence(new List<Node> { is_buried, buried_range, un_bury_self });

        // Buries self
        RunFunctionNode bury_self = new RunFunctionNode(BurySelf);

        // Patroll

        ///
        /// If chase_transform != player_transform act curious
        /// 
        ///
        /// Else try to kill the player
        /// 

        //ProceduralWalkNode procedural_walk = new ProceduralWalkNode(enemy_ai.chase_transform, wander_strength);
        ChaseNode chase = new ChaseNode(enemy_ai.chase_transform, enemy_ai.agent, enemy_ai, meele_transform);
        Sequence patroll = new Sequence(new List<Node> { /*procedural_walk, */chase });


        // Controlls spawned enemy in ground
        //Sequence hide_sequence = new Sequence(new List<Node> { NOT_YET_IMPLEMENTED });

        // Hits player
        //RangeNode meele_range_node = new RangeNode(meele_range, enemy_ai.player_transform, meele_transform);
        //MeeleNode meele_node = new MeeleNode(enemy_ai.agent, enemy_ai);
        //Sequence meele_sequence = new Sequence(new List<Node> { meele_range_node, meele_node });

        // Chases player
        //RangeNode chasing_range_node = new RangeNode(chasing_range, enemy_ai.player_transform, meele_transform);
        //ChaseNode chase_node = new ChaseNode(enemy_ai.player_transform, enemy_ai.agent, enemy_ai);
        //Sequence chase_sequence = new Sequence(new List<Node> { chasing_range_node, chase_node });

        // Searches for player
        // NOT YET IMPLEMENTED
        //Sequence search_sequence = new Sequence(new List<Node> { NOT_YET_IMPLEMENTED });

        return new Selector(new List<Node> { un_bury, is_buried, bury_self, patroll });
    }

    private Node MediumGolemBehaviourTree()
    {
        RangeNode NOT_YET_IMPLEMENTED = new RangeNode(0f, enemy_ai.player_transform, transform);

        // Controlls when ai dies
        HealthNode health_node = new HealthNode(enemy_ai);
        DeathNode death_node = new DeathNode(enemy_ai);
        Sequence health_sequence = new Sequence(new List<Node> { health_node, death_node });

        // Controlls spawned enemy in ground
        // NOT YET IMPLEMENTED
        Sequence hide_sequence = new Sequence(new List<Node> { NOT_YET_IMPLEMENTED });

        // Hits player
        RangeNode meele_range_node = new RangeNode(meele_range, enemy_ai.player_transform, meele_transform);
        MeeleNode meele_node = new MeeleNode(enemy_ai.agent, enemy_ai);
        Sequence meele_sequence = new Sequence(new List<Node> { meele_range_node, meele_node });

        // Picks up golem and throws them at player
        HasGolemInHandsNode has_golem_in_hands_node = new HasGolemInHandsNode(this);
        RangeNode throw_range_node = new RangeNode(throw_range, enemy_ai.player_transform, transform);
        ThrowGolemNode throw_golem_node = new ThrowGolemNode(enemy_ai.agent, enemy_ai, this);
        Sequence throw_golem_in_hands_sequence = new Sequence(new List<Node> { has_golem_in_hands_node, throw_range_node, throw_golem_node });

        GolemRangeNode golem_go_to_range_node = new GolemRangeNode(golem_find_range, this);
        GoToGolemNode go_to_golem_node = new GoToGolemNode(enemy_ai.agent, enemy_ai, this);
        Sequence go_to_golem_sequence = new Sequence(new List<Node> { golem_go_to_range_node, go_to_golem_node });

        GolemRangeNode golem_pickup_range_node = new GolemRangeNode(golem_pickup_range, this);
        PickupGolemNode pickup_golem_node = new PickupGolemNode(this);
        Sequence pick_up_golem_sequence = new Sequence(new List<Node> { golem_pickup_range_node, pickup_golem_node });

        RangeNode player_to_close_ignore_node = new RangeNode(golem_find_range_overide, enemy_ai.player_transform, transform);
        Inverter player_to_close_ignore_node_inverter = new Inverter(player_to_close_ignore_node);
        IsGolemAvailableNode is_golem_available_node = new IsGolemAvailableNode(throwable_parrent_transform, this);
        Selector go_to_golem_selector = new Selector(new List<Node> { pick_up_golem_sequence, go_to_golem_sequence });
        Sequence find_golem_sequence = new Sequence(new List<Node> { player_to_close_ignore_node_inverter, is_golem_available_node, go_to_golem_selector });

        Selector throw_selector = new Selector(new List<Node> { throw_golem_in_hands_sequence, find_golem_sequence });

        // Chases player
        RangeNode chasing_range_node = new RangeNode(chasing_range, enemy_ai.player_transform, transform);
        ChaseNode chase_node = new ChaseNode(enemy_ai.player_transform, enemy_ai.agent, enemy_ai, meele_transform);
        Sequence chase_sequence = new Sequence(new List<Node> { chasing_range_node, chase_node });

        // Searches for player
        // NOT YET IMPLEMENTED
        Sequence search_sequence = new Sequence(new List<Node> { NOT_YET_IMPLEMENTED });

        return new Selector(new List<Node> { health_sequence, hide_sequence, meele_sequence, throw_selector, chase_sequence, search_sequence });
    }

    private Node LargeGolemBehaviourTree()
    {
        RangeNode NOT_YET_IMPLEMENTED = new RangeNode(0f, enemy_ai.player_transform, transform);

        // Controlls when ai dies
        HealthNode health_node = new HealthNode(enemy_ai);
        DeathNode death_node = new DeathNode(enemy_ai);
        Sequence health_sequence = new Sequence(new List<Node> { health_node, death_node });

        // Controlls spawned enemy in ground
        // NOT YET IMPLEMENTED
        Sequence hide_sequence = new Sequence(new List<Node> { NOT_YET_IMPLEMENTED });

        // Hits player
        RangeNode meele_range_node = new RangeNode(meele_range, enemy_ai.player_transform, meele_transform);
        MeeleNode meele_node = new MeeleNode(enemy_ai.agent, enemy_ai);
        Sequence meele_sequence = new Sequence(new List<Node> { meele_range_node, meele_node });

        // Picks up golem and throws them at player
        HasGolemInHandsNode has_golem_in_hands_node = new HasGolemInHandsNode(this);
        RangeNode throw_range_node = new RangeNode(throw_range, enemy_ai.player_transform, transform);
        ThrowGolemNode throw_golem_node = new ThrowGolemNode(enemy_ai.agent, enemy_ai, this);
        Sequence throw_golem_in_hands_sequence = new Sequence(new List<Node> { has_golem_in_hands_node, throw_range_node, throw_golem_node });

        GolemRangeNode golem_go_to_range_node = new GolemRangeNode(golem_find_range, this);
        GoToGolemNode go_to_golem_node = new GoToGolemNode(enemy_ai.agent, enemy_ai, this);
        Sequence go_to_golem_sequence = new Sequence(new List<Node> { golem_go_to_range_node, go_to_golem_node });

        GolemRangeNode golem_pickup_range_node = new GolemRangeNode(golem_pickup_range, this);
        PickupGolemNode pickup_golem_node = new PickupGolemNode(this);
        Sequence pick_up_golem_sequence = new Sequence(new List<Node> { golem_pickup_range_node, pickup_golem_node });

        RangeNode player_to_close_ignore_node = new RangeNode(golem_find_range_overide, enemy_ai.player_transform, transform);
        Inverter player_to_close_ignore_node_inverter = new Inverter(player_to_close_ignore_node);
        IsGolemAvailableNode is_golem_available_node = new IsGolemAvailableNode(throwable_parrent_transform, this);
        Selector go_to_golem_selector = new Selector(new List<Node> { pick_up_golem_sequence, go_to_golem_sequence });
        Sequence find_golem_sequence = new Sequence(new List<Node> { player_to_close_ignore_node_inverter, is_golem_available_node, go_to_golem_selector });

        Selector throw_selector = new Selector(new List<Node> { throw_golem_in_hands_sequence, find_golem_sequence });

        // Chases player
        RangeNode chasing_range_node = new RangeNode(chasing_range, enemy_ai.player_transform, transform);
        ChaseNode chase_node = new ChaseNode(enemy_ai.player_transform, enemy_ai.agent, enemy_ai, meele_transform);
        Sequence chase_sequence = new Sequence(new List<Node> { chasing_range_node, chase_node });

        // Searches for player
        // NOT YET IMPLEMENTED
        Sequence search_sequence = new Sequence(new List<Node> { NOT_YET_IMPLEMENTED });

        return new Selector(new List<Node> { health_sequence, hide_sequence, meele_sequence, throw_selector, chase_sequence, search_sequence });
    }

    private void LateUpdate()
    {
#if UNITY_EDITOR
        top_node = ConstructBehaviourTree(golem_type);
#endif
        top_node.Evaluate();
        if (top_node.node_state == NodeState.failure)
        {
            enemy_ai.SetColor(Color.magenta);
        }
    }

    [SerializeField] private float repeating_damage_radius;
    private DamageByFire damage_by_fire;

    private void Awake()
    {
        throwable_parrent_transform = GetThrowableTransform(golem_type);
        damage_by_fire = GameObject.Find("Flammable").GetComponent<DamageByFire>();
        enemy_ai = GetComponent<EnemyAI>();
    }

    private Transform GetThrowableTransform(GolemType golem_type)
    {
        switch (golem_type)
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
        top_node = ConstructBehaviourTree(golem_type);
        InvokeRepeating("DamageCheckInterval", 0.25f, 0.25f);
    }

    /// <summary>
    /// All reacurrent damages that golems should be effected by. Like: Fire etc.
    /// </summary>
    private void DamageCheckInterval()
    {
        enemy_ai.Damage(damage_by_fire.DamageStacked(transform.position, repeating_damage_radius));
    }

    /// <summary>
    /// Returns the closest golem that can be picked up
    /// </summary>
    public Transform GetClosestGolem()
    {
        return closest_golem;
    }

    public void PlaceGolemInHands()
    {
        Transform hand_transform = transform.Find("Golem").Find("Hand.R");
        closest_golem.transform.parent = hand_transform;
        closest_golem.transform.localPosition = Vector3.zero;
        //closest_golem.GetComponent<Agent>().Tumble();
    }

    public void SetClosestGolem(Transform closest_golem)
    {
        this.closest_golem = closest_golem;
    }

    public void Die()
    {
        this.enabled = false;
    }

    public bool is_buried = true;
    public float time_since_seen_player = 0f;
    public NodeState BurySelf()
    {
        time_since_seen_player += Time.deltaTime;
        if (time_since_seen_player >= buried_self_time)
        {
            time_since_seen_player = 0f;
            is_buried = true;
            return NodeState.running;
        }
        return NodeState.failure;
    }

    public NodeState UnBurySelf()
    {
        is_buried = false;
        time_since_seen_player = 0f;
        return NodeState.running;
    }
}
