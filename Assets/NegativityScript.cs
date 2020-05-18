using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Random = UnityEngine.Random;

public class NegativityScript : MonoBehaviour
{
	public KMAudio Audio;
	public KMBombInfo Bomb;
	public KMBombModule Module;

	public AudioClip[] SFX;
	public KMSelectable[] Buttons;
	public Renderer[] ButtonRenderer;
	public Material[] BlackAndWhite;
	public TextMesh NumberLine;
	public TextMesh Ternary;
	public TextMesh[] SAndC;
	
	public SpriteRenderer Star;
	public Sprite[] Stars;

	private int[] Numbering = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
	private int[] NumberingConverted = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0};

	private int[][] TernaryFunctions = new int[2][]{
		new int[9] {0, 0, 0, 0, 0, 0, 0, 0, 0},
		new int[9] {6561, 2187, 729, 243, 81, 27, 9, 3, 1}
	};
	
	bool Playable = false;
	bool Silent = false;

	private int Totale = 0;
	string Tables;
	
	int RotationsNumber = 0;
	private int[] Status = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
	private int KSop = 0;
	private int Switcher = 0;

	//Logging
	static int moduleIdCounter = 1;
	int moduleId;
	private bool ModuleSolved;

	void Awake()
	{
		moduleId = moduleIdCounter++;
		for (int i = 0; i < 3; i++)
		{
			int index = i;
			Buttons[index].OnInteract += delegate
			{
				PressButton(index);
				return false;
			};
		}
	}

	void Start()
	{
		Module.OnActivate += NumberSheet;
	}

	void NumberSheet()
	{
		for (int a = 0; a < 10; a++)
		{
			Status[a] = Random.Range(0,2);
			Numbering[a] = Random.Range(100,1000);
			if (Random.Range(0,2) != 0) Numbering[a] = -Numbering[a];
			if (Status[a] == 0) NumberingConverted[a] = -Numbering[a]; else NumberingConverted[a] = Numbering[a];
			Totale += NumberingConverted[a];
			Debug.LogFormat("[Negativity #{2}] Original Number : {0}, Converted Number: {1}", Numbering[a].ToString(), NumberingConverted[a].ToString(), moduleId);
		}
		Debug.LogFormat("[Negativity #{0}] The total value: {1}", moduleId, Totale.ToString());

		if (Totale > 0)
		{
			for (int q = 0; q < 9; q++)
			{
				// Convert the number to ternary.
				for (int r = 0; r < 2; r++)
				{
					if (Totale >= TernaryFunctions[1][q])
					{
						Totale -= TernaryFunctions[1][q];
						++TernaryFunctions[0][q];
					}
				}

				// Replace 2s with 3 - 1.
				for (int z = 0; z < 3; z++)
				{
					for (int s = 0; s < q+1; s++)
					{
						if (TernaryFunctions[0][s] > 1)
						{
							TernaryFunctions[0][s] -= 3;
							++TernaryFunctions[0][s-1];
						}
					}
				}
			}
		}
		
		if (Totale < 0)
		{
			for (int q = 0; q < 9; q++)
			{
				// Convert the number to ternary.
				for (int r = 0; r < 2; r++)
				{
					if (Totale <= -TernaryFunctions[1][q])
					{
						Totale -= -TernaryFunctions[1][q];
						--TernaryFunctions[0][q];
					}
				}

				// Replace -2s with -3 + 1.
				for (int z = 0; z < 3; z++)
				{
					for (int s = 0; s < q+1; s++)
					{
						if (TernaryFunctions[0][s] < -1)
						{
							TernaryFunctions[0][s] += 3;
							--TernaryFunctions[0][s-1];
						}
					}
				}
			}
		}

		var Builder = new StringBuilder();
		// Using StringBuilder when concatenating many strings together is more efficient,
		// because it avoids copying and creating many string instances.
		for (int z = 0; z < 9; z++)
		{
			if (TernaryFunctions[0][z] == 1)
			{
				Builder.Append('+');
			}
			else if (TernaryFunctions[0][z] == -1)
			{
				Builder.Append('-');
			}
			else if (TernaryFunctions[0][z] != 0)
			{
				Debug.Log("The converter is broken");
				break;
			}
		}
		Tables = Builder.ToString();

		Debug.LogFormat("[Negativity #{0}] The answer is: {1}", moduleId, Tables);
		StartCoroutine(Rotations());
		Playable = true;
	}

	void PressButton(int index)
	{
		if (Playable)
		{
			Buttons[index].AddInteractionPunch(.2f);
			if (index == 0)
			{
				if (KSop == 0)
				{
					if (Silent == true) Silent = false; else Silent = true;
				}
				
				else if (KSop == 1)
				{
					if (Ternary.text.Length < 9)
					{
						Audio.PlaySoundAtTransform(SFX[1].name, transform);
						Ternary.text += Switcher == 0 ? "-" : "+";
					}
				}
			}

			if (index == 1)
			{
				if (KSop == 0)
				{
					Audio.PlaySoundAtTransform(SFX[0].name, transform);
					StopAllCoroutines();
					StartCoroutine(Flashes());
					Star.sprite = null;
					KSop = 1;
				}
				
				else if (KSop == 1)
				{
					StopAllCoroutines();
					Debug.LogFormat("[Negativity #{0}] The submitted balance: {1}", moduleId, Ternary.text);
					StartCoroutine(MusicPlay());
				}
			}

			if (index == 2)
			{
				if (KSop == 1 && Ternary.text.Length > 0)
				{
					Audio.PlaySoundAtTransform(SFX[0].name, transform);
					StartCoroutine(Clearer());
				}
				
				else if (KSop == 1 && Ternary.text.Length == 0)
				{
					StopAllCoroutines();
					KSop = 0;
					RotationsNumber = (RotationsNumber - 1 + 10) % 10;
					StartCoroutine(Rotations());
				}
			}
		}
	}

	IEnumerator Rotations()
	{
		while (true)
		{
			for (int b = RotationsNumber; b < 10; b++)
			{
				if (Silent == false)
				{
					Audio.PlaySoundAtTransform(SFX[1].name, transform);
				}
				
				RotationsNumber = (RotationsNumber + 1) % 10;
				if (b == 0)
				{
					if (Status[b] == 1)
					{
						Star.sprite = Stars[0];
					}
					
					else
					{
						Star.sprite = Stars[1];
					}
				}
				
				else
				{
					Star.sprite = null;
				}
				
				if (Status[b] == 0)
				{
					ButtonRenderer[0].material = BlackAndWhite[0];
					ButtonRenderer[1].material = BlackAndWhite[1];
					NumberLine.color = Color.white;
					NumberLine.text = Numbering[b].ToString();
					yield return new WaitForSecondsRealtime(1f);
				}

				else
				{
					ButtonRenderer[0].material = BlackAndWhite[1];
					ButtonRenderer[1].material = BlackAndWhite[0];
					NumberLine.color = Color.black;
					NumberLine.text = Numbering[b].ToString();
					yield return new WaitForSecondsRealtime(1f);
				}
			}
		}
	}

	IEnumerator Flashes()
	{
		while (true)
		{
			NumberLine.text = "";
			for (int c = 0; c < 2; c++)
			{
				if (c == 0)
				{
					ButtonRenderer[0].material = BlackAndWhite[0];
					ButtonRenderer[1].material = BlackAndWhite[1];
					Switcher = 0;
				}

				else
				{
					ButtonRenderer[0].material = BlackAndWhite[1];
					ButtonRenderer[1].material = BlackAndWhite[0];
					Switcher = 1;
				}

				yield return new WaitForSecondsRealtime(0.8f);
			}
		}
	}

	IEnumerator Clearer()
	{
		string Copper = Ternary.text;
		int Heal = Ternary.text.Length;
		for (int g = 0; g < Heal; g++)
		{
			Copper = Copper.Remove(Copper.Length - 1);
			Ternary.text = Copper;
			yield return new WaitForSecondsRealtime(0.05f);
		}
	}
	
	IEnumerator MusicPlay()
	{
		Playable = false;
		Debug.LogFormat("[Negativity #{0}] The balanced was disturbed. You are being judged", moduleId);
		string Answer = Ternary.text;
		Ternary.text = "";
		Audio.PlaySoundAtTransform(SFX[3].name, transform);
		
		ButtonRenderer[0].material = BlackAndWhite[0];
		ButtonRenderer[1].material = BlackAndWhite[1];
		NumberLine.color = Color.white;
		NumberLine.text = "P";
		SAndC[0].text = "+"; SAndC[1].text = "+";
		yield return new WaitForSecondsRealtime(0.2f);
		
		ButtonRenderer[0].material = BlackAndWhite[1];
		ButtonRenderer[1].material = BlackAndWhite[0];
		NumberLine.color = Color.black;
		NumberLine.text = "N";
		SAndC[0].text = "-"; SAndC[1].text = "-";
		yield return new WaitForSecondsRealtime(0.2f);
		
		ButtonRenderer[0].material = BlackAndWhite[0];
		ButtonRenderer[1].material = BlackAndWhite[1];
		NumberLine.color = Color.white;
		NumberLine.text = "Po";
		SAndC[0].text = "+"; SAndC[1].text = "+";
		yield return new WaitForSecondsRealtime(0.2f);
		
		ButtonRenderer[0].material = BlackAndWhite[1];
		ButtonRenderer[1].material = BlackAndWhite[0];
		NumberLine.color = Color.black;
		NumberLine.text = "Ne";
		SAndC[0].text = "-"; SAndC[1].text = "-";
		yield return new WaitForSecondsRealtime(0.2f);
		
		ButtonRenderer[0].material = BlackAndWhite[0];
		ButtonRenderer[1].material = BlackAndWhite[1];
		NumberLine.color = Color.white;
		NumberLine.text = "Pos";
		SAndC[0].text = "+"; SAndC[1].text = "+";
		yield return new WaitForSecondsRealtime(0.16f);
		
		ButtonRenderer[0].material = BlackAndWhite[1];
		ButtonRenderer[1].material = BlackAndWhite[0];
		NumberLine.color = Color.black;
		NumberLine.text = "Neg";
		SAndC[0].text = "-"; SAndC[1].text = "-";
		yield return new WaitForSecondsRealtime(0.16f);
		
		ButtonRenderer[0].material = BlackAndWhite[0];
		ButtonRenderer[1].material = BlackAndWhite[1];
		NumberLine.color = Color.white;
		NumberLine.text = "Posi";
		SAndC[0].text = "+"; SAndC[1].text = "+";
		yield return new WaitForSecondsRealtime(0.16f);
		
		ButtonRenderer[0].material = BlackAndWhite[1];
		ButtonRenderer[1].material = BlackAndWhite[0];
		NumberLine.color = Color.black;
		NumberLine.text = "Nega";
		SAndC[0].text = "-"; SAndC[1].text = "-";
		yield return new WaitForSecondsRealtime(0.16f);
		
		ButtonRenderer[0].material = BlackAndWhite[0];
		ButtonRenderer[1].material = BlackAndWhite[1];
		NumberLine.color = Color.white;
		NumberLine.text = "Posit";
		SAndC[0].text = "+"; SAndC[1].text = "+";
		yield return new WaitForSecondsRealtime(0.128f);
		
		ButtonRenderer[0].material = BlackAndWhite[1];
		ButtonRenderer[1].material = BlackAndWhite[0];
		NumberLine.color = Color.black;
		NumberLine.text = "Negat";
		SAndC[0].text = "-"; SAndC[1].text = "-";
		yield return new WaitForSecondsRealtime(0.128f);
		
		ButtonRenderer[0].material = BlackAndWhite[0];
		ButtonRenderer[1].material = BlackAndWhite[1];
		NumberLine.color = Color.white;
		NumberLine.text = "Positi";
		SAndC[0].text = "+"; SAndC[1].text = "+";
		yield return new WaitForSecondsRealtime(0.128f);
		
		ButtonRenderer[0].material = BlackAndWhite[1];
		ButtonRenderer[1].material = BlackAndWhite[0];
		NumberLine.color = Color.black;
		NumberLine.text = "Negati";
		SAndC[0].text = "-"; SAndC[1].text = "-";
		yield return new WaitForSecondsRealtime(0.128f);
		
		ButtonRenderer[0].material = BlackAndWhite[0];
		ButtonRenderer[1].material = BlackAndWhite[1];
		NumberLine.color = Color.white;
		NumberLine.text = "Positiv";
		SAndC[0].text = "+"; SAndC[1].text = "+";
		yield return new WaitForSecondsRealtime(0.1024f);
		
		ButtonRenderer[0].material = BlackAndWhite[1];
		ButtonRenderer[1].material = BlackAndWhite[0];
		NumberLine.color = Color.black;
		NumberLine.text = "Negativ";
		SAndC[0].text = "-"; SAndC[1].text = "-";
		yield return new WaitForSecondsRealtime(0.1024f);
		
		ButtonRenderer[0].material = BlackAndWhite[0];
		ButtonRenderer[1].material = BlackAndWhite[1];
		NumberLine.color = Color.white;
		NumberLine.text = "Positivi";
		SAndC[0].text = "+"; SAndC[1].text = "+";
		yield return new WaitForSecondsRealtime(0.1024f);
		
		ButtonRenderer[0].material = BlackAndWhite[1];
		ButtonRenderer[1].material = BlackAndWhite[0];
		NumberLine.color = Color.black;
		NumberLine.text = "Negativi";
		SAndC[0].text = "-"; SAndC[1].text = "-";
		yield return new WaitForSecondsRealtime(0.1024f);
		
		ButtonRenderer[0].material = BlackAndWhite[0];
		ButtonRenderer[1].material = BlackAndWhite[1];
		NumberLine.color = Color.white;
		NumberLine.text = "Positivit";
		SAndC[0].text = "+"; SAndC[1].text = "+";
		yield return new WaitForSecondsRealtime(0.08192f);
		
		ButtonRenderer[0].material = BlackAndWhite[1];
		ButtonRenderer[1].material = BlackAndWhite[0];
		NumberLine.color = Color.black;
		NumberLine.text = "Negativit";
		SAndC[0].text = "-"; SAndC[1].text = "-";
		yield return new WaitForSecondsRealtime(0.08192f);
		
		ButtonRenderer[0].material = BlackAndWhite[0];
		ButtonRenderer[1].material = BlackAndWhite[1];
		NumberLine.color = Color.white;
		NumberLine.text = "Positivity";
		SAndC[0].text = "+"; SAndC[1].text = "+";
		yield return new WaitForSecondsRealtime(0.08192f);
		
		ButtonRenderer[0].material = BlackAndWhite[1];
		ButtonRenderer[1].material = BlackAndWhite[0];
		NumberLine.color = Color.black;
		NumberLine.text = "Negativity";
		SAndC[0].text = "-"; SAndC[1].text = "-";
		yield return new WaitForSecondsRealtime(0.08192f);
		
		ButtonRenderer[0].material = BlackAndWhite[0];
		ButtonRenderer[1].material = BlackAndWhite[1];
		NumberLine.color = Color.white;
		NumberLine.text = "The";
		SAndC[0].text = "O"; SAndC[1].text = "X";
		yield return new WaitForSecondsRealtime(0.065536f);
		
		ButtonRenderer[0].material = BlackAndWhite[1];
		ButtonRenderer[1].material = BlackAndWhite[0];
		NumberLine.color = Color.black;
		NumberLine.text = "Balance";
		SAndC[0].text = "X"; SAndC[1].text = "O";
		yield return new WaitForSecondsRealtime(0.065536f);
		
		ButtonRenderer[0].material = BlackAndWhite[0];
		ButtonRenderer[1].material = BlackAndWhite[1];
		NumberLine.color = Color.white;
		NumberLine.text = "Is";
		SAndC[0].text = "O"; SAndC[1].text = "X";
		yield return new WaitForSecondsRealtime(0.065536f);
		
		ButtonRenderer[0].material = BlackAndWhite[1];
		ButtonRenderer[1].material = BlackAndWhite[0];
		NumberLine.color = Color.black;
		NumberLine.text = "Disturbed";
		SAndC[0].text = "X"; SAndC[1].text = "O";
		yield return new WaitForSecondsRealtime(0.065536f);
		
		ButtonRenderer[0].material = BlackAndWhite[0];
		ButtonRenderer[1].material = BlackAndWhite[1];
		NumberLine.color = Color.white;
		NumberLine.text = "I";
		SAndC[0].text = "O"; SAndC[1].text = "X";
		yield return new WaitForSecondsRealtime(0.065536f);
		
		ButtonRenderer[0].material = BlackAndWhite[1];
		ButtonRenderer[1].material = BlackAndWhite[0];
		NumberLine.color = Color.black;
		NumberLine.text = "Will";
		SAndC[0].text = "X"; SAndC[1].text = "O";
		yield return new WaitForSecondsRealtime(0.065536f);
		
		ButtonRenderer[0].material = BlackAndWhite[0];
		ButtonRenderer[1].material = BlackAndWhite[1];
		NumberLine.color = Color.white;
		NumberLine.text = "Now";
		SAndC[0].text = "O"; SAndC[1].text = "X";
		yield return new WaitForSecondsRealtime(0.065536f);
		
		ButtonRenderer[0].material = BlackAndWhite[1];
		ButtonRenderer[1].material = BlackAndWhite[0];
		NumberLine.color = Color.black;
		NumberLine.text = "Decide";
		SAndC[0].text = "X"; SAndC[1].text = "O";
		yield return new WaitForSecondsRealtime(0.065536f);
		
		ButtonRenderer[0].material = BlackAndWhite[0];
		ButtonRenderer[1].material = BlackAndWhite[1];
		NumberLine.color = Color.white;
		NumberLine.text = "I";
		SAndC[0].text = "O"; SAndC[1].text = "X";
		yield return new WaitForSecondsRealtime(0.0524288f);
		
		ButtonRenderer[0].material = BlackAndWhite[1];
		ButtonRenderer[1].material = BlackAndWhite[0];
		NumberLine.color = Color.black;
		NumberLine.text = "Have";
		SAndC[0].text = "X"; SAndC[1].text = "O";
		yield return new WaitForSecondsRealtime(0.0524288f);
		
		ButtonRenderer[0].material = BlackAndWhite[0];
		ButtonRenderer[1].material = BlackAndWhite[1];
		NumberLine.color = Color.white;
		NumberLine.text = "Now";
		SAndC[0].text = "O"; SAndC[1].text = "X";
		yield return new WaitForSecondsRealtime(0.0524288f);
		
		ButtonRenderer[0].material = BlackAndWhite[1];
		ButtonRenderer[1].material = BlackAndWhite[0];
		NumberLine.color = Color.black;
		NumberLine.text = "Decided";
		SAndC[0].text = "X"; SAndC[1].text = "O";
		yield return new WaitForSecondsRealtime(0.0524288f);
		
		ButtonRenderer[0].material = BlackAndWhite[0];
		ButtonRenderer[1].material = BlackAndWhite[1];
		NumberLine.color = Color.white;
		NumberLine.text = "My";
		SAndC[0].text = "O"; SAndC[1].text = "X";
		yield return new WaitForSecondsRealtime(0.04194304f);
		
		ButtonRenderer[0].material = BlackAndWhite[1];
		ButtonRenderer[1].material = BlackAndWhite[0];
		NumberLine.color = Color.black;
		NumberLine.text = "Final";
		SAndC[0].text = "X"; SAndC[1].text = "O";
		yield return new WaitForSecondsRealtime(0.04194304f);
		
		ButtonRenderer[0].material = BlackAndWhite[0];
		ButtonRenderer[1].material = BlackAndWhite[1];
		NumberLine.color = Color.white;
		NumberLine.text = "Decision";
		SAndC[0].text = "O"; SAndC[1].text = "X";
		yield return new WaitForSecondsRealtime(0.04194304f);
		
		ButtonRenderer[0].material = BlackAndWhite[1];
		ButtonRenderer[1].material = BlackAndWhite[0];
		NumberLine.color = Color.black;
		NumberLine.text = "Is";
		SAndC[0].text = "X"; SAndC[1].text = "O";
		yield return new WaitForSecondsRealtime(0.04194304f);


		if (Answer == Tables)
		{
			Module.HandlePass();
			Audio.PlaySoundAtTransform(SFX[2].name, transform);
			ButtonRenderer[0].material = BlackAndWhite[1];
			ButtonRenderer[1].material = BlackAndWhite[0];
			NumberLine.color = Color.black;
			NumberLine.text = "Peace";
			SAndC[0].text = ""; SAndC[1].text = "";
			Debug.LogFormat("[Negativity #{0}] The balanced was preserved. Module solved.", moduleId);
		}
		else
		{
			ButtonRenderer[0].material = BlackAndWhite[0];
			ButtonRenderer[1].material = BlackAndWhite[1];
			NumberLine.color = Color.white;
			NumberLine.text = "Chaos";
			SAndC[0].text = ""; SAndC[1].text = "";
			yield return new WaitForSecondsRealtime(0.5f);
			Module.HandleStrike();
			Debug.LogFormat("[Negativity #{0}] The balanced was destroyed. The balanced is being restored. A strike is given as a punishment.", moduleId);
			Playable = true; Totale = 0; KSop = 0; RotationsNumber = 0; SAndC[0].text = "C"; SAndC[1].text = "S"; 
			for (int x = 0; x < 9; x++)
			{
				TernaryFunctions[0][x] = 0;
			}
			NumberSheet();
		}
	}
	
	//twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} submit presses the submit button | !{0} clear presses the clear button | !{0} tick / !{0} silent will determine if the cycle produces a sound or not | !{0} [- or +] delivers the answer to the module (This command can be performed in a chain)";
    #pragma warning restore 414
	
	string[] Validity = {"+", "-"};
	
	IEnumerator ProcessTwitchCommand(string command)
	{
		string[] parameters = command.Split(' ');
		if (Regex.IsMatch(command, @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			yield return null;
			if (!Playable)
			{
				yield return "sendtochaterror Can not press the button since the button is not yet accessable.";
				yield break;
			}
			yield return "solve";
			yield return "strike";
			Buttons[1].OnInteract();
		}
		
		else if (Regex.IsMatch(command, @"^\s*clear\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			yield return null;
			if (!Playable)
			{
				yield return "sendtochaterror Can not press the button since the button is not yet accessable.";
				yield break;
			}
			
			if (KSop == 0)
			{
				yield return "sendtochaterror Can not clear text since the module is still cycling.";
				yield break;
			}
			
			Buttons[2].OnInteract();
		}
		
		else if (Regex.IsMatch(command, @"^\s*silent\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			yield return null;
			if (Silent == true)
			{
				yield return "sendtochaterror The cycle is already silent.";
				yield break;
			}
			Silent = true;
			yield return "sendtochat The cycle is now producing no sound.";
		}
		
		else if (Regex.IsMatch(command, @"^\s*tick\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			yield return null;
			if (Silent == false)
			{
				yield return "sendtochaterror The cycle is already ticking.";
				yield break;
			}
			Silent = false;
			yield return "sendtochat The cycle is now producing a ticking sound.";
		}
		
		else if (parameters[0].Contains('+') || parameters[0].Contains('-'))
		{
			yield return null;
			if (!Playable)
			{
				yield return "sendtochaterror Can not press the screen since the button is not yet accessable.";
				yield break;
			}
			
			if (KSop == 0)
			{
				yield return "sendtochaterror Can not toggle screen since the module is still cycling.";
				yield break;
			}
			
			foreach (char c in parameters[0])
			{
				if (!c.ToString().EqualsAny(Validity))
				{
					yield return "sendtochaterror The command being submitted contains a character that is not + or -";
					yield break;
				}
			}
			
			if (parameters[0].Length > 9 - Ternary.text.Length)
			{
				yield return "sendtochaterror The text being submitted will cause the display to go over 9 characters. Command was ignored.";
				yield break;
			}
			
			foreach (char c in parameters[0])
			{ 
				if (c.ToString() == "+")
				{
					while (Switcher != 1)
					{
						 yield return "trycancel The command to press the screen was halted due to a cancel request";
						 yield return new WaitForSeconds(0.01f);
					}
					Buttons[0].OnInteract();
					yield return new WaitForSeconds(0.1f);
				}
				
				else if (c.ToString() == "-")
				{
					while (Switcher != 0)
					{
						 yield return "trycancel The command to press the screen was halted due to a cancel request";
						 yield return new WaitForSeconds(0.01f);
					}
					Buttons[0].OnInteract();
					yield return new WaitForSeconds(0.1f);
				}
			}
		}
	}
}
