﻿//
// Copyright (c) Charles Simon. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//  

using System;
using System.Collections.Generic;
using static BrainSimulator.跨语言接口;
using static System.Math;

namespace BrainSimulator.Modules
{
    public class ModuleBehavior : ModuleBase
    {

        public ModuleBehavior()
        {
            minWidth = 11;
            minHeight = 1;
            maxWidth = 11;
            maxHeight = 1;
        }

        public enum TheBehavior { Rest, Move, Turn };
        public class behavior
        {
            public TheBehavior theBehavior;
            public float param1;
        }

        List<behavior> pending = new List<behavior>();

        public bool IsIdle()
        {
            return (pending.Count == 0);
        }

        public override void Fire()
        {
            Init();  //be sure to leave this here to enable use of the na variable
            try
            {
                if (GetNeuronValue(null, "Stop") == 1) Stop();
                if (GetNeuronValue(null, "TurnTo") == 1) TurnTo();
                if (GetNeuronValue(null, "MoveTo") == 1) MoveTo();
                if (GetNeuronValue(null, "Scan") == 1) Scan();
                if (GetNeuronValue(null, "Coll") == 1) Collision();
            }
            catch { return; }

            if (pending.Count == 0)
            {
                SetNeuronValue(null, "Done", 1);
            }


            if (pending.Count != 0)
            {
                behavior currentBehavior = pending[0];
                pending.RemoveAt(0);
                switch (currentBehavior.theBehavior)
                {
                    case TheBehavior.Rest: break;
                    case TheBehavior.Move:
                        SetNeuronValue("ModuleMove", 1, 2, currentBehavior.param1);
                        break;
                    case TheBehavior.Turn:
                        SetNeuronValue("ModuleTurn", 2, 0, currentBehavior.param1);
                        break;
                }
            }

        }
        public override void Initialize()
        {
            pending.Clear();
            mv.GetNeuronAt(0, 0).标签名 = "Stop";
            mv.GetNeuronAt(1, 0).标签名 = "Done";
            mv.GetNeuronAt(2, 0).标签名 = "TurnTo";
            mv.GetNeuronAt(3, 0).模型 = 神经元.模型类型.FloatValue;
            mv.GetNeuronAt(3, 0).标签名 = "Theta";
            mv.GetNeuronAt(4, 0).标签名 = "MoveTo";
            mv.GetNeuronAt(5, 0).模型 = 神经元.模型类型.FloatValue;
            mv.GetNeuronAt(5, 0).标签名 = "R";
            mv.GetNeuronAt(6, 0).标签名 = "Scan";
            mv.GetNeuronAt(9, 0).标签名 = "Coll";
            mv.GetNeuronAt(10, 0).标签名 = "CollAngle";
            mv.GetNeuronAt(10, 0).模型 = 神经元.模型类型.FloatValue;

            //Connect Neurons to the UKS
            神经元 nUKSDone = GetNeuron("Module2DUKS", "Done");
            if (nUKSDone != null)
                mv.GetNeuronAt("Done").添加突触(nUKSDone.Id, 1);
            神经元 nUKSStop = GetNeuron("UKSOut", "Stop");
            if (nUKSStop != null)
                nUKSStop.添加突触(mv.GetNeuronAt("Stop").Id, 1);

        }

        public override void ShowDialog() //delete this function if it isn't needed
        {
            base.ShowDialog();
        }

        //Several Behaviors...

        public void Stop()
        {
            pending.Clear();
            SetNeuronValue(null, "Done", 1);
        }

        //Random (not currently used)
        public void RandomBehavior()
        {
            //lowest priority...only do this if nothing else is pending
            if (pending.Count > 0) return;
            double x = new Random().NextDouble();

            behavior newBehavoir = new behavior()
            {
                theBehavior = TheBehavior.Turn
            };
            newBehavoir.param1 = -(float)PI / 6;
            if (x < .925) newBehavoir.param1 = -(float)PI / 12;
            else if (x < .95) newBehavoir.param1 = -(float)PI / 24;
            else if (x < .975) newBehavoir.param1 = (float)PI / 24;
            else if (x < 1) newBehavoir.param1 = (float)PI / 12;

            pending.Add(newBehavoir);
        }

        public void Scan()
        {
            SetNeuronValue(null, "Done", 0);

            TurnTo((float)PI / 2);
            TurnTo((float)PI / -2);
            TurnTo((float)PI / -2);
            TurnTo((float)PI / 2);
        }

        public bool IsMoving()
        {
            return pending.Count > 0 && pending[0].theBehavior == TheBehavior.Move;
        }

        private void Collision()
        {
            //SetNeuronValue(null, "Done", 0);
            //pending.Clear();
            //float collisionAngle = na.GetNeuronAt("CollAngle").CurrentCharge;
            //TurnTo(-collisionAngle - (float)PI / 2);
            //MoveTo(.2f);
            //TurnTo(+collisionAngle + (float)PI / 2);
        }

        //TurnTo
        public void TurnTo()
        {
            if (pending.Count > 0) return;
            float theta = GetNeuronValue(null, "Theta");
            if (theta == 0) return;
            TurnTo(theta);
        }

        public void TurnTo(Angle theta)
        {
            float x = theta % Rad(90);
            //if (Abs(x) > Rad(1)) //if correction is more than a degree...break
            //{
            //    x = x;
            //}

            SetNeuronValue(null, "Done", 0);

            //don't bother turing more than 180-degrees, turn the other way
            while (theta > PI) theta -= (float)PI * 2;
            while (theta < -PI) theta += (float)PI * 2;
            float deltaTheta = (float)PI / 6;

            while (Abs(theta) > 0.001)
            {
                float theta1 = 0;
                if (theta > 0)
                {
                    if (theta > deltaTheta) theta1 = deltaTheta;
                    else theta1 = theta;
                    theta = theta - theta1;
                }
                else
                {
                    if (theta < -deltaTheta) theta1 = -deltaTheta;
                    else theta1 = theta;
                    theta = theta - theta1;
                }
                behavior newBehavior = new behavior()
                {
                    theBehavior = TheBehavior.Turn,
                    param1 = theta1
                };
                pending.Add(newBehavior);
            }
        }

        //MoveTo
        private void MoveTo()
        {
            float dist = mv.GetNeuronAt("R").CurrentCharge;
            SetNeuronValue(null, "MoveTo", 0);
            if (dist <= 0) return;
            MoveTo(dist);
        }
        public void MoveTo(float dist)
        {
            SetNeuronValue(null, "Done", 0);

            while (Abs(dist) > 0.001)
            {
                behavior newBehavior = new behavior() { theBehavior = TheBehavior.Rest };
                pending.Add(newBehavior);
                pending.Add(newBehavior);
                float dist1 = 0;
                if (dist > .2f) dist1 = .2f;
                else dist1 = dist;
                dist = dist - dist1;
                newBehavior = new behavior()
                {
                    theBehavior = TheBehavior.Move,
                    param1 = dist1
                };
                pending.Add(newBehavior);
            }
        }
    }
}
