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

using System;

using UnityEngine;

public static class BrainFuckMath
{
    public static Vector3 FromQuadPositionMask(float r, byte[] sign, byte[] axis, QuadPosition quadPosition)
    {
        var indexOf = Array.IndexOf(Enum.GetValues(typeof(QuadPosition)), quadPosition);

        var s = (sign[indexOf] == 1) ? -r : r;
        var v = axis[indexOf];

        var output = new Vector3(v == 0 ? s : 0.0f, v == 1 ? s : 0.0f, v == 2 ? s : 0.0f);

        return output;
    }

    public static bool AlmostEquals(this float a, float b, float precision)
    {
        return Mathf.Abs(a - b) <= precision;
    }

    public static void DefineAxis(ref bool staticX, ref bool staticY, ref bool staticZ, Vector3 size)
    {
        if (AlmostEquals(size.x, 0.0f, 0.01f))
            staticX = true;
        else if (AlmostEquals(size.y, 0.0f, 0.01f))
            staticY = true;
        else if (AlmostEquals(size.z, 0.0f, 0.01f))
            staticZ = true;
    }

    public static void LockAxis(ref float tempAxisValue, ref Vector3 temp, bool staticX, bool staticY, bool staticZ)
    {
        if (staticX)
            tempAxisValue = temp.x;
        else if (staticY)
            tempAxisValue = temp.y;
        else if (staticZ)
            tempAxisValue = temp.z;
    }

    public static void UnlockAxis(ref Vector3 temp, ref float tempAxisValue, bool staticX, bool staticY, bool staticZ)
    {
        if (staticX)
            temp.x = tempAxisValue;
        else if (staticY)
            temp.y = tempAxisValue;
        else if (staticZ)
            temp.z = tempAxisValue;
    }

    public static decimal CalculateK(int lodLevel)
    {
        if (lodLevel == 1)
            return 0.5M;
        else
        {
            decimal prev = CalculateK(lodLevel - 1);
            decimal summ = 1.0M;

            for (int i = 0; i < lodLevel; i++)
            {
                summ = summ / 2.0M;
            }

            return prev + summ;
        }
    }

    public static decimal CalculateJ(int lodLevel)
    {
        if (lodLevel == 0)
            return 15.0M;
        else if (lodLevel == 1)
            return 7.5M;

        decimal prev1 = CalculateJ(lodLevel - 1);
        decimal prev2 = CalculateJ(lodLevel - 2);

        decimal summ = Math.Abs(prev1 - prev2) / 2.0M;

        return prev1 + summ;
    }

    public static decimal CalculateI(decimal J)
    {
        return 15.0M / J;
    }

    public static Vector3 Multiply(Vector3 v, double d)
    {
        Vector3d vd = v; //Cast vector to double typed.

        var result = vd * d; //Multiply in doubles.

        return result.ToVector3(); //Cast it back to float typed.
    }

    public static Vector3 LinearInterpolate(Vector3 a, Vector3 b, double t)
    {
        Vector3d ad = a; //Cast first vector to double typed.
        Vector3d bd = b; //Cast second vector to double typed.

        var result = Vector3d.Lerp(ad, bd, t); //Lerp it.

        return result.ToVector3(); //Cast it back to float typed.
    }

