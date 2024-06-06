using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using HarmonyLib;

using Pathfinder.Executable;

namespace PrincessRTFM.Hacknet.Foxnet.Util;

public record class RegisteredCustomExe {
	private const string
		CEI_FIELD_ID = "XmlId",
		CEI_FIELD_TYPE = "ExeType",
		CEI_FIELD_CONTENT = "ExeData",
		CEMGR_LIST_NAME = "CustomExes";

	private static Type? ceType;
	private static FieldInfo? fId;
	private static FieldInfo? fType;
	private static FieldInfo? fContent;

	public string XmlId { get; private set; }
	public string QualifiedTypeName { get; private set; }
	public string Magic { get; private set; }

	private RegisteredCustomExe(string id, string type, string content) {
		this.XmlId = id;
		this.QualifiedTypeName = type;
		this.Magic = content;
	}
	private RegisteredCustomExe(string id, Type type, string content) : this(id, type.FullName, content) { }

	internal static RegisteredCustomExe[] GetCustomExes() {
		// shoutout to Aaron on the Hacknet_Modding discord for this method
		IList<object> registered = (IList<object>)AccessTools.DeclaredField(typeof(ExecutableManager), CEMGR_LIST_NAME);
		if (registered.Count == 0)
			return [];

		ceType ??= registered[0].GetType();
		fId ??= AccessTools.DeclaredField(ceType, CEI_FIELD_ID);
		fType ??= AccessTools.DeclaredField(ceType, CEI_FIELD_TYPE);
		fContent ??= AccessTools.DeclaredField(ceType, CEI_FIELD_CONTENT);

		return registered
			.Select(o => new RegisteredCustomExe((string)fId.GetValue(o), (Type)fType.GetValue(o), (string)fContent.GetValue(o)))
			.ToArray();
	}

}
