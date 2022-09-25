#include "pch.h"

#include "��Ԫ�����װ��.h"
#include "..\NeuronEngine\��Ԫ�б�Base.h"
#include <Windows.h>


using namespace System;
using namespace std;
using namespace System::Runtime::InteropServices;


namespace NeuronEngine
{
	namespace CLI
	{
		��Ԫ�б�Base::��Ԫ�б�Base()
		{
		}
		��Ԫ�б�Base::~��Ԫ�б�Base()
		{
			delete theNeuronArray;
		}
		void ��Ԫ�б�Base::Initialize(int neuronCount)
		{
			if (theNeuronArray != NULL)
				delete theNeuronArray;
			theNeuronArray = new NeuronEngine::��Ԫ�б�Base();
			theNeuronArray->Initialize(neuronCount);
		}
		void ��Ԫ�б�Base::Fire()
		{
			theNeuronArray->Fire();
		}
		int ��Ԫ�б�Base::��ȡ�����С()
		{
			return theNeuronArray->��ȡ�����С();
		}
		long long ��Ԫ�б�Base::��ȡ�δ�()
		{
			return theNeuronArray->��ȡ�δ�();
		}
		void ��Ԫ�б�Base::���ôδ�(long long i)
		{
			theNeuronArray->���ôδ�(i);
		}
		int ��Ԫ�б�Base::��ȡ�������Ԫ����()
		{
			return theNeuronArray->��ȡ��������();
		}
		void ��Ԫ�б�Base::�����߳�����(int theCount)
		{
			theNeuronArray->�����߳�����(theCount);
		}
		int ��Ԫ�б�Base::��ȡ�߳���()
		{
			return theNeuronArray->��ȡ�߳�����();
		}
		void ��Ԫ�б�Base::SetRefractoryDelay(int i)
		{
			theNeuronArray->SetRefractoryDelay(i);
		}
		int ��Ԫ�б�Base::GetRefractoryDelay()
		{
			return theNeuronArray->GetRefractoryDelay();
		}
		float ��Ԫ�б�Base::GetNeuronLastCharge(int i)
		{
			��ԪBase* n = theNeuronArray->��ȡ��Ԫ(i);
			return n->GetLastCharge();
		}
		void ��Ԫ�б�Base::SetNeuronLastCharge(int i, float value)
		{
			��ԪBase* n = theNeuronArray->��ȡ��Ԫ(i);
			n->SetLastCharge(value);
		}
		void ��Ԫ�б�Base::SetNeuronCurrentCharge(int i, float value)
		{
			��ԪBase* n = theNeuronArray->��ȡ��Ԫ(i);
			n->SetCurrentCharge(value);
		}
		void ��Ԫ�б�Base::AddToNeuronCurrentCharge(int i, float value)
		{
			��ԪBase* n = theNeuronArray->��ȡ��Ԫ(i);
			n->AddToCurrentValue(value);
		}
		float ��Ԫ�б�Base::GetNeuronLeakRate(int i)
		{
			��ԪBase* n = theNeuronArray->��ȡ��Ԫ(i);
			return n->��ȡй¶��();
		}
		void ��Ԫ�б�Base::SetNeuronLeakRate(int i, float value)
		{
			��ԪBase* n = theNeuronArray->��ȡ��Ԫ(i);
			n->����й¶��(value);
		}
		int ��Ԫ�б�Base::GetNeuronAxonDelay(int i)
		{
			��ԪBase* n = theNeuronArray->��ȡ��Ԫ(i);
			return n->GetAxonDelay();
		}
		void ��Ԫ�б�Base::SetNeuronAxonDelay(int i, int value)
		{
			��ԪBase* n = theNeuronArray->��ȡ��Ԫ(i);
			n->SetAxonDelay(value);
		}
		long long ��Ԫ�б�Base::GetNeuronLastFired(int i)
		{
			��ԪBase* n = theNeuronArray->��ȡ��Ԫ(i);
			return n->GetLastFired();
		}
		int ��Ԫ�б�Base::��ȡ��Ԫģ��(int i)
		{
			��ԪBase* n = theNeuronArray->��ȡ��Ԫ(i);
			return (int)n->��ȡģ��();
		}
		void ��Ԫ�б�Base::������Ԫģ��(int i, int model)
		{
			��ԪBase* n = theNeuronArray->��ȡ��Ԫ(i);
			n->����ģ��((��ԪBase::modelType) model);
		}
		bool ��Ԫ�б�Base::��ȡ��Ԫ�Ƿ�ʹ����(int i)
		{
			��ԪBase* n = theNeuronArray->��ȡ��Ԫ(i);
			return n->GetInUse();
		}
		void ��Ԫ�б�Base::������Ԫ��ǩ(int i, String^ newLabel)
		{
			const wchar_t* chars = (const wchar_t*)(Marshal::StringToHGlobalAuto(newLabel)).ToPointer();
			theNeuronArray->��ȡ��Ԫ(i)->���ñ�ǩ(chars);
			Marshal::FreeHGlobal(IntPtr((void*)chars));
		}
		String^ ��Ԫ�б�Base::��ȡ��Ԫ��ǩ(int i)
		{
			wchar_t* labelChars = theNeuronArray->��ȡ��Ԫ(i)->��ȡ��ǩ();
			if (labelChars != NULL)
			{
				std::wstring label(labelChars);
				String^ str = gcnew String(label.c_str());
				return str;
			}
			else
			{
				String^ str = gcnew String("");
				return str;
			}
		}
		String^ ��Ԫ�б�Base::GetRemoteFiring()
		{
			std::string remoteFiring = theNeuronArray->GetRemoteFiringString();
			String^ str = gcnew String(remoteFiring.c_str());
			return str;
		}
		cli::array<byte>^ ��Ԫ�б�Base::GetRemoteFiringSynapses()
		{
			std::vector<ͻ��Base> tempVec;
			ͻ��Base s1 = theNeuronArray->GetRemoteFiringSynapse();
			while (s1.��ȡĿ����Ԫ() != NULL)
			{
				tempVec.push_back(s1);
				s1 = theNeuronArray->GetRemoteFiringSynapse();
			}
			return ReturnArray(tempVec);
		}

