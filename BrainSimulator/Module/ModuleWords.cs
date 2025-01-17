﻿//
// Copyright (c) Charles Simon. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//  

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace BrainSimulator.Modules
{
    public class ModuleWords : ModuleBase
    {
        //any public variable you create here will automatically be stored with the network
        //unless you precede it with the [XmlIgnore] directive
        //[XlmIgnore] 
        //public theStatus = 1;

        //fill this method in with code which will execute
        //once for each cycle of the engine
        public override void Fire()
        {
            Init();  //be sure to leave this here
            List<int> neuronsWhichFired = GetNeuronsWhichFired();

        }
        
        public ModuleWords()
        {
            minHeight = 10;
           minWidth = 20;
        }
        List<int> GetNeuronsWhichFired()
        {
            List<int> retVal = new List<int>();
            foreach(神经元 n in mv.Neurons)
            {
                if (n.最后更改 >= 1)
                    retVal.Add(n.Id);
            }
            return retVal;
        }

        int nextFreeNeuron = 1;

        Dictionary<string, int> wordDictionary = new Dictionary<string, int>();

        private void AddSynapse(string word, string nextWord)
        {
            word = TrimPunctuation(word);
            nextWord = TrimPunctuation(nextWord);
            wordDictionary.TryGetValue(word, out int wordNeuronID);
            if (wordNeuronID == 0) // if there is no neuron for the word, add one
            {
                wordDictionary.Add(word, nextFreeNeuron);
                wordNeuronID = nextFreeNeuron++;
                if (mv.GetNeuronAt(wordNeuronID) is 神经元 n1)
                    n1.标签名 = word; //useful someday
            }
            wordDictionary.TryGetValue(nextWord, out int nextWordNeuronID);
            if (nextWordNeuronID == 0) // if there is no neuron for the word, add one
            {
                wordDictionary.Add(nextWord, nextFreeNeuron);
                nextWordNeuronID = nextFreeNeuron++;
                if (mv.GetNeuronAt(nextWordNeuronID) is 神经元 n1)
                    n1.标签名 = nextWord; //useful someday
            }
            神经元 n = mv.GetNeuronAt(wordNeuronID);
            神经元 nNext = mv.GetNeuronAt(nextWordNeuronID);
            if (n != null && nNext != null)
            {
                突触 s = n.查找突触(nNext.id);
                float weight = .1f;
                if (s != null)
                    weight += .1f;
                n.添加突触(nNext.id, weight);
            }
        }

        /// <summary>
        /// TrimPunctuation from start and end of string.
        /// </summary>
        static string TrimPunctuation(string value)
        {
            // Count start punctuation.
            int removeFromStart = 0;
            for (int i = 0; i < value.Length; i++)
            {
                if (char.IsPunctuation(value[i]))
                {
                    removeFromStart++;
                }
                else
                {
                    break;
                }
            }

            // Count end punctuation.
            int removeFromEnd = 0;
            for (int i = value.Length - 1; i >= 0; i--)
            {
                if (char.IsPunctuation(value[i]))
                {
                    removeFromEnd++;
                }
                else
                {
                    break;
                }
            }
            // No characters were punctuation.
            if (removeFromStart == 0 &&
                removeFromEnd == 0)
            {
                return value;
            }
            // All characters were punctuation.
            if (removeFromStart == value.Length &&
                removeFromEnd == value.Length)
            {
                return "";
            }
            // Substring.
            return value.Substring(removeFromStart,
                value.Length - removeFromEnd - removeFromStart);
        }

        //fill this method in with code which will execute once
        //when the module is added, when "initialize" is selected from the context menu,
        //or when the engine restart button is pressed
        public override void Initialize()
        {
            string textFileName = "";
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                Filter = "TXT Command Files|*.txt",
                Title = "Select a Brain Simulator Command File"
            };
            // Show the Dialog.  
            // If the user clicked OK in the dialog  
            DialogResult result = DialogResult.Cancel;
            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate
            {
                result = openFileDialog1.ShowDialog();
            });
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                textFileName = openFileDialog1.FileName;
                nextFreeNeuron = 1;
                ClearNeurons();
                wordDictionary.Clear();

                if (File.Exists(textFileName))
                {
                    string[] commands = File.ReadAllLines(textFileName);
                    //for each line in the file
                    foreach (string s in commands)
                    {
                        //TODO strip out any punctuation
                        string[] s2 = s.Split(' ');
                        string word = "";
                        foreach (string nextWord in s2)
                        {
                            if (word != "")
                                AddSynapse(word, nextWord.ToLower());
                            word = nextWord.ToLower();
                        }
                    }
                }
            }
        }
    }
}
