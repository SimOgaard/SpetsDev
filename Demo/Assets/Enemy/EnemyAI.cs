using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private float health_remove_speed = 0.5f;
    [SerializeField] private float starting_health;
    private float _current_health;
    public float current_health
    {
        get { return _current_health; }
        private set { _current_health = Mathf.Clamp(value, 0f, starting_health); UpdateHealthBar(); }
    }

    public bool has_golem_in_hands;

    [SerializeField] private float chasing_range;
    [SerializeField] private float throw_range;
    [SerializeField] private float meele_range;
    [SerializeField] private float golem_find_range;
    [SerializeField] private float golem_find_range_overide;
    [SerializeField] private float golem_pickup_range;
    private float chasing_range_pow;
    private float throw_range_pow;
    private float meele_range_pow;
    private float golem_find_range_pow;
    private float golem_find_range_overide_pow;
    private float golem_pickup_range_pow;

    private Transform player_transform;
    private Transform closest_golem;
    [SerializeField] private Transform throwable_parrent_transform; 
    [SerializeField] private Transform meele_transform;

    private Node top_node;
    private Material material;
    private DamageByFire damage_by_fire;
    private Agent agent;

    private float collider_radius;

    private void Awake()
    {
        agent = gameObject.GetComponent<Agent>();
        material = transform.Find("Mesh").Find("UpperBody").GetComponent<MeshRenderer>().material;
        player_transform = GameObject.Find("Player").transform;
        damage_by_fire = GameObject.Find("Flammable").GetComponent<DamageByFire>();
        collider_radius = gameObject.GetComponent<CapsuleCollider>().radius * transform.localScale.x;
    }

    private void Start()
    {
        _current_health = starting_health;
        chasing_range_pow = chasing_range * chasing_range;
        throw_range_pow = throw_range * throw_range;
        meele_range_pow = meele_range * meele_range;
        golem_pickup_range_pow = golem_pickup_range * golem_pickup_range;
        golem_find_range_pow = golem_find_range * golem_find_range;
        golem_find_range_overide_pow = golem_find_range_overide * golem_find_range_overide;

        InitHealthBar();

        ConstructBehaviourTree();

        InvokeRepeating("DamageCheckInterval", 0.25f, 0.25f);
    }

    private void DamageCheckInterval()
    {
        Damage(damage_by_fire.DamageStacked(transform.position, collider_radius));
    }

    private void Update()
    {
        health_bar_transform.rotation = Quaternion.Euler(30f, 0f, 0f);
        current_animated_float_value -= Time.deltaTime * health_remove_speed;

        top_node.Evaluate();
        if(top_node.node_state == NodeState.failure)
        {
            SetColor(Color.red);
            agent.is_stopped = true;
        }
    }

    private void ConstructBehaviourTree()
    {
        RangeNode NOT_YET_IMPLEMENTED = new RangeNode(0f, player_transform, transform);


        // Controlls when ai dies
        HealthNode health_node = new HealthNode(this);
        DeathNode death_node = new DeathNode(agent, this);
        Sequence health_sequence = new Sequence(new List<Node> { health_node, death_node });

        // Controlls spawned enemy in ground
        // NOT YET IMPLEMENTED
        Sequence hide_sequence = new Sequence(new List<Node> { NOT_YET_IMPLEMENTED });
        
        // Hits player
        RangeNode meele_range_node = new RangeNode(meele_range_pow, player_transform, meele_transform);
        MeeleNode meele_node = new MeeleNode(agent, this);
        Sequence meele_sequence = new Sequence(new List<Node> { meele_range_node, meele_node });

        // Picks up golem and throws them at player
        HasGolemInHandsNode has_golem_in_hands_node = new HasGolemInHandsNode(this);
        RangeNode throw_range_node = new RangeNode(throw_range_pow, player_transform, transform);
        ThrowGolemNode throw_golem_node = new ThrowGolemNode(agent, this);
        Sequence throw_golem_in_hands_sequence = new Sequence(new List<Node> { has_golem_in_hands_node, throw_range_node, throw_golem_node });

        GolemRangeNode golem_go_to_range_node = new GolemRangeNode(golem_find_range_pow, this);
        GoToGolemNode go_to_golem_node = new GoToGolemNode(agent, this);
        Sequence go_to_golem_sequence = new Sequence(new List<Node> { golem_go_to_range_node, go_to_golem_node });

        GolemRangeNode golem_pickup_range_node = new GolemRangeNode(golem_pickup_range_pow, this);
        PickupGolemNode pickup_golem_node = new PickupGolemNode(this);
        Sequence pick_up_golem_sequence = new Sequence(new List<Node> { golem_pickup_range_node, pickup_golem_node });

        RangeNode player_to_close_ignore_node = new RangeNode(golem_find_range_overide_pow, player_transform, transform);
        Inverter player_to_close_ignore_node_inverter = new Inverter(player_to_close_ignore_node);
        IsGolemAvailableNode is_golem_available_node = new IsGolemAvailableNode(throwable_parrent_transform, this);
        Selector go_to_golem_selector = new Selector(new List<Node> { pick_up_golem_sequence, go_to_golem_sequence });
        Sequence find_golem_sequence = new Sequence(new List<Node> { player_to_close_ignore_node_inverter, is_golem_available_node, go_to_golem_selector });

        Selector throw_selector = new Selector(new List<Node> { throw_golem_in_hands_sequence, find_golem_sequence });

        // Chases player
        RangeNode chasing_range_node = new RangeNode(chasing_range_pow, player_transform, transform);
        ChaseNode chase_node = new ChaseNode(player_transform, agent, this);
        Sequence chase_sequence = new Sequence(new List<Node> { chasing_range_node, chase_node });

        // Searches for player
        // NOT YET IMPLEMENTED
        Sequence search_sequence = new Sequence(new List<Node> { NOT_YET_IMPLEMENTED });

        top_node = new Selector(new List<Node> { health_sequence, hide_sequence, meele_sequence, throw_selector, chase_sequence, search_sequence });
    }

    public Transform GetClosestGolem()
    {
        return closest_golem;
    }

    public void PlaceGolemInHands()
    {
        Transform hand_transform = transform.Find("Mesh").Find("LowerArm.R");
        closest_golem.transform.parent = hand_transform;
        closest_golem.transform.localPosition = Vector3.zero;
        closest_golem.GetComponent<Agent>().CompleteStop();
    }

    public void SetColor(Color color)
    {
        material.color = color;
    }

    public void SetClosestGolem(Transform closest_golem)
    {
        this.closest_golem = closest_golem;
    }

    private Transform health_bar_transform;
    private Slider health_bar_slider;
    private Slider removed_health_bar_slider;
    private float health_bar_slider_pixel_width;
    private void InitHealthBar()
    {
        health_bar_transform = transform.Find("Canvas");
        float canvas_scale = 40f / (216f * transform.localScale.x);
        health_bar_transform.localScale = new Vector3(canvas_scale, canvas_scale, canvas_scale);
        Transform health_bar_slider_transform = health_bar_transform.GetChild(0);
        health_bar_slider = health_bar_slider_transform.GetComponent<Slider>();
        health_bar_slider_pixel_width = Mathf.RoundToInt(health_bar_slider_transform.GetComponent<RectTransform>().rect.width);
        Transform removed_health_bar_slider_transform = health_bar_slider_transform.GetChild(1);
        removed_health_bar_slider = removed_health_bar_slider_transform.GetComponent<Slider>();
    }
    private void UpdateHealthBar()
    {
        float bar_value = current_health / starting_health;
        health_bar_slider.value = (Mathf.CeilToInt(bar_value * health_bar_slider_pixel_width) / health_bar_slider_pixel_width);
    }

    private float _current_animated_float_value;
    private float current_animated_float_value
    {
        get { return _current_animated_float_value; }
        set { _current_animated_float_value = Mathf.Clamp(value, health_bar_slider.value, 1f); AnimatedHealthBar(); }
    }

    private void AnimatedHealthBar()
    {
        removed_health_bar_slider.value = (Mathf.CeilToInt(current_animated_float_value * health_bar_slider_pixel_width) / health_bar_slider_pixel_width);
    }

    private List<string> damage_id_list = new List<string>();
    /// <summary>
    /// Remembers passed in damage id and discards further attempts of damaging ai with the same damage id. Damage id gets removed after given time.
    /// </summary>
    public bool Damage(float damage, string damage_id = "", float invulnerability_time = 0.25f)
    {
        if (damage_id == "")
        {
            current_health -= damage;
            return true;
        }

        if (!damage_id_list.Contains(damage_id))
        {
            current_health -= damage;
            damage_id_list.Add(damage_id);
            StartCoroutine(RemoveIdFromList(invulnerability_time, damage_id));
            return true;
        }
        return false;
    }

    private IEnumerator RemoveIdFromList(float time, string item)
    {
        yield return new WaitForSeconds(time);
        damage_id_list.Remove(item);
    }
}
