using Godot;
using System;
using System.Collections;


namespace StateMachine
{
	public struct State
	{
		public int ID;
		public string name;
		
		// Un ID, un nom
		public State(int id, string name)
		{
			ID = id;
			this.name = name;
		}
		
		public string GetName() {
			return name;
		}
		
		public int GetId() {
			return ID;
		}
	}

	public struct FSM
	{
		State[] states;
		State currentState;
		
		// Constructor
		// Une structure (type par valeur, optimisé)
		// On passe un tableau de Strings : la liste des states
		public FSM(String[] statesInfo)
		{
			// taille définie par le nombre de strings
			int size = statesInfo.Length;
			
			// On instancie plusieurs states
			states = new State[size];
			
			// La state actuelle possède l'ID : -1
			currentState = new State(-1, "Test");
						
			// Pour chaque states on faire RegisterState
			// On passe en argument la String à l'index indiqué
			for (int i = 0; i < size; i++)
			{
				RegisterState(statesInfo[i], i);
			}
			ChangeState(statesInfo[0], true);
		}


		// Le cas first = true correspond à ChangeState à l'initialisation
		public void ChangeState(string stateName, bool first = false)
		{
			foreach (State st in states)
			{
				if (st.name.Equals(stateName))
				{
					currentState = st;
					break;
				}
				else {
					//GD.Print("State not found");
				}
			}
		}

		// RegisterState va creer une nouvelle state et l'ajouter au tableau
		private void RegisterState(string name, int id)
		{
			states[id].name = name;
			states[id].ID = id;
		}

		// retourne ne nom de la State
		public string GetCurrentStateName()
		{
			return currentState.name;
		}


		public void Update()
		{
			// Mettre à jour la FSM
		}
	}

}


