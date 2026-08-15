using Sh.UIContract;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// written by Claude Opus 5
// the junk drawer handler. covers the three event types that don't belong to any particular
// bit of ui: sounds, camera shake, and GenericEffect.
//
// GenericEffect exists so designers can ask for a one-off without anyone writing a new event
// type. rather than a switch in here that grows forever, it's a list you fill in the inspector:
// give an effect an id, drag what should happen onto it, done.
public class MiscEffectHandler : BattleEventHandlerBase
{
    [Header("sound")]
    [SerializeField, Tooltip("where PlaySound events come out of")]
    private AudioSource audioSource;

    [Header("camera shake")]
    [SerializeField, Tooltip("what gets shaken. leave blank to use the main camera")]
    private Transform shakeTarget;
    [SerializeField] private float defaultShakeDuration = 0.25f;
    [SerializeField] private float defaultShakeStrength = 0.3f;

    [Header("generic effects")]
    [SerializeField, Tooltip("add an entry per one-off effect the designers ask for")]
    private List<GenericEffectBinding> genericEffects = new();

    protected override BattleEventType[] HandledTypes => new[]
    {
        BattleEventType.PlaySound,
        BattleEventType.GenericEffect,
        BattleEventType.CameraShake
    };

    protected override IEnumerator Handle(BattleEvent evt)
    {
        switch (evt.eventType)
        {
            case BattleEventType.PlaySound:
                yield return HandlePlaySound(evt);
                break;

            case BattleEventType.CameraShake:
                yield return HandleCameraShake(evt);
                break;

            case BattleEventType.GenericEffect:
                yield return HandleGenericEffect(evt);
                break;
        }
    }

    /// <summary>
    /// Fires a one-shot sound. The id can arrive on <c>strings[0]</c> (per the contract's note
    /// about triggering a specific file) or on <c>moveId</c>.
    ///
    /// Deliberately doesn't hold the event queue -- audio should overlap whatever's happening
    /// visually, not gate it. <c>floats[0]</c> is pitch and <c>floats[1]</c> is volume if set.
    /// </summary>
    IEnumerator HandlePlaySound(BattleEvent evt)
    {
        string soundId = GetFirstString(evt);

        if (string.IsNullOrEmpty(soundId))
        {
            soundId = evt.moveId;
        }

        if (string.IsNullOrEmpty(soundId))
        {
            Debug.LogWarning("[MiscEffectHandler] >> PlaySound event with no sound id on it.");
            yield break;
        }

        if (audioSource == null)
        {
            Debug.LogWarning($"[MiscEffectHandler] >> no audioSource assigned, can't play '{soundId}'.");
            yield break;
        }

        AudioClip clip = ResourceManager.Get<AudioClip>(BattleKeys.RootPaths.SFX + soundId);

        if (clip == null)
            yield break;

        // pitch/volume tweaks can ride along in floats
        audioSource.pitch = GetFloat(evt, 0, 1f);
        audioSource.PlayOneShot(clip, GetFloat(evt, 1, 1f));

        // sound shouldn't hold the queue up, so don't wait for it to finish
        yield break;
    }

    /// <summary>
    /// Shakes the camera. <c>floats[0]</c> is duration and <c>floats[1]</c> is strength;
    /// both fall back to the serialized defaults.
    /// </summary>
    IEnumerator HandleCameraShake(BattleEvent evt)
    {
        Transform target = shakeTarget != null ? shakeTarget : GetMainCameraTransform();

        if (target == null)
        {
            Debug.LogWarning("[MiscEffectHandler] >> nothing to shake.");
            yield break;
        }

        float duration = GetFloat(evt, 0, defaultShakeDuration);
        float strength = GetFloat(evt, 1, defaultShakeStrength);

        yield return ShakeRoutine(target, duration, strength);
    }

    // TODO (animation): replace with target.DOShakePosition(duration, strength) once we do the
    // DOTween pass. keeping it hand rolled for now so this doesn't depend on tween settings
    // that don't exist yet
    IEnumerator ShakeRoutine(Transform target, float duration, float strength)
    {
        Vector3 origin = target.localPosition;
        float elapsed = 0f;

        while (elapsed < duration && !isSkipping)
        {
            elapsed += Time.deltaTime;

            // ease the shake out so it settles instead of stopping dead
            float remaining = 1f - (elapsed / duration);
            Vector2 offset = UnityEngine.Random.insideUnitCircle * strength * remaining;

            target.localPosition = origin + new Vector3(offset.x, offset.y, 0f);

            yield return null;
        }

        target.localPosition = origin;
    }

    /// <summary>
    /// Runs whatever's bound to this effect id in the inspector.
    ///
    /// Warns rather than failing when nothing matches, since an unbound GenericEffect usually
    /// means a designer asked for something that hasn't been wired up yet.
    /// </summary>
    IEnumerator HandleGenericEffect(BattleEvent evt)
    {
        string effectId = GetFirstString(evt);

        if (string.IsNullOrEmpty(effectId))
        {
            effectId = evt.moveId;
        }

        foreach (GenericEffectBinding binding in genericEffects)
        {
            if (binding.effectId == effectId)
            {
                binding.onTriggered?.Invoke();

                if (binding.holdDuration > 0f)
                {
                    yield return WaitOrSkip(binding.holdDuration);
                }

                yield break;
            }
        }

        Debug.LogWarning($"[MiscEffectHandler] >> nothing bound for GenericEffect '{effectId}'. " +
            $"add it to the genericEffects list if it's meant to do something.");
    }

    private static Transform GetMainCameraTransform()
    {
        return Camera.main != null ? Camera.main.transform : null;
    }

    private static string GetFirstString(BattleEvent evt)
    {
        if (evt.strings == null || evt.strings.Count == 0)
            return null;

        return evt.strings[0];
    }

    // the floats list is an optional junk drawer, so every read needs a default
    private static float GetFloat(BattleEvent evt, int index, float fallback)
    {
        if (evt.floats == null || index >= evt.floats.Count)
            return fallback;

        return evt.floats[index];
    }
}

/// <summary>
/// One designer-requested one-off, wired up in the inspector.
///
/// This is what keeps <c>GenericEffect</c> from turning into an ever-growing switch statement:
/// new effects get added as list entries, not code.
/// </summary>
[Serializable]
public class GenericEffectBinding
{
    [Tooltip("matched against strings[0] (or moveId) on the GenericEffect event")]
    public string effectId;

    [Tooltip("how long to hold the event queue while this plays. 0 = don't wait")]
    public float holdDuration;

    public UnityEvent onTriggered;
}