		void ��Ԫ�б�Base::���ͻ��(int src, int dest, float weight, int model, bool noBackPtr)
		{
			if (src < 0)return;
			��ԪBase* n = theNeuronArray->��ȡ��Ԫ(src);
			if (dest < 0)
				n->���ͻ��((��ԪBase*)(long long)dest, weight, (ͻ��Base::modelType) model, noBackPtr);
			else
				n->���ͻ��(theNeuronArray->��ȡ��Ԫ(dest), weight, (ͻ��Base::modelType)model, noBackPtr);
		}
		void ��Ԫ�б�Base::�������ͻ��(int src, int dest, float weight, int model)
		{
			if (dest < 0)return;
			��ԪBase* n = theNeuronArray->��ȡ��Ԫ(dest);
			if (src < 0)
				n->AddSynapseFrom((��ԪBase*)(long long)src, weight, (ͻ��Base::modelType)model);
			else
				n->AddSynapseFrom(theNeuronArray->��ȡ��Ԫ(src), weight, (ͻ��Base::modelType)model);
		}
		void ��Ԫ�б�Base::ɾ��ͻ��(int src, int dest)
		{
			if (src < 0) return;
			��ԪBase* n = theNeuronArray->��ȡ��Ԫ(src);
			if (dest < 0)
				n->ɾ��ͻ��((��ԪBase*)(long long)dest);
			else
				n->ɾ��ͻ��(theNeuronArray->��ȡ��Ԫ(dest));
		}
		void ��Ԫ�б�Base::ɾ������ͻ��(int src, int dest)
		{
			if (dest < 0)return;
			��ԪBase* n = theNeuronArray->��ȡ��Ԫ(dest);
			if (src < 0)
				n->ɾ��ͻ��((��ԪBase*)(long long)src);
			else
				n->ɾ��ͻ��(theNeuronArray->��ȡ��Ԫ(src));
		}


