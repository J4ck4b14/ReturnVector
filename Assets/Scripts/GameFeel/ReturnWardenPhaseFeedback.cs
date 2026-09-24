using System.Collections;
using ReturnVector.Combat;
using ReturnVector.Enemies;
using UnityEngine;

namespace ReturnVector.GameFeel
{
    /// <summary>
    /// Drives Warden transformations, shockwaves and the final death beat.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ReturnWardenPhaseFeedback : MonoBehaviour
    {
        private const int RingSegments = 56;
        private const int DebrisCount = 12;
        private const int DeathFragmentCount = 8;
        private const int RoarSampleRate = 22050;

        [SerializeField] private ReturnWardenHealth health;
        [SerializeField] private ReturnWardenAI ai;
        [SerializeField] private ReturnWardenTuning tuning;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Renderer[] bodyRenderers;
        [SerializeField] private Material phaseTwoMaterial;
        [SerializeField] private Material effectMaterial;
        [SerializeField] private RVCameraFeedback cameraFeedback;
        [SerializeField] private EnemyAttackFeedback attackFeedback;

        private Vector3 baseVisualScale;
        private Vector3 baseVisualPosition;
        private AudioSource roarSource;
        private AudioClip roarClip;
        private AudioClip shockwaveClip;
        private Material runtimePhaseMaterial;
        private bool phaseTwoBeatStarted;
        private bool phaseThreeBeatStarted;
        private bool deathStarted;

        public void Configure(
            ReturnWardenHealth newHealth,
            ReturnWardenAI newAi,
            ReturnWardenTuning newTuning,
            Transform newVisualRoot,
            Renderer[] newBodyRenderers,
            Material newPhaseTwoMaterial,
            Material newEffectMaterial,
            RVCameraFeedback newCameraFeedback,
            EnemyAttackFeedback newAttackFeedback)
        {
            Unsubscribe();

            health = newHealth;
            ai = newAi;
            tuning = newTuning;
            visualRoot = newVisualRoot;
            bodyRenderers = newBodyRenderers ?? new Renderer[0];
            phaseTwoMaterial = newPhaseTwoMaterial;
            effectMaterial = newEffectMaterial;
            cameraFeedback = newCameraFeedback;
            attackFeedback = newAttackFeedback;

            if (visualRoot != null)
            {
                baseVisualScale = visualRoot.localScale;
                baseVisualPosition = visualRoot.localPosition;
            }

            if (phaseTwoMaterial != null)
            {
                runtimePhaseMaterial =
                    new Material(
                        phaseTwoMaterial);

                runtimePhaseMaterial.name =
                    "M_WardenSecondPhase_Runtime";
            }

            EnsureRoarSource();
            Subscribe();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (ai != null)
            {
                ai.PhaseTransitionStarted -=
                    HandlePhaseTwoStarted;

                ai.PhaseTransitionStarted +=
                    HandlePhaseTwoStarted;

                ai.PhaseThreeTransitionStarted -=
                    HandlePhaseThreeStarted;

                ai.PhaseThreeTransitionStarted +=
                    HandlePhaseThreeStarted;

                ai.ShockwaveReleased -=
                    HandleShockwaveReleased;

                ai.ShockwaveReleased +=
                    HandleShockwaveReleased;
            }

            if (health != null)
            {
                health.Died -= HandleDeath;
                health.Died += HandleDeath;
            }
        }

        private void Unsubscribe()
        {
            if (ai != null)
            {
                ai.PhaseTransitionStarted -=
                    HandlePhaseTwoStarted;

                ai.PhaseThreeTransitionStarted -=
                    HandlePhaseThreeStarted;

                ai.ShockwaveReleased -=
                    HandleShockwaveReleased;
            }

            if (health != null)
            {
                health.Died -= HandleDeath;
            }
        }

        private void HandlePhaseTwoStarted()
        {
            if (phaseTwoBeatStarted)
            {
                return;
            }

            phaseTwoBeatStarted = true;
            StartCoroutine(
                PlayPhaseTwoTransformation());
        }

        private void HandlePhaseThreeStarted()
        {
            if (phaseThreeBeatStarted)
            {
                return;
            }

            phaseThreeBeatStarted = true;
            StartCoroutine(
                PlayPhaseThreeTransformation());
        }

        private IEnumerator PlayPhaseTwoTransformation()
        {
            float duration =
                tuning != null
                    ? Mathf.Max(
                        0.5f,
                        tuning.PhaseTransitionDuration)
                    : 2.35f;

            SetAttackFeedbackEnabled(false);
            PlayRoar(0.88f, 0.94f);

            cameraFeedback?.Impulse(
                0.68f,
                1.7f,
                0.34f);

            StartCoroutine(
                PlayExpandingRing(
                    0.4f,
                    7.4f,
                    0.9f,
                    0f));

            StartCoroutine(
                PlayExpandingRing(
                    0.8f,
                    9f,
                    1.2f,
                    0.24f));

            StartCoroutine(
                PlayDebrisBurst(
                    duration * 0.74f));

            float elapsed = 0f;
            bool materialChanged = false;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t =
                    Mathf.Clamp01(
                        elapsed / duration);

                if (!materialChanged &&
                    t >= 0.38f)
                {
                    materialChanged = true;
                    ApplyPhaseTwoMaterial();

                    cameraFeedback?.Impulse(
                        0.46f,
                        0.82f,
                        0.2f);
                }

                AnimateTransformationPose(
                    t,
                    0.30f,
                    0.40f);

                yield return null;
            }

            if (!materialChanged)
            {
                ApplyPhaseTwoMaterial();
            }

            RestoreVisualPose();
            SetAttackFeedbackEnabled(true);
        }

