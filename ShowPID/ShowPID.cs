using System;
using System.Reflection;

using BepInEx;
using BepInEx.Hacknet;
using BepInEx.Logging;

using Hacknet;

using HarmonyLib;

using Mono.Cecil;
using Mono.Cecil.Cil;

using MonoMod.Cil;

using Pathfinder.Executable;

using Module = Hacknet.Module;

namespace PrincessRTFM.Hacknet.ShowPID;

[HarmonyPatch]
[BepInPlugin(GUID, NAME, VERSION)]
[BepInDependency(Pathfinder.PathfinderAPIPlugin.ModGUID)]
public class ShowPID: HacknetPlugin {
	public const string
		GUID = $"PrincessRTFM.{NAME}",
		NAME = "ShowPID",
		VERSION = "1.0.0";

	internal static ManualLogSource Logger { get; private set; } = null!;
	public override bool Load() {
		Logger = this.Log;
		Logger.LogInfo("Patching OS.addExe()");
		this.HarmonyInstance.PatchAll(this.GetType().Assembly);
		return true;
	}

	#region Harmony patching
	private static FieldInfo getField(Type type, string name) => AccessTools.Field(type, name) ?? throw new NullReferenceException($"failed to reflect field {type.FullName}.{name}");
	private static MethodInfo getMethod(Type type, string name, params Type[] args) => AccessTools.Method(type, name, args)
		?? throw new NullReferenceException($"failed to reflect method {type.FullName}.{name}{args.Description()}");

	[HarmonyILManipulator]
	[HarmonyPatch(typeof(OS), nameof(OS.addExe))]
	[HarmonyPatch(typeof(ExecutableManager), nameof(ExecutableManager.AddGameExecutable), [typeof(OS), typeof(GameExecutable)])]
	private static void injectPidDisplayOnVanillaExeAdd(ILContext context, MethodBase original) {
		if (original is null)
			return;

		try {
			ILCursor il = new(context);

			Type str = typeof(string);
			Type exe = typeof(ExeModule);
			Type custom = typeof(GameExecutable);

			FieldInfo exeIdentifierName = getField(exe, nameof(ExeModule.IdentifierName));
			FieldInfo exeName = getField(exe, nameof(ExeModule.name));
			FieldInfo exePid = getField(exe, nameof(ExeModule.PID));

			MethodInfo customInit = getMethod(custom, nameof(GameExecutable.OnInitialize));
			MethodInfo customCanAdd = getMethod(custom, $"get_{nameof(GameExecutable.CanAddToSystem)}");

			MethodInfo strConcat = getMethod(str, nameof(string.Concat), [str, str]);
			MethodInfo intToStr = getMethod(typeof(int), nameof(int.ToString), []);

			MethodInfo osWrite = getMethod(typeof(OS), nameof(OS.write), [str]);

			Func<Instruction, bool>[] instructionSetAddNewExe = [
				x => x.MatchLdarg(0), // loads OS instance
				x => x.MatchLdfld(typeof(OS), nameof(OS.exes)), // loads OS.exes field
				x => x.MatchLdarg(1), // loads the exe object
				x => x.MatchCallvirt(out MethodReference _), // the OS.exes.Add(exe) call is a mess
			];
			Func<Instruction, bool>[] predicates = original.DeclaringType == typeof(OS)
				? [
					x => x.MatchNop(),
					x => x.MatchLdarg(1), // loads ExeModule object
					x => x.MatchCallOrCallvirt(typeof(Module), nameof(Module.LoadContent)), // ilspy shows it's doing `callvirt instance void Hacknet.Module::LoadContent()` on the ExeModule object here
					x => x.MatchNop(),
					..instructionSetAddNewExe,
					x => x.MatchNop(),
				]
				: [
					x => x.MatchLdarg(1), // loads GameExecutable object
					x => x.MatchCallvirt(customInit), // calls GameExecutable.OnInitialize()
					x => x.MatchLdarg(1), // loads GameExecutable object again
					x => x.MatchCallvirt(customCanAdd), // calls GameExecutable.get_CanAddToSystem()
					x => x.MatchBrfalse(out ILLabel _),
					..instructionSetAddNewExe,
				];

			il.GotoNext(MoveType.Before, predicates);
			Logger.LogInfo($"Found target IL sequence ({predicates.Length} instructions) in {original.FullDescription()} at {il.Index:X4}");
			il.Index += predicates.Length;
			Logger.LogInfo($"Injecting patch at IL instruction {il.Index:X4}");

			ILLabel skipExeName = il.DefineLabel(); // the label used to branch past the exe name fallback if necessary
			il.Emit(OpCodes.Ldarg_0); // load OS instance
			il.Emit(OpCodes.Ldstr, "Started "); // first part of the message
			il.Emit(OpCodes.Ldarg_1); // load the ExeModule
			il.Emit(OpCodes.Ldfld, exeIdentifierName); // load ExeModule.IdentifierName from the exe
			il.Emit(OpCodes.Dup); // make a second copy for the null check
			il.Emit(OpCodes.Ldnull); // load a null reference to check against
			il.Emit(OpCodes.Ceq); // compare loaded value against null
			il.Emit(OpCodes.Brfalse, skipExeName); // if the value is NOT null, branch past the fallback
			il.Emit(OpCodes.Pop); // remove the null value that was saved before the check so we can replace it
			il.Emit(OpCodes.Ldarg_1); // load the ExeModule again
			il.Emit(OpCodes.Ldfld, exeName); // load ExeModule.name from the exe
			il.Emit(OpCodes.Nop); // branch here from the null check if ExeModule.IdentifierName was not null
			skipExeName.Target = il.Prev; // set the label target for the jump
			il.Emit(OpCodes.Call, strConcat); // concatenate the first part of the message (string constant) with the acquired name
			il.Emit(OpCodes.Ldstr, " ("); // load the next bit of constant text
			il.Emit(OpCodes.Call, strConcat); // concat with the message so far
			il.Emit(OpCodes.Ldarg_1); // load the ExeModule again
			il.Emit(OpCodes.Ldflda, exePid); // load the address of the ExeModule.PID field for the exe
			il.Emit(OpCodes.Call, intToStr); // convert the int PID to a string representation
			il.Emit(OpCodes.Call, strConcat); // stick that on the end of the message
			il.Emit(OpCodes.Ldstr, ")"); // load the last part of the message
			il.Emit(OpCodes.Call, strConcat); // append it
			il.Emit(OpCodes.Callvirt, osWrite); // invoke OS.write(string) on the OS instance loaded at the start of all this, passing the constructed message
		}
		catch (Exception e) {
			Logger.LogError($"Failed to patch OS.addExe(ExeModule): {e.GetType().Name}");
			Logger.LogError(e.Message);
			foreach (string line in e.StackTrace.Split('\n'))
				Logger.LogError(line);
			while (e.InnerException is Exception i) {
				Logger.LogError("----- CAUSED BY -----");
				Logger.LogError($"{i.GetType().Name}: {i.Message}");
				foreach (string line in i.StackTrace.Split('\n'))
					Logger.LogError(line);
				e = i;
			}
		}
	}

	#endregion
}
