using Framework;
using UnityEngine;
using UnityEngine.PostProcessing;

[RequireComponent(typeof(PostProcessingBehaviour))]
public class HealthBasedVignette : MonoBehaviour
{
    public PostProcessingBehaviour behaviour; // assign in Inspector
    public float healthBasedSpeedMultiplier = 1;

    private void Awake()
    {
        // create runtime profile so the project files aren't modified permanently
        behaviour.profile = Instantiate(behaviour.profile);
    }

    private void SetVignetteSmoothness( float value )
    {
        var vignette = behaviour.profile.vignette.settings;
        vignette.smoothness = value;
        behaviour.profile.vignette.settings = vignette;
    }

    private void Update()
    {
        var player = GlobalManager.Instance.Player.GetRepresent();
        if (!player) return;

        var healthPercent = player.GetComponent<ScriptHealth>().Percent();
        var speed = 1 + (1 - healthPercent) * healthBasedSpeedMultiplier; // scale speed with health
        var wave = Mathf.Abs(Mathf.Sin(Time.realtimeSinceStartup * speed));
        SetVignetteSmoothness((1 - healthPercent) * (0.5f + wave / 2f));
    }
}