// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using Narupa.Core.Math;
using Narupa.Frame;
using Narupa.Visualisation.Properties;
using Narupa.Visualisation.Properties.Collections;
using Narupa.Visualisation.Property;
using Narupa.Visualisation.Utility;
using Plugins.Narupa.Visualisation.Node.Renderer;
using UnityEngine;

namespace Narupa.Visualisation.Node.Renderer
{
    /// <summary>
    /// Visualisation node for rendering hyperballs between particles.
    /// </summary>
    [Serializable]
    public class HyperballRendererNode : ParticleBondRendererNode
    {
        public override bool ShouldRender => base.ShouldRender
                                          && tension.HasValue;

        public override bool IsInputDirty => base.IsInputDirty
                                          || tension.IsDirty;

        protected override void SetMaterialParameters()
        {
            base.SetMaterialParameters();
            DrawCommand.SetFloat("_Tension", tension.Value);
        }
        
        [SerializeField]
        private FloatProperty tension = new FloatProperty();

    }
}