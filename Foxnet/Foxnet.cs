using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using BepInEx;
using BepInEx.Hacknet;
using BepInEx.Logging;

using Hacknet;

using Pathfinder.Command;

using PrincessRTFM.Hacknet.Lib;

namespace PrincessRTFM.Hacknet.Foxnet;

[BepInPlugin(GUID, NAME, VERSION)]
[BepInDependency(Pathfinder.PathfinderAPIPlugin.ModGUID)]
public class Foxnet: HacknetPlugin {
	public const string
		GUID = $"PrincessRTFM.{NAME}",
		NAME = "Foxnet",
		VERSION = "0.1.0";

	internal delegate void PluginCommandDelegate(OS os, string cmd, string[] args);

	internal static Dictionary<string, CommandBase> RegisteredCommands { get; private set; } = [];
	private static ManualLogSource logger { get; set; } = null!;
	internal static LibsuneHN Libsune { get; private set; } = null!;

	private static readonly Dictionary<string, string> exeFileCache = [];

	private static readonly string[] snark = [
		// WTNV
		"Not everything with a human face is human.",
		"Death is only the end if you assume the story is about you.",
		"Dream the impossible.\nLive the improbable.\nDie from the inevitable.",
		"Confused? Unsure what to do?\nSounds like you're human.\nGood luck.",
		"When life seems dangerous and unmanageable,\njust remember that it is,\nand that you can't survive forever.",
		"I will tell you an important secret.\nI will also tell you volumes of worthless nonsense.\nYou will never know which is which.",
		"Looking for a simple solution to all life's problems?\nWe are proud to present obstinate denial.\nAccept no substitute.\nAccept nothing.",
		// FFXIV (I shit you not)
		"If it is folly to hope,\nthen I am content to die a fool.",
		"I've sins aplenty, aye, but regrets?\nNot so much.",
		"When it comes, I shall welcome it with open arms.\nBut today will not be the day,\nand you will not be the judge.",
		"No more shall man have wings to bear him to paradise.\nHenceforth, he shall walk.",
		"Open your eyes to the darkness,\nand drown in its loveless embrace.\nThe gods will not be watching.",
		"We may accept our fate or run from it,\nbut we cannot deny it.",
		"We whom gods and men have forsaken,\nshall be the instruments of our own deliverance.",
		// Generic badass
		"It's not a matter of who will let me.\nIt's a question of who can stop me.",
		"Bury me shallow.\nI'll be back.",
		"I am a monument to all your sins.",
		"This is hell's territory,\nand I am beholden to no gods.",
		// Raw quotes (mostly from tumblr lmao)
		"There is no light at the end of the tunnel,\nso it's a good thing we brought matches.",
		"If the truth doesn't save us,\nwhat does that say about us?",
		"To become god is the lonliest achievement of all.",
		"When tyranny becomes law,\nresistance becomes duty.",
		"I intend to put up with nothing that I can put down.",
		"I have seen your ideas of justice,\nand I have found them wanting.",
		// Cool quotes
		"Not all those who wander are lost.",
		// Inspirational (sorta)
		"The dead cannot cry out for justice;\nit is a duty of the living to do so for them.",
		"The answer to despair is action.",
		"When you choose an action,\nyou choose the consequences of that action.\n\nChoose well.",
		// Snarky shit
		"Lead me not into temptation,\nfor I can find it myself.",
		// Paranoia (TTRPG)
		"Friend Computer thanks you for your service.",
		"Mine is the last voice you will ever hear.\nDo not be alarmed.",
		"Have you taken your happiness pills today, Citizen?\nHappiness is mandatory!",
	];

	public static string[] Snark => snark[Utils.random.Next(0, snark.Length)].Split(Utils.newlineDelim).Select(l => $"// {l}").ToArray();

	internal static void Fatal(string msg) => logger.LogFatal(msg);
	internal static void Error(string msg) => logger.LogError(msg);
	internal static void Warn(string msg) => logger.LogWarning(msg);
	internal static void Print(string msg) => logger.LogMessage(msg);
	internal static void Info(string msg) => logger.LogInfo(msg);
	internal static void Debug(string msg) => logger.LogDebug(msg);

	public override bool Load() {
		logger = this.Log;
		Libsune = new(this);
		Libsune.Terminal.OutputPrefix = "OuO >>";

		Info("Applying harmony patches");
		this.HarmonyInstance.PatchAll(this.GetType().Assembly);

		Type cmdBase = typeof(CommandBase);
		Assembly asm = cmdBase.Assembly;
		CommandBase[] commands = asm
			.GetTypes()
			.Where(t => cmdBase.IsAssignableFrom(t) && !t.IsAbstract)
			.Select(t => Activator.CreateInstance(t))
			.Cast<CommandBase>()
			.ToArray();

		Info($"Registering {commands.Length} custom game commands");
		foreach (CommandBase cmd in commands) {
			//Console.WriteLine($"Registering custom command {cmd.Command} from {cmd.GetType().Name}");
			RegisteredCommands[cmd.GetType().Name] = cmd;
			try {
				CommandManager.RegisterCommand(cmd.Command, cmd.RedirectHacknetInvocation);
				RegisteredCommands[cmd.Command] = cmd;
			}
			catch (NotImplementedException ex) {
				Warn($"Ignoring {ex.GetType().Name} ({ex.Message}) and crossing our fingers...");
			}

			string[] alts = cmd.Aliases;
			if (alts.Length > 0) {
				foreach (string alt in alts) {
					//Console.WriteLine($"Registering command alias {alt}");
					try {
						CommandManager.RegisterCommand(alt, cmd.RedirectHacknetInvocation);
						RegisteredCommands[alt.ToLower()] = cmd;
					}
					catch (NotImplementedException ex) {
						Warn($"Ignoring {ex.GetType().Name} ({ex.Message}) and crossing our fingers...");
					}
				}
			}
		}

		Info("Finished registering custom game commands");
		return true;
	}
	public override bool Unload() {
		logger = null!;
		return base.Unload();
	}

	public static string GetMagicFromExeName(string fileName) {
		string wanted = fileName.ToLower().Replace(".exe", "");

		if (exeFileCache.TryGetValue(wanted, out string magic))
			return magic;

		foreach (int exeNum in PortExploits.exeNums) {
			if (PortExploits.cracks[exeNum].ToLower().Replace(".exe", "") == wanted)
				return exeFileCache[wanted] = PortExploits.crackExeData[exeNum];
		}

		return exeFileCache[wanted] = null!;
	}

	public static void PrintRandomSnark(OS os, bool includeNewlinePrefix = true) {
		if (includeNewlinePrefix)
			os.write("\n");
		Foxnet.Libsune.Terminal.Print(Snark);
		os.write("\n");
	}
}
