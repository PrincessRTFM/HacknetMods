using System.Collections.Generic;
using System.Linq;

using Hacknet;

namespace PrincessRTFM.Hacknet.Foxnet.Commands;

internal class AddToAuthenticator: CommandBase {
	public override string[] Aliases { get; } = ["Authenticate", "Whitelist"];
	public override string Description { get; } = "Adds you to an authenticator IP list";
	public override string[] Arguments { get; } = ["target?"];

	public override void Execute(OS os, string cmd, string[] args) {
		Computer c = args.Length > 0
			? Programs.getComputer(os, args[0])
			: os.connectedComp;
		if (c is not null) {
			string me = os.thisComputer.ip;
			Folder app = c.files.root.searchForFolder("Whitelist");
			FileEntry? sourceFile = app?.searchForFile("source.txt");
			FileEntry? localList = app?.searchForFile("list.txt");
			string? target = sourceFile?.data?.Trim();

			if (localList is not null) {
				localList.data = string.Join("\n", new HashSet<string>(
					  $"{localList.data.Trim()}\n{me}"
						.Split('\n')
						.Where(s => !string.IsNullOrWhiteSpace(s))
				));
				Foxnet.Libsune.Terminal.Print($"Added to IP list");
				return;
			}

			if (!string.IsNullOrEmpty(target)) {
				Computer daemon = Programs.getComputer(os, target);
				Folder? remoteApp = daemon?.files?.root?.searchForFolder("Whitelist");
				FileEntry? remoteList = remoteApp?.searchForFile("list.txt");

				if (remoteList is not null) {
					remoteList.data = string.Join("\n", new HashSet<string>(
						  $"{remoteList.data.Trim()}\n{me}"
							.Split('\n')
							.Where(s => !string.IsNullOrWhiteSpace(s))
					));
					os.netMap.discoverNode(daemon);
					Foxnet.Libsune.Terminal.Print($"Identified authenticator {target}, added to IP list");
					Foxnet.PrintRandomSnark(os);
					return;
				}
			}

			Foxnet.Libsune.Terminal.Print("Couldn't find local or remote authenticator");
		}
		else {
			Foxnet.Libsune.Terminal.Print($"Can't find target computer {args[0]}");
		}
	}
}
