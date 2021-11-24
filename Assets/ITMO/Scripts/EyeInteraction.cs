using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NarupaIMD;
using NarupaIMD.Selection;
using Narupa.Core.Math;
using Narupa.Frontend.Manipulation;
using Narupa.Visualisation;
using Narupa.Visualisation.Properties;
using Narupa.Visualisation.Property;
using NarupaIMD.Interaction;
using NarupaXR.Interaction;

namespace NarupaXR.Interaction
{
    public class EyeInteraction : MonoBehaviour, IInteractableParticles
    {
        [Header("The provider of the frames which can be grabbed.")]
        [SerializeField]
        private SynchronisedFrameSource frameSource;

        [Header("The object which provides the selection information.")]
        [SerializeField]
        private VisualisationScene visualisationScene;

        [SerializeField]
        private NarupaImdSimulation simulation;

        [SerializeField]
        [Header("This object is atom prefab for raycasting")]
        private GameObject atomPrefab;

        private List<GameObject> spheres;

        private const string parentSpheresName = "ParentEyeInteraction";


        public enum InteractionTarget
        {
            Single,
            Residue
        }

        [SerializeField]
        private InteractionTarget interactionTarget = InteractionTarget.Single;

        public void SetInteractionTarget(InteractionTarget target)
        {
            this.interactionTarget = target;
        }

        public void SetInteractionTargetSingle()
        {
            SetInteractionTarget(InteractionTarget.Single);
        }

        public void SetInteractionTargetResidue()
        {
            SetInteractionTarget(InteractionTarget.Residue);
        }

        /// <inheritdoc cref="InteractedParticles"/>
        private readonly IntArrayProperty interactedParticles = new IntArrayProperty();


        /// <summary>
        /// The set of particles which are currently being interacted with.
        /// </summary>
        public IReadOnlyProperty<int[]> InteractedParticles => interactedParticles;

        private bool isInit = false;

        private void Update()
        {
            var interactions = simulation.Interactions;
            var pts = new List<int>();
            foreach (var interaction in interactions.Values)
                pts.AddRange(interaction.Particles);
            interactedParticles.Value = pts.ToArray();

            if (isInit)
            {
                var frame = frameSource.CurrentFrame;
                UpdateSpherePositions(frame);
            }
            else
            {
                var frame = frameSource.CurrentFrame;

                if (!isInit && frame != null && frame.ParticlePositions != null)
                {
                    isInit = true;
                    CreateSpheres(frame);
                }
            }
        }

        private void CreateSpheres(Narupa.Frame.Frame frame)
        {
            GameObject parent = new GameObject();

            parent.transform.parent = this.gameObject.transform;

            parent.name = parentSpheresName;
            parent.transform.localPosition = new Vector3(0f, 0f, 0f);
            parent.transform.localScale = new Vector3(1f, 1f, 1f);

            Instantiate(parent);

            spheres = new List<GameObject>();


            for (var i = 0; i < frame.ParticlePositions.Length; ++i)
            {
                var particlePosition = frame.ParticlePositions[i];

                GameObject sphere = Instantiate(atomPrefab, particlePosition, Quaternion.identity, parent.transform);

                spheres.Add(sphere);

                AtomInfo atomInfo = sphere.GetComponent<AtomInfo>();

                atomInfo.Index = frame.Particles[i].Index;
                atomInfo.Obj = sphere;
            }
        }


        private void UpdateSpherePositions(Narupa.Frame.Frame frame)
        {
            if (frame.ParticleCount != spheres.Count)
            {
                ReInit(frame);
                return;
            }


            for (var i = 0; i < frame.ParticlePositions.Length; ++i)
            {
                var atomPos = frame.ParticlePositions[i];

                spheres[i].transform.localPosition = new Vector3(atomPos.x, atomPos.y, atomPos.z);
            }
        }


        private void ReInit(Narupa.Frame.Frame frame)
        {
            GameObject parent = GameObject.Find(parentSpheresName);

            if (parent != null)
            {
                DestroySpheres();

                spheres = new List<GameObject>();

                for (var i = 0; i < frame.ParticlePositions.Length; ++i)
                {
                    var particlePosition = frame.ParticlePositions[i];

                    GameObject sphere = Instantiate(atomPrefab, particlePosition, Quaternion.identity, parent.transform);

                    spheres.Add(sphere);

                    AtomInfo atomInfo = sphere.GetComponent<AtomInfo>();

                    atomInfo.Index = frame.Particles[i].Index;
                    atomInfo.Obj = sphere;
                }
            }
        }


        private void DestroySpheres()
        {
            for (int i = spheres.Count - 1; i >= 0; --i)
            {
                Destroy(spheres[i]);
            }

            spheres.Clear();       
        }

        /// <summary>
        /// Attempt to grab the nearest particle, returning null if no interaction is possible.
        /// </summary>
        /// <param name="grabberPose">The transformation of the grabbing pivot.</param>
        public ActiveParticleGrab GetParticleGrab(Transformation grabberPose)
        {
            // here
            var particleIndex = GetParticleToWorldPositionByRay(grabberPose.Position);

            if (!particleIndex.HasValue)
                return null;

            var selection = visualisationScene.GetSelectionForParticle(particleIndex.Value);

            if (selection.Selection.InteractionMethod == ParticleSelection.InteractionMethodNone)
                return null;

            var indices = GetInteractionIndices(particleIndex.Value);

            var grab = new ActiveParticleGrab(indices);
            if (selection.Selection.ResetVelocities)
                grab.ResetVelocities = true;
            return grab;
        }

        private IEnumerable<int> GetInteractionIndices(int particleIndex)
        {
            switch (interactionTarget)
            {
                case InteractionTarget.Single:
                    yield return particleIndex;
                    break;
                case InteractionTarget.Residue:
                    var frame = simulation.FrameSynchronizer.CurrentFrame;
                    if (frame.ParticleResidues == null || frame.ParticleResidues.Length == 0)
                    {
                        yield return particleIndex;
                        break;
                    }

                    var residue = frame.ParticleResidues[particleIndex];
                    if (residue == -1)
                    {
                        yield return particleIndex;
                        break;
                    }
                    for (var i = 0; i < frame.ParticleCount; i++)
                        if (frame.ParticleResidues[i] == residue)
                            yield return i;
                    break;
            }
        }

        /// <summary>
        /// Get the particle indices to select, given the nearest particle index.
        /// </summary>
        private IReadOnlyList<int> GetIndicesInSelection(VisualisationSelection selection,
                                                      int particleIndex)
        {
            switch (selection.Selection.InteractionMethod)
            {
                case ParticleSelection.InteractionMethodGroup:
                    if (selection.FilteredIndices == null)
                        return Enumerable.Range(0, frameSource.CurrentFrame.ParticleCount)
                                         .ToArray();
                    else
                        return selection.FilteredIndices.Value;
                default:
                    return new[] { particleIndex };
            }
        }

        /// <summary>
        /// Get the particle to a given point in world space by ray.
        /// </summary>
        private int? GetParticleToWorldPositionByRay(Vector3 worldPosition, float cutoff = Mathf.Infinity)
        {
            Vector3 origin, direction;

            SRanipal.GetRay(out origin, out direction);

            var position = transform.InverseTransformPoint(worldPosition);

            var frame = frameSource.CurrentFrame;

            var bestSqrDistance = cutoff * cutoff;
            int? bestParticleIndex = null;

            for (var i = 0; i < frame.ParticlePositions.Length; ++i)
            {
                var particlePosition = frame.ParticlePositions[i];
                
            }

            return bestParticleIndex;
        }
    }
}
