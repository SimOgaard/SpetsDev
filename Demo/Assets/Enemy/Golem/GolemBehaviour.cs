using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds general functionality that all golems should inherrent from.
/// </summary>
public class GolemBehaviour : MonoBehaviour
{
    public enum GolemType { Small, Medium, Large };
    public GolemType golem_type;

    public bool has_golem_in_hands;
    [SerializeField] private Transform closest_golem;
    [HideInInspector] private Transform throwable_parrent_transform;

    [SerializeField] private Transform meele_transform;

    private Node top_node;
    private EnemyAI enemy_ai;

    [SerializeField] private float meele_range;
    [SerializeField] private float chasing_range;

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

        // Chases player
        RangeNode chasing_range_node = new RangeNode(chasing_range, enemy_ai.player_transform, transform);
        ChaseNode chase_node = new ChaseNode(enemy_ai.player_transform, enemy_ai.agent, enemy_ai);
        Sequence chase_sequence = new Sequence(new List<Node> { chasing_range_node, chase_node });

        // Searches for player
        // NOT YET IMPLEMENTED
        Sequence search_sequence = new Sequence(new List<Node> { NOT_YET_IMPLEMENTED });

        return new Selector(new List<Node> { health_sequence, hide_sequence, meele_sequence, chase_sequence, search_sequence });
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
        ChaseNode chase_node = new ChaseNode(enemy_ai.player_transform, enemy_ai.agent, enemy_ai);
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
        ChaseNode chase_node = new ChaseNode(enemy_ai.player_transform, enemy_ai.agent, enemy_ai);
        Sequence chase_sequence = new Sequence(new List<Node> { chasing_range_node, chase_node });

        // Searches for player
        // NOT YET IMPLEMENTED
        Sequence search_sequence = new Sequence(new List<Node> { NOT_YET_IMPLEMENTED });

        return new Selector(new List<Node> { health_sequence, hide_sequence, meele_sequence, throw_selector, chase_sequence, search_sequence });
    }

    private void LateUpdate()
    {
        top_node.Evaluate();
        if (top_node.node_state == NodeState.failure)
        {
            enemy_ai.SetColor(Color.magenta);
            enemy_ai.agent.StopMoving();
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
                break;
            case GolemType.Large:
                return GameObject.Find("mGolems").transform;
                break;
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
}
