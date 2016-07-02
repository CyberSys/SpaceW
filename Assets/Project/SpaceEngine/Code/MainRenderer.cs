﻿#region License
// Procedural planet generator.
// 
// Copyright (C) 2015-2016 Denis Ovchinnikov [zameran] 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. Neither the name of the copyright holders nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION)HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// 
// Creation Date: Undefined
// Creation Time: Undefined
// Creator: zameran
#endregion

using System.Collections.Generic;

using UnityEngine;

[RequireComponent(typeof(Camera))]
public sealed class MainRenderer : MonoBehaviour
{
    public bool OverrideExternalRendering = true;

    public List<Planetoid> planets = new List<Planetoid>();

    public Planet.PlanetoidDistanceToLODTargetComparer pdtltc;

    private void Start()
    {
        if (pdtltc == null)
            pdtltc = new Planet.PlanetoidDistanceToLODTargetComparer();

        Planetoid[] p = FindObjectsOfType<Planetoid>();

        for (int i = 0; i < p.Length; i++)
        {
            planets.Add(p[i]);
        }

        for (int i = 0; i < planets.Count; i++)
        {
            if (planets[i] != null)
                if (!planets[i].ExternalRendering && OverrideExternalRendering)
                    planets[i].ExternalRendering = true;
        }
    }

    private void Update()
    {
        Render();
    }

    private void OnEnable()
    {
        for (int i = 0; i < planets.Count; i++)
        {
            if (planets[i] != null)
                if (!planets[i].ExternalRendering && OverrideExternalRendering)
                    planets[i].ExternalRendering = true;
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < planets.Count; i++)
        {
            if (planets[i] != null)
                if (!planets[i].ExternalRendering && OverrideExternalRendering)
                    planets[i].ExternalRendering = false;
        }
    }

    public void Render()
    {
        planets.Sort(pdtltc);

        for (int i = 0; i < planets.Count; i++)
        {
            if (planets[i] != null)
                planets[i].Render(CameraHelper.Main());
        }

        //-----------------------------------------------------------------------------
        planets[0].RenderQueueOffset = 10000;
        if (planets[0].Atmosphere != null) { planets[0].Atmosphere.RenderQueueOffset = 10001; }
        for (int i = 1; i < planets.Count; i++)
        {
            planets[i].RenderQueueOffset = 0;
            if (planets[i].Atmosphere != null) planets[i].Atmosphere.RenderQueueOffset = 1;
        }
        //-----------------------------------------------------------------------------
    }
}