        private IEnumerator PlayPhaseThreeTransformation()
        {
            float duration =
                tuning != null
                    ? Mathf.Max(
                        0.5f,
                        tuning.PhaseThreeTransitionDuration)
                    : 2.65f;

            SetAttackFeedbackEnabled(false);
            PlayRoar(1f, 0.82f);

            cameraFeedback?.Impulse(
                0.82f,
                1.9f,
                0.38f);

            StartCoroutine(
                PlayExpandingRing(
                    0.4f,
                    8.2f,
                    0.78f,
                    0f));

            StartCoroutine(
                PlayExpandingRing(
                    1.0f,
                    10f,
                    1.05f,
                    0.2f));

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t =
                    Mathf.Clamp01(
                        elapsed / duration);

                float pulse =
                    Mathf.Sin(
                        t * Mathf.PI * 14f) *
                    (1f - t) *
                    0.055f;

                if (visualRoot != null)
                {
                    float rise =
                        Mathf.Sin(t * Mathf.PI);

                    visualRoot.localScale =
                        baseVisualScale +
                        Vector3.one *
                        (0.19f * rise + pulse);

                    visualRoot.localPosition =
                        baseVisualPosition +
                        Vector3.up *
                        (0.22f * rise);
                }

                yield return null;
            }

            RestoreVisualPose();
            SetAttackFeedbackEnabled(true);
        }

        private void HandleShockwaveReleased()
        {
            float range =
                tuning != null
                    ? Mathf.Max(
                        1f,
                        tuning.ShockwaveRange)
                    : 4.25f;

            cameraFeedback?.Impulse(
                0.52f,
                0.58f,
                0.20f);

            EnsureRoarSource();

            if (roarSource != null &&
                shockwaveClip != null)
            {
                roarSource.pitch = 0.92f;
                roarSource.PlayOneShot(
                    shockwaveClip,
                    0.92f);
            }

            StartCoroutine(
                PlayExpandingRing(
                    0.35f,
                    range,
                    0.30f,
                    0f));

            StartCoroutine(
                PlayExpandingRing(
                    0.65f,
                    range * 1.12f,
                    0.42f,
                    0.06f));
        }

        private void HandleDeath(
            DamageInfo damage)
        {
            if (deathStarted)
            {
                return;
            }

            deathStarted = true;
            StartCoroutine(
                PlayFinalDeath());
        }

        private IEnumerator PlayFinalDeath()
        {
            SetAttackFeedbackEnabled(false);
            PlayRoar(1f, 0.72f);

            cameraFeedback?.Impulse(
                0.88f,
                1.35f,
                0.34f);

            StartCoroutine(
                PlayExpandingRing(
                    0.5f,
                    9f,
                    0.9f,
                    0.12f));

            float roarSeconds = 0.72f;
            float elapsed = 0f;

            while (elapsed < roarSeconds)
            {
                elapsed +=
                    Time.unscaledDeltaTime;

                float t =
                    Mathf.Clamp01(
                        elapsed /
                        roarSeconds);

                float tremor =
                    Mathf.Sin(
                        t * Mathf.PI * 18f) *
                    (1f - t) *
                    0.045f;

                if (visualRoot != null)
                {
                    visualRoot.localScale =
                        baseVisualScale *
                        (1f +
                         0.18f * t +
                         tremor);

                    visualRoot.localPosition =
                        baseVisualPosition +
                        Vector3.up *
                        (0.12f * t);
                }

                yield return null;
            }

            SpawnDeathFragments();
            SetBodyVisible(false);

            cameraFeedback?.Impulse(
                0.72f,
                0.72f,
                0.26f);

            yield return
                new WaitForSecondsRealtime(
                    0.82f);

            if (gameObject != null)
            {
                Destroy(gameObject);
            }
        }

        private void SpawnDeathFragments()
        {
            Material fragmentMaterial = effectMaterial;

            if (bodyRenderers != null &&
                bodyRenderers.Length > 0 &&
                bodyRenderers[0] != null &&
                bodyRenderers[0].sharedMaterial != null)
            {
                fragmentMaterial =
                    bodyRenderers[0].sharedMaterial;
            }

            for (int i = 0;
                 i < DeathFragmentCount;
                 i++)
            {
                float angle =
                    i /
                    (float)DeathFragmentCount *
                    Mathf.PI *
                    2f;

                Vector3 direction =
                    new Vector3(
                        Mathf.Cos(angle),
                        0.38f +
                        (i % 3) * 0.12f,
                        Mathf.Sin(angle))
                    .normalized;

                GameObject fragment =
                    GameObject.CreatePrimitive(
                        PrimitiveType.Cube);

                fragment.name =
                    "Warden_Fragment";

                fragment.transform.position =
                    transform.position +
                    Vector3.up * 0.25f;

                fragment.transform.localScale =
                    Vector3.one *
                    (0.18f +
                     (i % 3) * 0.04f);

                Renderer renderer =
                    fragment.GetComponent<
                        Renderer>();

                if (renderer != null &&
                    fragmentMaterial != null)
                {
                    renderer.sharedMaterial =
                        fragmentMaterial;
                }

                Collider collider =
                    fragment.GetComponent<
                        Collider>();

                if (collider != null)
                {
                    collider.enabled = false;
                    Destroy(collider);
                }

                StartCoroutine(
                    AnimateDeathFragment(
                        fragment,
                        direction,
                        0.7f +
                        (i % 4) * 0.08f));
            }
        }

        private IEnumerator AnimateDeathFragment(
            GameObject fragment,
            Vector3 direction,
            float duration)
        {
            if (fragment == null)
            {
                yield break;
            }

            Vector3 start =
                fragment.transform.position;

            float elapsed = 0f;

            while (elapsed < duration &&
                   fragment != null)
            {
                elapsed +=
                    Time.unscaledDeltaTime;

                float t =
                    Mathf.Clamp01(
                        elapsed /
                        duration);

                float arc =
                    4f *
                    t *
                    (1f - t);

                fragment.transform.position =
                    start +
                    direction *
                    (2.4f * t) +
                    Vector3.up *
                    (0.5f * arc);

                fragment.transform.Rotate(
                    170f *
                    Time.unscaledDeltaTime,
                    230f *
                    Time.unscaledDeltaTime,
                    120f *
                    Time.unscaledDeltaTime,
                    Space.Self);

                fragment.transform.localScale =
                    Vector3.one *
                    Mathf.Lerp(
                        0.22f,
                        0f,
                        t);

                yield return null;
            }

            if (fragment != null)
            {
                Destroy(fragment);
            }
        }

        private void ApplyPhaseTwoMaterial()
        {
            Material material =
                runtimePhaseMaterial != null
                    ? runtimePhaseMaterial
                    : phaseTwoMaterial;

            if (material == null ||
                bodyRenderers == null)
            {
                return;
            }

            for (int i = 0;
                 i < bodyRenderers.Length;
                 i++)
            {
                Renderer renderer =
                    bodyRenderers[i];

                if (renderer == null)
                {
                    continue;
                }

                renderer.SetPropertyBlock(null);
                renderer.sharedMaterial =
                    material;
            }
        }

        private void AnimateTransformationPose(
            float t,
            float scaleAmount,
            float riseAmount)
        {
            if (visualRoot == null)
            {
                return;
            }

            float gather =
                t < 0.48f
                    ? Mathf.SmoothStep(
                        0f,
                        1f,
                        t / 0.48f)
                    : 1f -
                      Mathf.SmoothStep(
                          0f,
                          1f,
                          (t - 0.48f) /
                          0.52f);

            float pulse =
                Mathf.Sin(
                    t *
                    Mathf.PI *
                    10f) *
                (1f - t) *
                0.035f;

            visualRoot.localScale =
                baseVisualScale +
                Vector3.one *
                ((scaleAmount + pulse) *
                 gather);

            visualRoot.localPosition =
                baseVisualPosition +
                Vector3.up *
                (riseAmount * gather);
        }

        private void RestoreVisualPose()
        {
            if (visualRoot == null)
            {
                return;
            }

            visualRoot.localScale =
                baseVisualScale;

            visualRoot.localPosition =
                baseVisualPosition;
        }

        private void SetAttackFeedbackEnabled(
            bool enabled)
        {
            if (attackFeedback != null)
            {
                attackFeedback.enabled =
                    enabled;
            }
        }

        private void SetBodyVisible(
            bool visible)
        {
            if (bodyRenderers == null)
            {
                return;
            }

            for (int i = 0;
                 i < bodyRenderers.Length;
                 i++)
            {
                if (bodyRenderers[i] != null)
                {
                    bodyRenderers[i].enabled =
                        visible;
                }
            }
        }

        private void PlayRoar(
            float volume,
            float pitch)
        {
            EnsureRoarSource();

            if (roarSource == null ||
                roarClip == null)
            {
                return;
            }

            roarSource.Stop();
            roarSource.pitch = pitch;
            roarSource.volume = volume;
            roarSource.clip = roarClip;
            roarSource.Play();
        }

        private IEnumerator PlayExpandingRing(
            float startRadius,
            float endRadius,
            float duration,
            float delay)
        {
            if (delay > 0f)
            {
                yield return
                    new WaitForSecondsRealtime(
                        delay);
            }

            GameObject ringObject =
                new GameObject(
                    "Warden_Ring");

            ringObject.transform.position =
                transform.position +
                Vector3.down * 0.91f;

            LineRenderer line =
                ringObject.AddComponent<
                    LineRenderer>();

            line.useWorldSpace = false;
            line.loop = true;
            line.positionCount = RingSegments;
            line.widthMultiplier = 0.07f;
            line.numCapVertices = 2;

            if (effectMaterial != null)
            {
                line.sharedMaterial =
                    effectMaterial;
            }

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed +=
                    Time.unscaledDeltaTime;

                float t =
                    Mathf.Clamp01(
                        elapsed /
                        Mathf.Max(
                            0.01f,
                            duration));

                float radius =
                    Mathf.Lerp(
                        startRadius,
                        endRadius,
                        1f -
                        Mathf.Pow(
                            1f - t,
                            3f));

                line.widthMultiplier =
                    Mathf.Lerp(
                        0.12f,
                        0.025f,
                        t);

                for (int i = 0;
                     i < RingSegments;
                     i++)
                {
                    float angle =
                        i /
                        (float)RingSegments *
                        Mathf.PI *
                        2f;

                    line.SetPosition(
                        i,
                        new Vector3(
                            Mathf.Cos(angle) *
                            radius,
                            0.03f,
                            Mathf.Sin(angle) *
                            radius));
                }

                yield return null;
            }

            Destroy(ringObject);
        }

        private IEnumerator PlayDebrisBurst(
            float duration)
        {
            GameObject[] debris =
                new GameObject[
                    DebrisCount];

            Vector3[] starts =
                new Vector3[
                    DebrisCount];

            Vector3[] directions =
                new Vector3[
                    DebrisCount];

            float[] heights =
                new float[
                    DebrisCount];

            for (int i = 0;
                 i < DebrisCount;
                 i++)
            {
                float angle =
                    i /
                    (float)DebrisCount *
                    Mathf.PI *
                    2f;

                Vector3 direction =
                    new Vector3(
                        Mathf.Cos(angle),
                        0f,
                        Mathf.Sin(angle));

                GameObject piece =
                    GameObject.CreatePrimitive(
                        i % 3 == 0
                            ? PrimitiveType.Capsule
                            : PrimitiveType.Cube);

                piece.name =
                    "Warden_Debris";

                piece.transform.localScale =
                    Vector3.one *
                    (0.08f +
                     (i % 4) *
                     0.025f);

                starts[i] =
                    transform.position +
                    Vector3.down *
                    0.78f +
                    direction *
                    (0.8f +
                     (i % 3) *
                     0.28f);

                piece.transform.position =
                    starts[i];

                directions[i] =
                    direction;

                heights[i] =
                    0.55f +
                    (i % 5) *
                    0.12f;

                Renderer renderer =
                    piece.GetComponent<
                        Renderer>();

                if (renderer != null &&
                    effectMaterial != null)
                {
                    renderer.sharedMaterial =
                        effectMaterial;
                }

                Collider collider =
                    piece.GetComponent<
                        Collider>();

                if (collider != null)
                {
                    collider.enabled = false;
                    Destroy(collider);
                }

                debris[i] = piece;
            }

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed +=
                    Time.unscaledDeltaTime;

                float t =
                    Mathf.Clamp01(
                        elapsed /
                        Mathf.Max(
                            0.01f,
                            duration));

                float arc =
                    4f *
                    t *
                    (1f - t);

                for (int i = 0;
                     i < debris.Length;
                     i++)
                {
                    GameObject piece =
                        debris[i];

                    if (piece == null)
                    {
                        continue;
                    }

                    piece.transform.position =
                        starts[i] +
                        directions[i] *
                        (1.35f * t) +
                        Vector3.up *
                        heights[i] *
                        arc;

                    piece.transform.Rotate(
                        130f *
                        Time.unscaledDeltaTime,
                        190f *
                        Time.unscaledDeltaTime,
                        90f *
                        Time.unscaledDeltaTime,
                        Space.Self);
                }

                yield return null;
            }

            for (int i = 0;
                 i < debris.Length;
                 i++)
            {
                if (debris[i] != null)
                {
                    Destroy(debris[i]);
                }
            }
        }

        private void EnsureRoarSource()
        {
            if (roarSource == null)
            {
                roarSource =
                    GetComponent<
                        AudioSource>();

                if (roarSource == null)
                {
                    roarSource =
                        gameObject.AddComponent<
                            AudioSource>();
                }

                roarSource.playOnAwake = false;
                roarSource.loop = false;
                roarSource.spatialBlend = 0.45f;
                roarSource.volume = 0.92f;
                roarSource.minDistance = 2f;
                roarSource.maxDistance = 32f;
            }

            if (roarClip == null)
            {
                roarClip =
                    BuildRoarClip();
            }

            if (shockwaveClip == null)
            {
                shockwaveClip =
                    BuildShockwaveClip();
            }
        }

        private static AudioClip BuildRoarClip()
        {
            const float duration = 1.65f;
            int sampleCount =
                Mathf.CeilToInt(
                    RoarSampleRate *
                    duration);

            float[] samples =
                new float[
                    sampleCount];

            uint noiseState =
                0x7A31B59Du;

            for (int i = 0;
                 i < sampleCount;
                 i++)
            {
                float t =
                    i /
                    (float)RoarSampleRate;

                float normalized =
                    t /
                    duration;

                float attack =
                    Mathf.Clamp01(
                        normalized /
                        0.1f);

                float release =
                    Mathf.Clamp01(
                        (1f - normalized) /
                        0.32f);

                float envelope =
                    attack *
                    release;

                noiseState =
                    noiseState *
                    1664525u +
                    1013904223u;

                float noise =
                    ((noiseState >> 9) &
                     0x7FFFFF) /
                    4194303.5f -
                    1f;

                float fundamental =
                    Mathf.Sin(
                        t *
                        Mathf.PI *
                        2f *
                        52f);

                float second =
                    Mathf.Sin(
                        t *
                        Mathf.PI *
                        2f *
                        78f +
                        0.7f);

                float rumble =
                    Mathf.Sin(
                        t *
                        Mathf.PI *
                        2f *
                        31f) *
                    0.5f;

                samples[i] =
                    (fundamental *
                     0.44f +
                     second *
                     0.24f +
                     rumble *
                     0.18f +
                     noise *
                     0.14f) *
                    envelope *
                    0.76f;
            }

            AudioClip clip =
                AudioClip.Create(
                    "Warden_Roar",
                    sampleCount,
                    1,
                    RoarSampleRate,
                    false);

            clip.SetData(
                samples,
                0);

            return clip;
        }

        private static AudioClip BuildShockwaveClip()
        {
            const float duration = 0.42f;

            int sampleCount =
                Mathf.CeilToInt(
                    RoarSampleRate *
                    duration);

            float[] samples =
                new float[
                    sampleCount];

            uint noiseState =
                0x2E5A4C13u;

            for (int i = 0;
                 i < sampleCount;
                 i++)
            {
                float t =
                    i /
                    (float)RoarSampleRate;

                float normalized =
                    t /
                    duration;

                float envelope =
                    Mathf.Pow(
                        1f -
                        normalized,
                        2.2f);

                noiseState =
                    noiseState *
                    1664525u +
                    1013904223u;

                float noise =
                    ((noiseState >> 9) &
                     0x7FFFFF) /
                    4194303.5f -
                    1f;

                float boom =
                    Mathf.Sin(
                        t *
                        Mathf.PI *
                        2f *
                        43f) *
                    0.68f +
                    Mathf.Sin(
                        t *
                        Mathf.PI *
                        2f *
                        67f) *
                    0.22f;

                samples[i] =
                    (boom +
                     noise *
                     0.18f) *
                    envelope *
                    0.82f;
            }

            AudioClip clip =
                AudioClip.Create(
                    "Warden_Shockwave",
                    sampleCount,
                    1,
                    RoarSampleRate,
                    false);

            clip.SetData(
                samples,
                0);

            return clip;
        }
    }
}