    public static void CalculatePatchCubeCenter(int lodLevel, Vector3 patchCubeCenter, ref Vector3 temp)
    {
        /*
        1 : 15.0 / 7,5 | 0,5
        2 : 15.0 / 11,25 | 0,75
        3 : 15.0 / 13,125 | 0,875
        4 : 15.0 / 14,0625 | 0,9375
        5 : 15.0 / 14,53125 | 0,96875
        6 : 15.0 / 14,765625 | 0,984375
        7 : 15.0 / 14,8828125 | 0,9921875
        8 : 15.0 / 14,94140625 | 0,99609375
        9 : 15.0 / 14,970703125 | 0,998046875
        10 : 15.0 / 14,9853515625 | 0,9990234375
        11 : 15.0 / 14,99267578125 | 0,99951171875
        12 : 15.0 / 14,996337890625 | 0,999755859375
        13 : 15.0 / 14,9981689453125 | 0,9998779296875
        14 : 15.0 / 14,99908447265625 | 0,99993896484375
        15 : 15.0 / 14,999542236328125 | 0,999969482421875
        16 : 15.0 / 14,9997711181640625 | 0,9999847412109375
        17 : 15.0 / 14,99988555908203125 | 0,99999237060546875
        18 : 15.0 / 14,999942779541015625 | 0,999996185302734375
        19 : 15.0 / 14,9999713897705078125 | 0,9999980926513671875
        20 : 15.0 / 14,99998569488525390625 | 0,99999904632568359375
        21 : 15.0 / 14,999992847442626953125 | 0,999999523162841796875
        22 : 15.0 / 14,9999964237213134765625 | 0,9999997615814208984375
        23 : 15.0 / 14,99999821186065673828125 | 0,99999988079071044921875
        24 : 15.0 / 14,999999105930328369140625 | 0,999999940395355224609375
        25 : 15.0 / 14,9999995529651641845703125 | 0,9999999701976776123046875
        26 : 15.0 / 14,99999977648258209228515625 | 0,99999998509883880615234375
        27 : 15.0 / 14,999999888241291046142578125 | 0,999999992549419403076171875
        28 : 15.0 / 14,999999944120645523071289062 | 0,9999999962747097015380859375
        29 : 15.0 / 14,999999972060322761535644530 | 0,9999999981373548507690429687
        30 : 15.0 / 14,999999986030161380767822264 | 0,9999999990686774253845214843
        31 : 15.0 / 14,999999993015080690383911131 | 0,9999999995343387126922607421
        32 : 15.0 / 14,999999996507540345191955564 | 0,9999999997671693563461303710
        */

        //So. We have exponential modifier... WTF!?
        //Fuck dat shit. 7 LOD level more than i need. fuck. dat.
        //Too small numbers... So. Solution is planet radius scaling. 1 unit = 1 milion unity units,
        //then i simply gonna "scale" the overhaul planet.

        //WARNING!!! Magic! Ya, it works...
        if (lodLevel >= 1)
        {
            if (lodLevel == 1)
                temp = LinearInterpolate(temp, Multiply(patchCubeCenter, (15.0 / 7.5)), 0.5); //0.5
            else if (lodLevel == 2)
                temp = LinearInterpolate(temp, Multiply(patchCubeCenter, (15.0 / 11.25)), 0.75); //0.5 + 0.5 / 2.0
            else if (lodLevel == 3)
                temp = LinearInterpolate(temp, Multiply(patchCubeCenter, (15.0 / 13.125)), 0.875); //0.75 + ((0.5 / 2.0) / 2.0)
            else if (lodLevel == 4)
                temp = LinearInterpolate(temp, Multiply(patchCubeCenter, (15.0 / 14.0625)), 0.9375); //0.875 + (((0.5 / 2.0) / 2.0) / 2.0)
            else if (lodLevel == 5)
                temp = LinearInterpolate(temp, Multiply(patchCubeCenter, (15.0 / 14.53125)), 0.96875); //0.9375 + ((((0.5 / 2.0) / 2.0) / 2.0) / 2.0)
            else if (lodLevel == 6)
                temp = LinearInterpolate(temp, Multiply(patchCubeCenter, (15.0 / 14.765625)), 0.984375); //0.96875 + (((((0.5 / 2.0) / 2.0) / 2.0) / 2.0) / 2.0)
            else if (lodLevel == 7) //Experimental!
                temp = LinearInterpolate(temp, Multiply(patchCubeCenter, (15.0 / 14.8828125)), 0.9921875); //0.984375 + ((((((0.5 / 2.0) / 2.0) / 2.0) / 2.0) / 2.0) / 2.0)
            else if (lodLevel == 8) //Experimental!
                temp = LinearInterpolate(temp, Multiply(patchCubeCenter, (15.0 / 14.94140625)), 0.99609375); //0.9921875 + (((((((0.5 / 2.0) / 2.0) / 2.0) / 2.0) / 2.0) / 2.0) / 2.0)
            else if (lodLevel == 9) //Experimental! Maybe float precision have place on small planet radius!
                temp = LinearInterpolate(temp, Multiply(patchCubeCenter, (15.0 / 14.970703125)), 0.998046875); //0.99609375 + ((((((((0.5 / 2.0) / 2.0) / 2.0) / 2.0) / 2.0) / 2.0) / 2.0) / 2.0)
            else if (lodLevel == 10) //Sooooo deep... what i'am doing?
                temp = LinearInterpolate(temp, Multiply(patchCubeCenter, (15.0 / 14.9853515625)), 0.9990234375); //0.998046875 + (((((((((0.5 / 2.0) / 2.0) / 2.0) / 2.0) / 2.0) / 2.0) / 2.0) / 2.0) / 2.0)
            else if (lodLevel == 11) //WHY?!
                temp = LinearInterpolate(temp, Multiply(patchCubeCenter, (15.0 / 14.99267578125)), 0.99951171875); //0.9990234375 + ((((((((((0.5 / 2.0) / 2.0) / 2.0) / 2.0) / 2.0) / 2.0) / 2.0) / 2.0) / 2.0) / 2.0)
            else if (lodLevel == 12) //NOOOOO! STOP IT! STOP THIS!
                temp = LinearInterpolate(temp, Multiply(patchCubeCenter, (15.0 / 14.996337890625)), 0.999755859375); //0.99951171875 + (((((((((((0.5 / 2.0) / 2.0) / 2.0) / 2.0) / 2.0) / 2.0) / 2.0) / 2.0) / 2.0) / 2.0) / 2.0)
            else if (lodLevel == 13) //Only qbit science can save us...
                temp = LinearInterpolate(temp, Multiply(patchCubeCenter, (15.0 / 14.9981689453125)), 0.9998779296875);
            else if (lodLevel == 14)
                temp = LinearInterpolate(temp, Multiply(patchCubeCenter, (15.0 / 14.99908447265625)), 0.99993896484375);
            else if (lodLevel == 15)
                temp = LinearInterpolate(temp, Multiply(patchCubeCenter, (15.0 / 14.999542236328125)), 0.999969482421875);
            else if (lodLevel == 16)
                temp = LinearInterpolate(temp, Multiply(patchCubeCenter, (15.0 / 14.9997711181640625)), 0.9999847412109375);
        }
        //End of magic here.
    }
}