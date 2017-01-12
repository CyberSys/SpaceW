﻿#region License
// Procedural planet generator.
// 
// Copyright (C) 2015-2017 Denis Ovchinnikov [zameran] 
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

using System;

using UnityEngine;

[Serializable]
public sealed class Chunk : Space
{
    public const byte Size = 8;
    public const byte HalfSize = Size / 2;
    public const long SpaceSize = 65536;
    public const long SideSize = Size * SpaceSize;

    public Cluster[,,] Clusters;

    public Chunk() : base()
    {
    }

    public Chunk(Vector3d Position) : base(Position)
    {
    }

    public override void Init()
    {
        Clusters = new Cluster[Size, Size, Size];

        for (sbyte x = -HalfSize; x < HalfSize; x++)
        {
            for (sbyte y = -HalfSize; y < HalfSize; y++)
            {
                for (sbyte z = -HalfSize; z < HalfSize; z++)
                {
                    var idx = x + HalfSize;
                    var idy = y + HalfSize;
                    var idz = z + HalfSize;

                    var position = new Vector3d(x * SpaceSize, y * SpaceSize, z * SpaceSize) + new Vector3d(SpaceSize / 2.0, SpaceSize / 2.0, SpaceSize / 2.0);
                    var cluster = new Cluster(position);

                    cluster.Parent = this;
                    cluster.Init();
                    cluster.Update(Position + position);

                    Clusters[idx, idy, idz] = cluster;
                }
            }
        }
    }

    public override void Update(Vector3d Position)
    {
        if (Clusters != null)
        {
            for (byte x = 0; x < Size; x++)
            {
                for (byte y = 0; y < Size; y++)
                {
                    for (byte z = 0; z < Size; z++)
                    {
                        var cluster = Clusters[x, y, z];

                        cluster.Update(Position + (cluster.Position - this.Position));
                    }
                }
            }
        }

        this.Position = Position;
    }

    public void OnDrawGizmos()
    {
        if (Clusters == null) return;

        for (byte x = 0; x < Size; x++)
        {
            for (byte y = 0; y < Size; y++)
            {
                for (byte z = 0; z < Size; z++)
                {
                    var cluster = Clusters[x, y, z];

                    if (cluster != null)
                    {
                        Gizmos.color = Color.blue;
                        Gizmos.DrawWireCube(cluster.Position, Vector3.one * SpaceSize);
                    }
                }
            }
        }
    }
}