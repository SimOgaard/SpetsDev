using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Holds general functionality that all enemies should inherrent from.
/// </summary>
public class EnemyAI : MonoBehaviour
{
    public interface IAIBehaviour
    {
        void Die();
    }

    [HideInInspector] public Agent agent;
    [HideInInspector] public Transform player_transform;
    public Transform chase_transform;

    [SerializeField] private float health_remove_speed = 0.5f;
    [SerializeField] private float starting_health;
    private float _current_health;
    public float current_health
    {
        get { return _current_health; }
        private set
        {

            if (!health_bar_game_object.activeSelf)
            {
                health_bar_game_object.SetActive(true);
            }

            if (value <= 0f)
            {
                _current_health = 0f;
                UpdateHealthBar();
                //Die();
                return;
            }
            else if (value > starting_health)
            {
                _current_health = starting_health;
            }
            else
            {
                _current_health = value;
            }
            UpdateHealthBar();
        }
    }

    public float damage;

    private void ControllActiveCanvas(bool canvas, bool question, bool exclamation)
    {
        if (canvas_game_object.activeSelf != canvas)
        {
            canvas_game_object.SetActive(canvas);
        }
        if (question_mark_game_object.activeSelf != question)
        {
            question_mark_game_object.SetActive(question);
        }
        if (exclamation_point_game_object.activeSelf != exclamation)
        {
            exclamation_point_game_object.SetActive(exclamation);
        }
    }

    private IEnumerator ControllActiveCanvasEnumerator(bool canvas, bool question, bool exclamation)
    {
        yield return new WaitForSeconds(exclamation_point_time);
        ControllActiveCanvas(canvas, question, exclamation);
    }

    public float hearing_amplification = 1f;
    [SerializeField] private float hearing_threshold = 0.0001f;
    [SerializeField] private float attention_decrease_factor = 0.1f;
    public static float exclamation_point_time = 2f;
    private float _current_attention = 0f;
    [SerializeField] private float max_attentiveness;
    public float current_attention
    {
        get { return _current_attention; }
        private set
        {
            // from 0 to 0.285 ? is fully white
            // from 0.8555 to 1 is fully red

            if (value <= 0f)
            {
                // canvas is not active
                ControllActiveCanvas(false, false, false);
                _current_attention = 0f;

                if (chase_transform != null)
                {
                    chase_transform = null;
                }
            }
            else if (value < 0.285f)
            {
                // ? is fully white
                ControllActiveCanvas(true, true, false);
                _current_attention = value;
            }
            else if (value < 1f)
            {
                // ? starts becomming red
                ControllActiveCanvas(true, true, false);
                _current_attention = value;
            }
            else
            {
                if (_current_attention < 1f)
                {
                    ControllActiveCanvas(true, false, true);
                    StartCoroutine(ControllActiveCanvasEnumerator(false, false, false));
                }
                if (value >= _current_attention)
                {
                    _current_attention = max_attentiveness;
                }
                else
                {
                    _current_attention = value;
                }
            }

            question_mark_material.SetFloat("_CutoffY", _current_attention);
        }
    }

    public void AttendToSound(Transform sound_origin, float sound_level, float hearing_threshold_change = 1f)
    {
        if (Mathf.Abs(sound_level) >= hearing_threshold * hearing_threshold_change)
        {
            current_attention += sound_level;

            if(current_attention == max_attentiveness && chase_transform != sound_origin && chase_transform != player_transform)
            {
                chase_transform = sound_origin;
            }
        }
    }

    private void Awake()
    {
        player_transform = GameObject.Find("Player").transform;
        agent = GetComponent<Agent>();
        InitHealthBar();
        _current_health = starting_health;
    }

    private void Update()
    {
        current_animated_float_value -= Time.deltaTime * health_remove_speed;
        current_attention -= Time.deltaTime * attention_decrease_factor;
    }

    [SerializeField] private GameObject canvas_game_object;
    private GameObject question_mark_game_object;
    private GameObject exclamation_point_game_object;
    private Material question_mark_material;
    private Material exclamation_point_material;
    [SerializeField] private GameObject health_bar_game_object;
    private Material health_material;
    private Material removed_health_material;
    private Material no_health_material;

    private float health_bar_slider_value = 1;

    [SerializeField] private Vector2 health_bar_pixel_size = Vector2.one;

