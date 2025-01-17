﻿//
// Copyright (c) Charles Simon. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//  

namespace BrainSimulator.Modules
{
    public class ModuleMove : ModuleBase
    {

        public ModuleMove()
        {
            minHeight = 5;
            minWidth = 3;
        }

        public override void Fire()
        {
            Init();  //be sure to leave this here to enable use of the na variable
            float[] dist = { .5f, .1f, 0, -.025f, -.1f };
            float motionX = 0;
            float motionY = 0;

            if (mv.Width < 3) Initialize();

            for (int i = 0; i < mv.Height; i++)
            {
                if (mv.GetNeuronAt(1, i).LastCharge > 0.9)
                {
                    motionX = dist[i];
                }
            }

            if (mv.GetNeuronAt(1, 2).LastCharge != 0)
            {
                motionX = mv.GetNeuronAt(1, 2).LastCharge;
                mv.GetNeuronAt(1, 2).SetValue(0);
            }

            if (mv.GetNeuronAt(0, 2).LastCharge > 0.9) motionY = 0.5f;
            if (mv.GetNeuronAt(2, 2).LastCharge > 0.9) motionY = -0.5f;


            Module3DSim m3D = (Module3DSim)FindModleu(typeof(Module3DSim));
            if (m3D != null && motionX != 0)
                m3D.Move(motionX);

            bool moved = false;
            Module2DSim m2D = (Module2DSim)FindModleu(typeof(Module2DSim));
            if (m2D != null && (motionX != 0 || motionY != 0))
                moved = m2D.Move(motionX, motionY);

            Module2DModel m2DModel = (Module2DModel)FindModleu(typeof(Module2DModel));
            if (m2DModel != null && moved && motionX != 0 || motionY != 0)
                m2DModel.Move(motionX, motionY);

            Module2DVision m2DVision = (Module2DVision)FindModleu(typeof(Module2DVision));
            if (m2DVision != null && motionX != 0) m2DVision.ViewChanged();

        }

        public override void Initialize()
        {
            ClearNeurons();
            if (mv.Width == 1)
            {
                mv.Width = MinWidth;
                mv.FirstNeuron -= MainWindow.此神经元数组.rows;
            }
            mv.GetNeuronAt(1, 2).模型 = 神经元.模型类型.FloatValue;
            mv.GetNeuronAt(1, 0).标签名 = "^^";
            mv.GetNeuronAt(1, 1).标签名 = "^";
            mv.GetNeuronAt(1, 3).标签名 = "v";
            mv.GetNeuronAt(1, 4).标签名 = "vv";
            mv.GetNeuronAt(0, 2).标签名 = "<";
            mv.GetNeuronAt(2, 2).标签名 = ">";

        }
    }

}