		cli::array<byte>^ ��Ԫ�б�Base::��ȡͻ������(int src)
		{
			��ԪBase* n = theNeuronArray->��ȡ��Ԫ(src);
			n->GetLock();
			std::vector<ͻ��Base> tempVec = n->��ȡͻ������();
			n->ClearLock();
			return ReturnArray(tempVec);

		}
		cli::array<byte>^ ��Ԫ�б�Base::GetSynapsesFrom(int src)
		{
			��ԪBase* n = theNeuronArray->��ȡ��Ԫ(src);
			n->GetLock();
			std::vector<ͻ��Base> tempVec = n->GetSynapsesFrom();
			n->ClearLock();
			return ReturnArray(tempVec);
		}

		cli::array<byte>^ ��Ԫ�б�Base::ReturnArray(std::vector<ͻ��Base> tempVec)
		{
			if (tempVec.size() == 0)
			{
				return gcnew cli::array<byte>(0);
			}
			byte* firstElem = (byte*)&tempVec.at(0);
			const size_t SIZE = tempVec.size(); //#of synapses
			const int byteCount = (int)(SIZE * sizeof(ͻ��));
			cli::array<byte>^ tempArr = gcnew cli::array<byte>(byteCount);
			int k = 0;
			for (int j = 0; j < tempVec.size(); j++)
			{
				ͻ�� s;
				s.model = (int)tempVec.at(j).��ȡģ��();
				s.weight = tempVec.at(j).��ȡȨ��();
				//if the top bit of the target is not set, it's a raw pointer
				//if it is set, this is the negative of a global neuron ID
				��ԪBase* target = tempVec.at(j).��ȡĿ����Ԫ();
				if (((long long)target >> 63) != 0 || target == NULL)
					s.target = (int)(long long)(tempVec.at(j).��ȡĿ����Ԫ());
				else
					s.target = tempVec.at(j).��ȡĿ����Ԫ()->��ȡID();
				byte* firstElem = (byte*)&s;
				for (int i = 0; i < sizeof(ͻ��); i++)
				{
					tempArr[k++] = *(firstElem + i);
				}
			}
			return tempArr;
		}

		struct Neuron {
			int id;  bool inUse; float lastCharge; float currentCharge;
			float leakRate; int axonDelay; ��ԪBase::modelType model; long long lastFired;
		};
		cli::array<byte>^ ��Ԫ�б�Base::��ȡ��Ԫ(int src)
		{
			��ԪBase* n = theNeuronArray->��ȡ��Ԫ(src);
			const int byteCount = sizeof(Neuron);
			cli::array<byte>^ tempArr = gcnew cli::array<byte>(byteCount);
			Neuron n1;
			memset(&n1, 0, byteCount); //clear out the space between struct elements
			n1.id = n->��ȡID();
			n1.inUse = n->GetInUse();
			n1.lastCharge = n->GetLastCharge();
			n1.currentCharge = n->GetCurrentCharge();
			n1.leakRate = n->��ȡй¶��();
			n1.lastFired = n->GetLastFired();
			n1.model = n->��ȡģ��();
			n1.axonDelay = n->GetAxonDelay();
			byte* firstElem = (byte*)&n1;
			for (int i = 0; i < sizeof(Neuron); i++)
			{
				tempArr[i] = *(firstElem + i);
			}
			return tempArr;
		}
		long long ��Ԫ�б�Base::��ȡ��ͻ����()
		{
			return theNeuronArray->GetTotalSynapseCount();
		}
		long ��Ԫ�б�Base::��ȡʹ���е���Ԫ����()
		{
			return theNeuronArray->��ȡʹ������Ԫ����();
		}
	}
}