    /// <summary>
    /// Inits health bar to correct size.
    /// </summary>
    private void InitHealthBar()
    {
        float parrent_scale_offset = 1f / transform.localScale.x;

        SpriteRenderer health_sprite_renderer = health_bar_game_object.transform.GetChild(0).GetComponent<SpriteRenderer>();
        SpriteRenderer removed_sprite_renderer = health_bar_game_object.transform.GetChild(1).GetComponent<SpriteRenderer>();
        SpriteRenderer no_health_sprite_renderer = health_bar_game_object.transform.GetChild(2).GetComponent<SpriteRenderer>();

        health_sprite_renderer.material = new Material(Shader.Find("Custom/SpriteBillboardShader"));
        removed_sprite_renderer.material = new Material(Shader.Find("Custom/SpriteBillboardShader"));
        no_health_sprite_renderer.material = new Material(Shader.Find("Custom/SpriteBillboardShader"));

        health_sprite_renderer.material.SetColor("_Color", health_sprite_renderer.color);
        removed_sprite_renderer.material.SetColor("_Color", removed_sprite_renderer.color);
        no_health_sprite_renderer.material.SetColor("_Color", no_health_sprite_renderer.color);

        health_sprite_renderer.material.renderQueue = 4003;
        removed_sprite_renderer.material.renderQueue = 4002;
        no_health_sprite_renderer.material.renderQueue = 4001;

        health_material = health_sprite_renderer.material;
        removed_health_material = removed_sprite_renderer.material;
        no_health_material = no_health_sprite_renderer.material;

        health_bar_game_object.transform.localScale = new Vector3(health_bar_pixel_size.x, SpriteInitializer.y_scale * health_bar_pixel_size.y, health_bar_pixel_size.x) * parrent_scale_offset;

        health_bar_game_object.SetActive(false);

        question_mark_game_object = canvas_game_object.transform.GetChild(0).gameObject;
        exclamation_point_game_object = canvas_game_object.transform.GetChild(1).gameObject;

        TextMeshProUGUI question_mark_text = question_mark_game_object.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI exclamation_point_text = exclamation_point_game_object.GetComponent<TextMeshProUGUI>();

        var font = question_mark_text.fontMaterial.GetTexture("_MainTex");

        question_mark_text.fontMaterial = new Material(Shader.Find("Custom/Font Color Shader"));
        exclamation_point_text.fontMaterial = new Material(Shader.Find("Custom/Font Color Shader"));

        question_mark_text.fontMaterial.SetColor("_Color1", health_sprite_renderer.color);
        exclamation_point_text.fontMaterial.SetColor("_Color2", removed_sprite_renderer.color);

        question_mark_text.fontMaterial.renderQueue = 4500;
        exclamation_point_text.fontMaterial.renderQueue = 4500;

        question_mark_text.fontMaterial.SetTexture("_MainTex", font);
        exclamation_point_text.fontMaterial.SetTexture("_MainTex", font);

        question_mark_material = question_mark_text.fontMaterial;
        exclamation_point_material = exclamation_point_text.fontMaterial;

        canvas_game_object.SetActive(false);
    }

    /// <summary>
    /// Uppdates the visuals of our health bar
    /// </summary>
    private void UpdateHealthBar()
    {
        float bar_value = current_health / starting_health;
        health_bar_slider_value = Mathf.CeilToInt(bar_value * health_bar_pixel_size.x) / health_bar_pixel_size.x;
        health_material.SetFloat("_CutoffX", health_bar_slider_value);
    }

    private float _current_animated_float_value;
    private float current_animated_float_value
    {
        get { return _current_animated_float_value; }
        set { _current_animated_float_value = Mathf.Clamp(value, health_bar_slider_value, 1f); AnimatedHealthBar(); }
    }

    /// <summary>
    /// Lerps removed/red health to black
    /// </summary>
    private void AnimatedHealthBar()
    {
        removed_health_material.SetFloat("_CutoffX" , Mathf.CeilToInt(current_animated_float_value * health_bar_pixel_size.x) / health_bar_pixel_size.x);
    }

    private List<string> damage_id_list = new List<string>();
    /// <summary>
    /// Remembers passed in damage id and discards further attempts of damaging ai with the same damage id. Damage id gets removed after given time.
    /// </summary>
    public bool Damage(float damage, string damage_id, float invulnerability_time = 0.25f)
    {
        if (!damage_id_list.Contains(damage_id) && damage != 0f)
        {
            current_health -= damage;
            damage_id_list.Add(damage_id);
            StartCoroutine(RemoveIdFromList(invulnerability_time, damage_id));
            return true;
        }
        return false;
    }
    /// <summary>
    /// Damages enemy by damage
    /// </summary>
    public bool Damage(float damage)
    {
        if (damage != 0f)
        {
            current_health -= damage;
        }
        return true;
    }

    /// <summary>
    /// Removes damage id from list
    /// </summary>
    private IEnumerator RemoveIdFromList(float time, string item)
    {
        yield return new WaitForSeconds(time);
        damage_id_list.Remove(item);
    }

    /// <summary>
    /// For debug purposes
    /// </summary>
    public void SetColor(Color color)
    {
        if (transform.TryGetComponent<MeshRenderer>(out MeshRenderer mesh_renderer_parrent))
        {
            mesh_renderer_parrent.material.SetColor("_Color", color);
        }
        foreach (Transform child in transform)
        {
            if(child.TryGetComponent<MeshRenderer>(out MeshRenderer mesh_renderer))
            {
                mesh_renderer.material.SetColor("_Color", color);
            }
        }
    }

    /// <summary>
    /// Kills this unit by disabeling scripts
    /// </summary>
    public void Die()
    {
        Debug.Log("died");
        SetColor(Color.black);

        agent.enabled = false;
        IAIBehaviour script = (IAIBehaviour)GetComponent(typeof(IAIBehaviour));
        script.Die();

        /*
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            script.enabled = false;
        }
        */
    }
}
