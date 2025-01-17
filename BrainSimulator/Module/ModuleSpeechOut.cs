﻿//
// Copyright (c) Charles Simon. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//  

using System.Speech.Synthesis;

namespace BrainSimulator.Modules
{
    public class ModuleSpeechOut : ModuleBase
    {
        SpeechSynthesizer synth = null;

        string toSpeak = ""; //accumulates an array of words to speak
        string prePend = "";
        string postPend = "";
        string insertAnd = "";

        int anyNewWords = 0;

        public ModuleSpeechOut()
        {
            minHeight = 4;
            minWidth = 3;
        }

        public override void Fire()
        {
            Init();
            if (synth == null) Initialize();
            foreach (神经元 n in mv.Neurons)
            {
                if (n.LastCharge > .9)
                {
                    if (n.标签名 == "")
                    {
                        模块视图 msi = theNeuronArray.FindModuleByLabel("ModuleSpeechIn");
                        int i = mv.GetNeuronOffset(n);
                        if (msi != null)
                            mv.GetNeuronAt(i).标签名 = msi.GetNeuronAt(i).标签名;
                    }
                    toSpeak += prePend + n.标签名 + " ";
                    prePend = "";
                    anyNewWords = 5;
                }
            }
            //builds up a phrase to be spoken, and passes it all at once to the synthisizer
            //this is much better than individual words
            anyNewWords--;
            if (anyNewWords == 0)
                if (toSpeak != "")
                {
                    toSpeak = toSpeak.Trim();
                    if (toSpeak.IndexOf(' ') != -1)
                        toSpeak = toSpeak.Insert(toSpeak.Trim().LastIndexOf(' '), " " + insertAnd);
                    toSpeak = toSpeak + " " + postPend;
                    ModuleSpeechIn msi = (ModuleSpeechIn)FindModleu(typeof(ModuleSpeechIn));
                    if (msi != null)
                        msi.PauseRecognition(); //if there is a recognizer active
                    synth.SpeakAsync(toSpeak + ".");

                    //use this when we want to work with phonetics instead of words
                    //string[] words = toSpeak.Split(' ');
                    //PromptBuilder pb = new PromptBuilder();
                    //foreach (string p in words)
                    //    if (p != "")
                    //        pb.AppendTextWithPronunciation("NotUsed", p);
                    //pb.AppendText("."); //this improves the prosidy
                    //synth.SpeakAsync(pb);

                    toSpeak = "";
                    prePend = "";
                    postPend = "";
                    insertAnd = "";
                }
        }

        public override void Initialize()
        {
            synth = new SpeechSynthesizer();

            // Configure the audio output.
            synth.SetOutputToDefaultAudioDevice();
            synth.SpeakCompleted += Synth_SpeakCompleted;
            synth.SelectVoice("Microsoft Zira Desktop");

            //temporarily assign output vocabulary to be identical to input vocabulary
            ClearNeurons();

            模块视图 msi = theNeuronArray.FindModuleByLabel("ModuleSpeechIn");
            if (msi != null)
            {
                for (int i = 0; i < mv.NeuronCount && i < msi.NeuronCount; i++)
                {
                    mv.GetNeuronAt(i).标签名 = msi.GetNeuronAt(i).标签名;
                    msi.GetNeuronAt(i).添加突触(mv.GetNeuronAt(i).Id, 1);
                }
            }
        }


        private void Synth_SpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
            // Restart speech recognition.  
            ModuleSpeechIn msi = (ModuleSpeechIn)FindModleu(typeof(ModuleSpeechIn));
            if (msi != null)
                msi.ResumeRecognition();
        }

    }
}
